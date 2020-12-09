// Make sure this file is included in memcached.c before coyote_mc_wrapper.h file is included
#ifndef COYOTE_MC_WRAP

#include <include_coyote/include/coyote_mc_wrapper.h>
#include <pthread.h>
#include <stdlib.h>
#include <arpa/inet.h>
#include <string.h>
#include <unistd.h>
#include <stdio.h>

// static long long unsigned sfd_counter = 1;

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

	((param->start_routine))(param->arg);

	FFI_complete_operation((long unsigned)pthread_self());

	return NULL;
}

// Call our wrapper function instead of original parameters to pthread_create
int FFI_pthread_create(void *tid, void *attr, void *(*start_routine) (void *), void* arguments){

	// This assert can trigger in create_worker() becoz they use non-null attributes,
	// though they use only defaults attributes
	// assert(attr == NULL && "We don't yet support using custom attributes");

	pthread_c_params *p = (pthread_c_params *)malloc(sizeof(pthread_c_params));
	p->start_routine = start_routine;
	p->arg = arguments;

	return pthread_create(tid, attr, coyote_new_thread_wrapper, (void*)p);
}

int FFI_pthread_join(pthread_t tid, void* arg){

	FFI_join_operation((long unsigned) tid); // This is a machine & OS specific hack
	return pthread_join(tid, arg);
}

int FFI_accept(int sfd, void* addr, void* addrlen){
	FFI_schedule_next();
	// Return a new sfd! We can return the same sfd also if we want to perform operatoion on an existing connection
	// Always return 2, this is a hack to check if the application is trying to write on a socket
	return pthread_self();
}

// Dummy connection!
int FFI_getpeername(int sfd, void* addr, void* addrlen){

	struct sockaddr_in6 *sockaddr = (struct sockaddr_in6 *)addr;
	sockaddr->sin6_family = AF_INET;
    sockaddr->sin6_port = 8080;
    inet_pton(AF_INET, "192.0.2.33", &(sockaddr->sin6_addr));

    return 0;
}

ssize_t FFI_write(int sfd, const void* buff, size_t count){

	ssize_t retval = FFI_event_write(sfd, buff, count);

	// Check if you are tying to signal any event
	if(retval >= 0){

		return retval;
	} else {

		// Means that you are trying to write to a socket
		if(sfd == 2){

			assert(0 && "Missing implementation");
			return -1;
		} else {

			// You are trying to write to an actual file
			return write(sfd, buff, count);
		}
	}
}

ssize_t FFI_sendmsg(int sfd, struct msghdr *msg, int flags){

    assert(strcmp((msg->msg_iov->iov_base), "STORED\r\n") == 0);
    printf("Recieved msg on socket: STORED\n");

    return 0;
}

ssize_t FFI_read(int fd, void* buff, int count){


	if(fd != 2){

		// You are trying to read from an event fd
		return read(fd, buff, count);
	} else {

		// You are tyring to read from the socket
		char str[40] = "stats cachedump 0 0\r\n";
		memcpy(buff, str, strlen(str));
		return strlen(str);
	}
}

ssize_t FFI_recvfrom(int socket, void* buffer, size_t length,
       int flags, struct sockaddr* address,
       socklen_t* address_len){

	assert(0 && "Not implemented");
	return 1;
}

int main(int argc, char **argv){

	FFI_create_scheduler();

	for(int j = 0; j < 10; j++){

		printf("Starting iteration #%d \n", j);

		FFI_attach_scheduler();

		run_coyote_iteration(argc, argv);

		FFI_detach_scheduler();
		FFI_scheduler_assert();
	}

	FFI_delete_scheduler();

	return 0;
}

#endif /* COYOTE_MC_WRAP */