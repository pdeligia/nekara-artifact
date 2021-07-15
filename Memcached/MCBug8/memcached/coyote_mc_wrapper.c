// Make sure this file is included in memcached.c before coyote_mc_wrapper.h file is included
#ifndef COYOTE_MC_WRAP

#include <include_coyote/include/coyote_mc_wrapper.h>
#include <pthread.h>
#include <stdlib.h>
#include <arpa/inet.h>
#include <string.h>
#include <unistd.h>
#include <stdio.h>
#include <signal.h>
#include <math.h>
#include <mcheck.h>

#define EXECUTION_COYOTE_CONTROLLED

#ifndef EXECUTION_COYOTE_CONTROLLED
#define FFI_schedule_next()
#endif
// Declarations of test methods. These methods should be implemented by the test case.
bool CT_is_socket(int);
ssize_t CT_socket_read(int, const void*, int);
ssize_t CT_socket_write(int,  void*, int);
ssize_t CT_socket_recvmsg(int, struct msghdr*, int);
int CT_new_socket();
ssize_t CT_socket_sendto(int, void*, size_t, int, struct sockaddr*,
       socklen_t*);
uint64_t get_operation_seq_hash();

// Main function of the test case
int CT_main( int (*run_iteration)(int, char**), void (*reset_gloabs)(void), uint64_t (*get_prog_state)(void), int argc, char** argv );

// Temporary data structure used for passing parameteres to pthread_create
typedef struct pthread_create_params{
	void *(*start_routine) (void *);
	void* arg;
} pthread_c_params;

// This function will be called in pthread_create
void *coyote_new_thread_wrapper(void *p){

	FFI_create_operation((long unsigned)pthread_self());
	FFI_start_operation((long unsigned)pthread_self());

	pthread_c_params* param = (pthread_c_params*)p;

	FFI_schedule_next();
	((param->start_routine))(param->arg);

	FFI_complete_operation((long unsigned)pthread_self());

	return NULL;
}

// Call our wrapper function instead of original parameters to pthread_create
int FFI_pthread_create(void *tid, void *attr, void *(*start_routine) (void *), void* arguments){

	// This assert can trigger in create_worker() becoz they use non-null attributes,
	// though they use only defaults attributes
	// assert(attr == NULL && "We don't yet support using custom attributes");

	FFI_schedule_next();
	pthread_c_params *p = (pthread_c_params *)malloc(sizeof(pthread_c_params));
	p->start_routine = start_routine;
	p->arg = arguments;

	return pthread_create(tid, attr, coyote_new_thread_wrapper, (void*)p);
}

static bool stats_state_read = true;
static bool stats_state_write = true;

void FFI_check_stats_data_race(bool isWrite){

	static int num_readers = 0;

	if(isWrite){

		assert(stats_state_write == true); // Make sure no one is reading this
		stats_state_write = false;
		stats_state_read = false;

		FFI_schedule_next();

		stats_state_write = true;
		stats_state_read  = true;

	} else{

		assert(stats_state_read == true);
		stats_state_write = false;
		num_readers++;

		FFI_schedule_next();

		num_readers--;
		// If all the readers have finished reading
		if(num_readers == 0){
			stats_state_write = true;
		}
	}
}

int FFI_pthread_join(pthread_t tid, void* arg){

	FFI_join_operation((long unsigned) tid); // This is a machine & OS specific hack
	return pthread_join(tid, arg);
}

static int *stop_main = NULL;
void FFI_register_main_stop(int *flag){
	stop_main = flag;
}

int FFI_accept(int sfd, void* addr, void* addrlen){

#ifdef EXECUTION_COYOTE_CONTROLLED
	FFI_schedule_next();
#endif
	int retval = CT_new_socket();

	// retval == 0 means no new connection available
	while(retval == 0){
		FFI_clock_handler();

#ifdef EXECUTION_COYOTE_CONTROLLED
		FFI_schedule_next();
#endif
		retval = CT_new_socket();
	}

	// If the test is returning -1, means it wants to stop the server
	if(retval < 0){
		*stop_main = 2;
		return retval;
	}

	assert(retval >= 200 && "Please use fds > 200 as others are reserved");
	return retval;
}

// Dummy connection!
int FFI_getpeername(int sfd, void* addr, void* addrlen){

#ifdef EXECUTION_COYOTE_CONTROLLED
	FFI_schedule_next();
#endif
	struct sockaddr_in6 *sockaddr = (struct sockaddr_in6 *)addr;
	sockaddr->sin6_family = AF_INET;
    sockaddr->sin6_port = 8080;
    inet_pton(AF_INET, "192.0.2.33", &(sockaddr->sin6_addr));

    return 0;
}

// 1-D array containing FDs of both the ends of a pipe
int *global_pipes = NULL;
// Used for closing all the open pipes
static int FFI_pipe_max = 0;

int FFI_pipe(int pipes[2]){

	int retval = pipe(pipes);
	FFI_schedule_next();

	if(global_pipes == NULL){
		// There can be at max 1000 pipes
		global_pipes = (int*)malloc(sizeof(int)*1000);

		// Set default values
		memset(global_pipes, -1, 1000);
		FFI_pipe_max = 0;
	}

	assert(global_pipes[ pipes[1] ] == -1 && "2 pipes with same fds?");
	global_pipes[ pipes[1] ] = pipes[0];

	// Store the max of FFI_pipe_max and pipes[1]
	FFI_pipe_max = (FFI_pipe_max >= pipes[1])?FFI_pipe_max:pipes[1];

	return retval;
}

int FFI_close(int fd){

	// Don't put a schedule_next here! This function will be called after detaching the client
	// FFI_schedule_next();
	return close(fd);
}

// No need to wait on a socket
int FFI_poll(struct pollfd *fds, nfds_t nfds, int timeout){

	FFI_schedule_next();
	fds->revents = POLLOUT;

	return 1;
}

ssize_t FFI_write(int sfd, const void* buff, size_t count){

	FFI_clock_handler();
	ssize_t retval = -1;
	FFI_schedule_next();

	// If you are tying to write to a pipe
	if(global_pipes[sfd] != -1){

		retval = FFI_event_write(sfd, buff, count, global_pipes[sfd]);
	}
	else{

		retval = FFI_event_write(sfd, buff, count, -1);
	}

	// Check if you are tying to signal any event
	if(retval >= 0){

		return retval;
	}
	else {

		// Means that you are trying to write to a socket
		if(CT_is_socket(sfd)){

			return CT_socket_read(sfd, buff, count);
		}
		else {

			// You are trying to write to an actual file
			return write(sfd, buff, count);
		}
	}
}

ssize_t FFI_sendmsg(int sfd, struct msghdr *msg, int flags){

	FFI_schedule_next();
    return CT_socket_recvmsg(sfd, msg, flags);
}

int FFI_fcntl(int fd, int cmd, ...){

	FFI_schedule_next();
	return 1;
}

ssize_t FFI_read(int fd, void* buff, int count){

	FFI_schedule_next();
	if(!CT_is_socket(fd)){

		// You are trying to read from an event fd
		return read(fd, buff, count);
	} else {

// Try to simulate a random delay in every incoming network client
#ifdef EXECUTION_COYOTE_CONTROLLED

	//int delay = FFI_next_integer(5) % 5;

	//for(int i = 0; i < delay; i++){
	//	FFI_schedule_next();
	//}
#endif

		return CT_socket_write(fd, buff, count);
	}
}

ssize_t FFI_recvfrom(int socket, void* buffer, size_t length,
       int flags, struct sockaddr* address,
       socklen_t* addr_len){

	FFI_schedule_next();
	return CT_socket_sendto(socket, buffer, length, flags, address, addr_len);
}

#ifdef CATCH_INTERMEDIATE_STATES

/*For storing sequence of set, delete and not found operations*/
enum type_of_op {SET = 1, DEL = 2, NF = 3, PREPEND = 4};

struct op{
	enum type_of_op type;
	char* key_name;
	int size_of_key;
};

void* get_new_operation(enum type_of_op _type, const char* _key_name, int size);

void* get_new_operation(enum type_of_op _type, const char* _key_name, int size){

	struct op *new_opr = (struct op*)FFI_malloc(sizeof(struct op));
	new_opr->type = _type;
	new_opr->key_name = (char*)FFI_malloc(size+1);
	memcpy(new_opr->key_name, _key_name, size);
	new_opr->size_of_key = size;

	return new_opr;
}

void **operation_vector = NULL;
int operation_vector_size = 0;
// Don't do a global initialization
pthread_mutex_t operation_vector_lock;

#ifdef EXECUTION_COYOTE_CONTROLLED
	#define VECTOR_INIT() FFI_pthread_mutex_init(&operation_vector_lock, NULL)
	#define VECTOR_LOCK() FFI_pthread_mutex_lock(&operation_vector_lock)
	#define VECTOR_UNLOCK() FFI_pthread_mutex_unlock(&operation_vector_lock)
	#define VECTOR_DESTROY() FFI_pthread_mutex_destroy(&operation_vector_lock)
#else
	#define VECTOR_INIT() pthread_mutex_init(&operation_vector_lock, NULL)
	#define VECTOR_LOCK() pthread_mutex_lock(&operation_vector_lock)
	#define VECTOR_UNLOCK() pthread_mutex_unlock(&operation_vector_lock)
	#define VECTOR_DESTROY() pthread_mutex_destroy(&operation_vector_lock)
#endif

#endif

#pragma GCC diagnostic ignored "-Wmissing-prototypes"
void check_and_init_opr_vector(){

#ifdef CATCH_INTERMEDIATE_STATES

	if(operation_vector != NULL) return;

	operation_vector = FFI_malloc(4096);
	memset(operation_vector, '1', 4096);

	// Make stdout unbuffered - important for using printfs in multi-threaded applications
	setvbuf(stdout, NULL, _IONBF, 0);

	operation_vector_size  = 0;

	VECTOR_INIT();

#endif
}

uint64_t get_operation_seq_hash(){

	uint64_t retval = 0;

#ifdef CATCH_INTERMEDIATE_STATES

	struct op *temp_op;

	for(int i = 0; i < operation_vector_size; i++){

		temp_op = (struct op*)operation_vector[i];

		char* key = temp_op->key_name;
		int key_size = temp_op->size_of_key;

		// Compute the key hash
		uint64_t key_hash = 0;
		for(int j = 0; j < key_size; j++){

			key_hash = ((long long unsigned)(key_hash + key[j]*pow(3, j))) % (1ULL << 60);
		}

		retval = ((long long unsigned)(retval + (key_hash * ((long long unsigned)(temp_op->type)) * (1ULL << (i+1)) ))) % (1ULL << 60);
	}

#else

	assert(0 && "Catching intermediate states is disabled. Enable it to use this functionality.");
#endif

	return retval;
}

#pragma GCC diagnostic ignored "-Wmissing-prototypes"
void reset_oper_vector(){

#ifdef CATCH_INTERMEDIATE_STATES

	assert(operation_vector != NULL);
	for(int i = 0; i < operation_vector_size; i++){

		void *temp_op = operation_vector[i];
		assert(temp_op != NULL);

		FFI_free(((struct op*)temp_op)->key_name);
		((struct op*)temp_op)->key_name = NULL;

		FFI_free(temp_op);
		temp_op = NULL;
	}

	FFI_free(operation_vector);
	operation_vector = NULL;

	operation_vector_size = 0;

	VECTOR_DESTROY();

#endif
}

void FFI_register_not_found(const char* key, int size){

#ifdef CATCH_INTERMEDIATE_STATES

	check_and_init_opr_vector();

	struct op *new_operation = (struct op *)get_new_operation(NF, key, size);

	VECTOR_LOCK();
	fprintf(stdout, "Thread with id: %lu, register the operation: %s, on key: %s\n", pthread_self(), "Not found", key);
	operation_vector[operation_vector_size++] = (void*)new_operation;
	VECTOR_UNLOCK();

#endif
}

void FFI_register_set(const char* key, int size){

#ifdef CATCH_INTERMEDIATE_STATES

	check_and_init_opr_vector();

	struct op *new_operation = (struct op *)get_new_operation(SET, key, size);

	VECTOR_LOCK();
	fprintf(stdout, "Thread with id: %lu, register the operation: %s, on key: %s\n", pthread_self(), "SET", key);
	operation_vector[operation_vector_size++] = (void*)new_operation;
	VECTOR_UNLOCK();

#endif
}

void FFI_register_delete(const char* key, int size){

#ifdef CATCH_INTERMEDIATE_STATES

	check_and_init_opr_vector();

	struct op *new_operation = (struct op *)get_new_operation(DEL, key, size);

	VECTOR_LOCK();
	fprintf(stdout, "Thread with id: %lu, register the operation: %s, on key: %s\n", pthread_self(), "Delete", key);
	operation_vector[operation_vector_size++] = (void*)new_operation;
	VECTOR_UNLOCK();

#endif
}

void FFI_register_prepend(const char* key, int size){

#ifdef CATCH_INTERMEDIATE_STATES

	check_and_init_opr_vector();

	struct op *new_operation = (struct op *)get_new_operation(PREPEND, key, size);

	VECTOR_LOCK();
	fprintf(stdout, "Thread with id: %lu, register the operation: %s, on key: %s\n", pthread_self(), "Prepend", key);
	operation_vector[operation_vector_size++] = (void*)new_operation;
	VECTOR_UNLOCK();

#endif
}

void FFI_reset_coyote_mc_wrapper(void){

	// Close all the open pipes
	for(int i = 0; i <= FFI_pipe_max; i++){

		if(global_pipes[i] == -1) continue;

		FFI_close(global_pipes[i]);
		FFI_close(i);
	}

	free(global_pipes);
	global_pipes = NULL;
	FFI_pipe_max = 0;

	stats_state_read = true;
	stats_state_write = true;

	reset_oper_vector();
}

uint64_t get_program_state(void){

	//return (FFI_assoc_hash() + get_slab_hash() + get_lru_hash()) % (1ULL<<60);
	//return (FFI_assoc_hash(2) * get_lru_hash()) % (1ULL<<60);
	//return (FFI_assoc_hash(1) + get_slab_hash() + get_operation_seq_hash()) % (1ULL<<60);
	//return (FFI_assoc_hash(1) + get_slab_hash()) % (1ULL<<60);
	return (FFI_assoc_hash(0)) % (1ULL<<60);
}

void reset_all_globals(){

	reset_logger_globals();
	reset_memcached_globals();
	reset_thread_globals();
	reset_assoc_globals();
	reset_crawler_globals();
	reset_items_globals();
	reset_slabs_globals();
	// For resetting libevent mock DS
	FFI_event_reset_all();
	// Now, reset the globals in coyote_mc_wrapper
	FFI_reset_coyote_mc_wrapper();
}

// Delegate the functionality to the test main method
int main(int argc, char **argv){

	mcheck(0);
 	return CT_main( &run_coyote_iteration, &reset_all_globals, &get_program_state, argc, argv );
}

#endif /* COYOTE_MC_WRAP */