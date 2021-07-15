/* Foreign function interface for Coyote scheduler
*  APIs for C language.
*  Most of the functions defined here just delegates the functionality to Coyote Scheduler APIs so refer to its documentation
*  for understanding the working of each function.
*  Before compiling make sure that you have LD_LIBRARY_PATH environment variable set and pointing to the
*  location at which coyote.so file is located.
*  Compile it as DLL using: g++ -std=c++11 -shared -fPIC -I../../../include/ -o libcoyote_c_ffi.so coyote_c_ffi.cpp -lcoyote -L../../../build/
*  For static library use: g++ -std=c++11 -c -I../../../include/ -o libcoyote_c_ffi.o coyote_c_ffi.cpp -lcoyote -L../../../build/ ; ar rvs libcoyote_c_ffi.a libcoyote_c_ffi.o
*  @author: Udit Kumar Agarwal <t-uagarwal@microsoft.com>
*/

//#define COYOTE_DEBUG_LOG 1
#include "test.h"
#include <cassert>
#include <climits>

// Require C++11
#include <unordered_map>
#include <vector>

typedef unsigned long long llu;

Scheduler* scheduler = NULL;

// Use this flag to enable printf statements in functions modelling pthread APIs
// #define DEBUG_PTHREAD_API 1

// Use this flag to use PCT branch of Coyote Scheduler
//#define USING_PCT_BRANCH 1

/******************************************** CoyoteLock Start ******************************************/

/* This class is intended to model a pthread mutex or a condition variable (condV)
*  using Coyote resources. For every mutrex or condV, we should have a unique
*  Coyote resource ID.
*/
class CoyoteLock{
public:

	// Boolean variable to store whether this resource is locked or not
	bool is_locked;
	// Unique Coyote resource id
	int coyote_resource_id;
	// Counter to keep a track of resource IDs, we have already allocated.
	// We won't be using the same resource ID again, even if the previous
	// resource is deleted.
	static int total_resource_count;
	// Is it a conditional variable?
	bool is_cond_var;
	// Vector of operations waiting for this conditional variable
	std::vector<size_t>* waitingOps;
	// Who is holding this lock?
	size_t user_op_id;

	// reserved_resource_id_min is used to tell CoyoteLock that there are already
	// existing coyote resources with IDs less than or equal to reserved_resource_id_min.
	// Use it when your application is moduler and you want to reserve some resource_ids for
	// one module.
	CoyoteLock(int reserved_resource_id_min = -1, int reserved_resource_id_max = INT_MAX, bool is_conditional_var = false){
		assert(scheduler != NULL && "CoyoteLock: please initialize the coyote scheduler first!\n");

		assert(total_resource_count < reserved_resource_id_max && "CoyoteLock: Can not allocate more resources!");

		// Should only be true once per module
		if(total_resource_count <= reserved_resource_id_min){
			total_resource_count = reserved_resource_id_min + 1;
		}

		coyote_resource_id = total_resource_count;
		total_resource_count ++;

		ErrorCode e = scheduler->create_resource(coyote_resource_id);
		assert(e == coyote::ErrorCode::Success && "CoyoteLock: failed to create resource! perhaps it already exists\n");

		is_locked = false;

		// if it is a conditional variable
		if(is_conditional_var){

			is_cond_var = true;
			waitingOps  = new std::vector<size_t>();
			assert(waitingOps != NULL && "CoyoteLock: Unable to allocate on heap!");
		} else {

			// If it is a mutex
			is_cond_var = false;
			waitingOps = NULL;
		}
		user_op_id = 0; // Held by main thread
	}

	~CoyoteLock(){
		assert(scheduler != NULL && "~CoyoteLock: please initialize the coyote scheduler first!\n");

		if(is_cond_var && (waitingOps != NULL) ){

			assert(waitingOps->empty() &&
				"Some operations are still waiting to be signaled!" &&
				 "Is it valid to destroy a cond_var when operations are waiting on it? No!");

			// If there is no one waiting on this conditional variable, then it is okay to delete it.
			assert( (is_locked == false || waitingOps->empty()) && "Can not delete the resource as it is locked!");

			delete waitingOps;
		} else {

			assert( (is_locked == false) && "Can not delete the resource as it is locked!");
		}

		ErrorCode e = scheduler->delete_resource(coyote_resource_id);
		assert(e == coyote::ErrorCode::Success && "~CoyoteLock: failed to delete resource!\n");
	}

	// Call with caution!! Currently, it is being called only during coyote_scheduler->detach()
	static void reset_resource_count(){
		total_resource_count = 0;
	}
};

int CoyoteLock::total_resource_count = 0;

// Global hash map to store which pointer corresponds to which CoyoteLock object
std::unordered_map<llu, CoyoteLock*>* hash_map = NULL;

// List to keep a track of all statically allocated global mutexes and initialize them if needed
std::vector<void *>* lazy_mutex_init_list = NULL;

// List to keep a track of all statically allocated global conditional variable and initialize them if needed
std::vector<void *>* lazy_cond_init_list = NULL;

/************************************* For checking liveness property *******************************/

// Memcached can be in the following 3 states 
enum program_state{STATE_READ, STATE_WRITE, STATE_INIT};
enum program_state curr_state = STATE_INIT;

// Number of 'attempted' context switches in one program state
llu num_cxt_switch = 0;

// Maximum number of context switches that can happen *without* changing the program state
#define MAX_NUM_CXT_SWITCH 20000

/******************************************** CoyoteLock End ******************************************/

/* Since these functions will be called from a C code, we
* need to specifiy this to our C++ compiler (g++) so that it accordingly
* adjust name mangling. In C, we don't need name mangling at all
* becasue there is no function overloading. In C++, we do need it.
* In short, this is to make C++ DLL compatible with C programs.
*/
extern "C"{

void clean_coyote_ops_hash_map();

void FFI_create_scheduler(){

	// Assuming that we can have only one instance
	// of Coyote scheduler
	if(scheduler != NULL){
		return;
	}

	scheduler = new coyote::Scheduler();
	assert(scheduler != NULL && "coyote::Scheduler() returned NULL!");
}

// Create scheduler with the seed
void FFI_create_scheduler_w_seed(size_t seed){

	// Udit: Assuming that we can have only one instance
	// of Coyote scheduler
	if(scheduler != NULL){
		return;
	}

	scheduler = new coyote::Scheduler(seed);
	assert(scheduler != NULL && "coyote::Scheduler() returned NULL!");
}

#ifdef USING_PCT_BRANCH

// Create scheduler with the random strategy
void FFI_create_scheduler_rand(){

	// Udit: Assuming that we can have only one instance
	// of Coyote scheduler
	if(scheduler != NULL){
		return;
	}

	std::string st = "RandomStrategy";
	scheduler = new coyote::Scheduler(st);
	assert(scheduler != NULL && "coyote::Scheduler() returned NULL!");
}

// Create scheduler with the pct strategy
void FFI_create_scheduler_pct(){

	// Udit: Assuming that we can have only one instance
	// of Coyote scheduler
	if(scheduler != NULL){
		return;
	}

	std::string st = "PCTStrategy";
	scheduler = new coyote::Scheduler(st);
	assert(scheduler != NULL && "coyote::Scheduler() returned NULL!");
}

#endif // USING_PCT_BRANCH

void FFI_delete_scheduler(){

	if(lazy_mutex_init_list != NULL){
		delete lazy_mutex_init_list;
		lazy_mutex_init_list = NULL;
	}

	if(lazy_cond_init_list != NULL){
		delete lazy_cond_init_list;
		lazy_cond_init_list = NULL;
	}

	// Make sure the scheduler pointer is not NULL
	if(scheduler == NULL){
		return;
	}

	delete scheduler;
}

void FFI_attach_scheduler(){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->attach();
	assert(e == coyote::ErrorCode::Success && "FFI_attach_scheduler: attach failed");
}

void FFI_detach_scheduler(){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	// If hash_map is non-null, clear and destroy it!
	if(hash_map != NULL){

		// Delete all the resources present in the hash map
		for(auto it = hash_map->begin(); it != hash_map->end(); it ++){

			CoyoteLock* obj = (*it).second;
			delete obj;
			obj = NULL;
		}

		hash_map->clear();

		delete hash_map;
		hash_map = NULL;

		CoyoteLock::reset_resource_count();
	}

	//clean_coyote_ops_hash_map();

	ErrorCode e = scheduler->detach();
	assert(e == coyote::ErrorCode::Success && "FFI_detach_scheduler: detach failed");
}

void FFI_scheduler_assert(){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");
	coyote_sch_assert(scheduler->error_code(), ErrorCode::Success);
}

void FFI_create_operation(size_t id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->create_operation(id);
	assert(e == coyote::ErrorCode::Success && "FFI_create_operation: failed");
}

void FFI_start_operation(size_t id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->start_operation(id);
	assert(e == coyote::ErrorCode::Success && "FFI_start_operation: failed");
}

void FFI_join_operation(size_t id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->join_operation(id);
	assert(e == coyote::ErrorCode::Success && "FFI_join_operation: failed");
}

void FFI_join_operations(const size_t* operation_ids, size_t size, bool wait_all){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->join_operations(operation_ids, size, wait_all);
	assert(e == coyote::ErrorCode::Success && "FFI_join_operations: failed");
}

void FFI_complete_operation(size_t id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->complete_operation(id);
	assert(e == coyote::ErrorCode::Success && "FFI_complete_operation: failed");
}

void FFI_create_resource(size_t id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->create_resource(id);
	assert(e == coyote::ErrorCode::Success && "FFI_create_resource: failed");
}

void FFI_wait_resource(size_t id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->wait_resource(id);
	assert(e == coyote::ErrorCode::Success && "FFI_wait_resource: failed");
}

void FFT_wait_resources(const size_t* resource_ids, size_t size, bool wait_all){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->wait_resources(resource_ids, size, wait_all);
	assert(e == coyote::ErrorCode::Success && "FFT_wait_resources: failed");
}

void FFI_signal_resource(size_t id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->signal_resource(id);
	assert(e == coyote::ErrorCode::Success && "FFI_signal_resource: failed");
}

// Signal resource availability to a specific operation, op_id
void FFI_signal_resource_to_op(size_t id, size_t op_id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->signal_resource(id, op_id);
	assert(e == coyote::ErrorCode::Success && "FFI_signal_resource_to_op: failed");
}

void FFI_delete_resource(size_t id){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->delete_resource(id);
	assert(e == coyote::ErrorCode::Success && "FFI_delete_resource: failed");
}

void FFI_schedule_next(){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	num_cxt_switch++;
//	assert(num_cxt_switch < MAX_NUM_CXT_SWITCH && "Potential violation of the liveness property.");

	ErrorCode e = scheduler->schedule_next();
	assert(e == coyote::ErrorCode::Success && "FFI_schedule_next: failed");
}

bool FFI_next_boolean(){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	bool val = scheduler->next_boolean();
	return val;
}

size_t FFI_next_integer(size_t max_value){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	return scheduler->next_integer(max_value);
}

size_t FFI_seed(){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	return scheduler->seed();
}

size_t FFI_error_code(){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	ErrorCode e = scheduler->error_code();
	return (size_t)e;
}

size_t FFI_get_operation_id(){

	assert(scheduler != NULL && "Wrong sequence of API calls. Create Coyote Scheduler first.");

	size_t id = scheduler->get_operation_id();
	assert(id >= 0 && "operation id can't be negative!");

	return id;
}

void FFI_set_state_read(){

	if(curr_state == STATE_READ) return;

	// There is a state change!
	num_cxt_switch = 0;
	curr_state = STATE_READ;
}

void FFI_set_state_write(){

	if(curr_state == STATE_WRITE) return;

	// There is a state change!
	num_cxt_switch = 0;
	curr_state = STATE_WRITE;
}

/***** Modelling of pthread APIs using coyote scheduler APIs *****
* Goal: to provide a drop-in replacement of default pthread APIs.
******************************************************************/

// Assuming that no 2 threads can simultaneously call the hash_map related methods.
// STL containers like hash map and vectors are NOT thread safe, but this shouldn't
// be a problem in our case.
int FFI_pthread_mutex_init(void *ptr){

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_mutex_init: recieved: %p \n", ptr);
#endif

	llu key = (llu)ptr;

	// Lazy initialization of hash map
	if(hash_map == NULL){
		hash_map = new std::unordered_map<llu, CoyoteLock*>();
	}

	assert(hash_map->find(key) == hash_map->end() && "FFI_pthread_mutex_init: Key is already in the map\n");

	// Make sure that this key is not in the list of `Globally initialized' mutexes. Otherwise, it can be a potential
	// double initialization bug!

	if(lazy_mutex_init_list != NULL){

		std::vector<void*>::iterator it;
		it = std::find(lazy_mutex_init_list->begin(), lazy_mutex_init_list->end(), ptr);

		//assert(it == lazy_mutex_init_list->end() && "This mutex is already globally initialized!");
	}

	// Create a new resource object and insert it into the hash map
	CoyoteLock* new_obj = new CoyoteLock();
	bool rv = (hash_map->insert({key, new_obj})).second;
	assert(rv == true && "FFI_pthread_mutex_init: Inserting in the map failed!\n");

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_mutex_init: Mapped: %p to coyote resource id: %d \n", ptr, new_obj->coyote_resource_id);
#endif

	return 0;
}

// Called only for globally initialized mutexs
int FFI_pthread_mutex_lazy_init(void *ptr){

	if(lazy_mutex_init_list == NULL){
		lazy_mutex_init_list = new std::vector<void*>();
	}

	// Add it to the list of globally initialized mutex
	lazy_mutex_init_list->push_back(ptr);
	return 0;
}

void check_and_init_mutex(void* ptr){

	if(lazy_mutex_init_list == NULL) return;

	std::vector<void*>::iterator it;
	it = std::find(lazy_mutex_init_list->begin(), lazy_mutex_init_list->end(), ptr);

	// If the item is in the list
	if(it != lazy_mutex_init_list->end()){
		FFI_pthread_mutex_init(ptr);
	}
}

int FFI_pthread_mutex_lock(void *ptr){

	assert(hash_map != NULL && "FFI_pthread_mutex_lock: Initialize the hash map first\n");

	llu key = (llu)ptr;
	std::unordered_map<llu, CoyoteLock*>::iterator it = hash_map->find(key);

	// If it is not in the hash map, try looking up in the lazy init list
	if(it == hash_map->end()){
		check_and_init_mutex(ptr);
		it = hash_map->find(key);
	}

	assert(it != hash_map->end() && "FFI_pthread_mutex_lock: key not in map\n");

	CoyoteLock* obj = it->second;

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_mutex_lock: Locking on: %p as coyote resource id: %d \n", ptr, obj->coyote_resource_id);
#endif

	assert( ( !(obj->is_locked) || FFI_get_operation_id() != obj->user_op_id ) && 
		"This thread is already holding this lock, why is it trying to lock it again?");

	// If the resource is already locked, then spinlock!
	while(obj->is_locked){
		FFI_wait_resource(obj->coyote_resource_id);
	}

	// If the resource is free for use, lock it!
	obj->is_locked = true;
	// How is holding this lock?
	obj->user_op_id = FFI_get_operation_id();

	return 0;
}

int FFI_pthread_mutex_trylock(void *ptr){

	assert(hash_map != NULL && "FFI_pthread_mutex_trylock: Initialize the hash map first\n");

	llu key = (llu)ptr;
	std::unordered_map<llu, CoyoteLock*>::iterator it = hash_map->find(key);

	// If it is not in the hash map, try looking up in the lazy init list
	if(it == hash_map->end()){

		check_and_init_mutex(ptr);
		it = hash_map->find(key);
	}

	assert(it != hash_map->end() && "FFI_pthread_mutex_trylock: key not in map\n");

	CoyoteLock* obj = it->second;

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_mutex_trylock: Locking on: %p as coyote resource id: %d \n", ptr, obj->coyote_resource_id);
#endif

	// If the resource is already locked, then return -1;
	if(obj->is_locked){
		return -1;
	}
	// Otherwise, return 0 and gain the lock
	obj->is_locked = true;
	// How is holding this lock?
	obj->user_op_id = FFI_get_operation_id();

	return 0;
}

int FFI_pthread_mutex_is_lock(void *ptr){

	assert(hash_map != NULL && "FFI_pthread_mutex_is_lock: Initialize the hash map first\n");

	llu key = (llu)ptr;
	std::unordered_map<llu, CoyoteLock*>::iterator it = hash_map->find(key);

	// If it is not in the hash map, try looking up in the lazy init list
	if(it == hash_map->end()){
		check_and_init_mutex(ptr);
		it = hash_map->find(key);
	}

	assert(it != hash_map->end() && "FFI_pthread_mutex_is_lock: key not in map\n");

	CoyoteLock* obj = it->second;

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_mutex_is_lock: Locking on: %p as coyote resource id: %d \n", ptr, obj->coyote_resource_id);
#endif

	// If the resource is already locked, then return -1;
	if(obj->is_locked){
		return 0;
	} else {
		return -1;
	}
}

int FFI_pthread_mutex_unlock(void *ptr){

	assert(hash_map != NULL && "FFI_pthread_mutex_unlock: Initialize the hash map first\n");

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_mutex_unlock: Unlocking on: %p \n", ptr);
#endif

	llu key = (llu)ptr;
	std::unordered_map<llu, CoyoteLock*>::iterator it = hash_map->find(key);

	// If it is not in the hash map, try looking up in the lazy init list
	if(it == hash_map->end()){
		check_and_init_mutex(ptr);
		it = hash_map->find(key);
	}

	assert(it != hash_map->end() && "FFI_pthread_mutex_unlock: key not in map\n");

	CoyoteLock* obj = it->second;

	assert(obj->is_locked == true &&
		 "FFI_pthread_mutex_unlock: Resource wasn't locked before calling this function");

	obj->is_locked = false;

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_mutex_unlock: Unlocking on: %p as coyote resource id: %d \n", ptr, obj->coyote_resource_id);
#endif

	FFI_signal_resource(obj->coyote_resource_id);

	return 0;
}

int FFI_pthread_mutex_destroy(void *ptr){

	assert(hash_map != NULL && "FFI_pthread_mutex_destroy: Initialize the hash map first\n");

	llu key = (llu)ptr;
	std::unordered_map<llu, CoyoteLock*>::iterator it = hash_map->find(key);

	// If it is not in the hash map, try looking up in the lazy init list
	if(it == hash_map->end()){
		check_and_init_mutex(ptr);
		it = hash_map->find(key);
	}

	assert(it != hash_map->end() && "FFI_pthread_mutex_destroy: key not in map\n");

	CoyoteLock* obj = it->second;
	assert(obj->is_locked == false && "FFI_pthread_mutex_destroy: Don't destroy a locked mutex!");

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_mutex_destroy: Destroying: %p and coyote resource id: %d \n", ptr, obj->coyote_resource_id);
#endif

	hash_map->erase(it); // Remove the object from hash_map
	delete obj; // Remove the object from heap

	return 0;
}

int FFI_pthread_cond_init(void* ptr){

	llu key = (llu)ptr;

	if(hash_map == NULL){
		hash_map = new std::unordered_map<llu, CoyoteLock*>();
	}

	assert(hash_map->find(key) == hash_map->end() && "FFI_pthread_cond_init: Key is already in the map\n");

	// Make sure that this key is not in the list of `Globally initialized' condition vars. Otherwise, it can be a potential
	// double initialization bug!

	if(lazy_cond_init_list != NULL){

		std::vector<void*>::iterator it;
		it = std::find(lazy_cond_init_list->begin(), lazy_cond_init_list->end(), ptr);

		// assert(it == lazy_cond_init_list->end() && "This condition variable is already globally initialized!");
	}

	CoyoteLock* new_obj = new CoyoteLock(-1, INT_MAX, true /*it is a condition variable*/);
	bool rv = (hash_map->insert({key, new_obj})).second;
	assert(rv == true && "FFI_pthread_cond_init: Inserting in the map failed!\n");

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_cond_init: Initializing: %p as coyote resource id: %d \n", ptr, new_obj->coyote_resource_id);
#endif

	return 0;
}

int FFI_pthread_cond_lazy_init(void *ptr){

	if(lazy_cond_init_list == NULL){
		lazy_cond_init_list = new std::vector<void*>();
	}

	lazy_cond_init_list->push_back(ptr);
	return 0;
}

void check_and_init_cond(void *ptr){

	if(lazy_cond_init_list == NULL) return;

	std::vector<void*>::iterator it;
	it = std::find(lazy_cond_init_list->begin(), lazy_cond_init_list->end(), ptr);

	// If the item is in the list, initialize it!
	if(it != lazy_cond_init_list->end()){
		FFI_pthread_cond_init(ptr);
	}
}

int FFI_pthread_cond_wait(void* cond_var_ptr, void* mtx){

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_cond_wait: with cond_var: %p and mutex is: %p \n", cond_var_ptr, mtx);
#endif

	assert(hash_map != NULL && "FFI_pthread_cond_wait: Initialize the hash map first\n");

	llu cond_var_key = (llu)cond_var_ptr;
	llu mutex_key = (llu)mtx;

	// First check whether the conditional variable and mutex are in the map or not
	std::unordered_map<llu, CoyoteLock*>::iterator it_cond = hash_map->find(cond_var_key);
	std::unordered_map<llu, CoyoteLock*>::iterator it_mtx = hash_map->find(mutex_key);

	// If conditional variable is not in the map; check it in the lazy initialization list
	if(it_cond == hash_map->end()){

		check_and_init_cond(cond_var_ptr);
		it_cond = hash_map->find(cond_var_key);
	}

	assert(it_cond != hash_map->end() && "FFI_pthread_cond_wait: conditional variable not in map\n");
	assert(it_mtx != hash_map->end() && "FFI_pthread_cond_wait: mutex not in map\n");

	CoyoteLock* cond_var = (*it_cond).second;
	assert(cond_var->is_cond_var && "It is not a conditional variable!");
	assert(cond_var->waitingOps != NULL && "FFI_pthread_cond_wait: Vector of WaitingOps is NULL");

	// If they are in the map:
	// Register this operation in the list of all operations waiting on this
	// conditional variable.
	size_t current_op_id = FFI_get_operation_id();

	cond_var->waitingOps->push_back(current_op_id);
	cond_var->is_locked = true;

	// Now, unlock the mutex. Unlocking the mutex can cause a context switch becoz of scheduler->signal_resource()
	// Even in that case, we should be safe.
	FFI_pthread_mutex_unlock(mtx);

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_cond_wait: Going to sleep on cond_var: %p and coyote res_id: %d \n", cond_var_ptr, cond_var->coyote_resource_id);
#endif

	// Wait for cond_signal or cond_broadcast
	while(cond_var->is_locked && (find(cond_var->waitingOps->begin(), cond_var->waitingOps->end(), current_op_id) 
									  != cond_var->waitingOps->end())   ){
		FFI_wait_resource(cond_var->coyote_resource_id);
	}

	// Lock that conditional variable again, so that other operations can wait on this conditional variable
	cond_var->is_locked = true;

	// Lock the mutex again before exiting this function
	FFI_pthread_mutex_lock(mtx);

	return 0;
}

int FFI_pthread_cond_signal(void* ptr){

	assert(hash_map != NULL && "FFI_pthread_cond_signal: Initialize the hash map first\n");

	llu cond_key = (llu)ptr;

	// First check whether the conditional variable are in the map or not
	std::unordered_map<llu, CoyoteLock*>::iterator it_cond = hash_map->find(cond_key);

	// If conditional variable is not in the map; check it in the lazy initialization list
	if(it_cond == hash_map->end()){

		check_and_init_cond(ptr);
		it_cond = hash_map->find(cond_key);
	}
	assert(it_cond != hash_map->end() && "FFI_pthread_cond_signal: conditional variable not in map\n");

	CoyoteLock* cond_obj = (*it_cond).second;
	assert(cond_obj->is_cond_var && "FFI_pthread_cond_signal: this is not a conditional variable");

	// Check whether there is someone waiting on this cond_var or not
	if(!cond_obj->waitingOps->empty()){

		// Get the last element and signal it
		size_t op_id = cond_obj->waitingOps->back();
		cond_obj->waitingOps->pop_back(); // remove the last element from the list

		cond_obj->is_locked = false; // Unlock it and signal the operation

		// It is the responsibility of this operation to lock the conditional variable again!
		FFI_signal_resource_to_op(cond_obj->coyote_resource_id, op_id);
	} else{

		// If there is no one waiting, then just unlock it
		cond_obj->is_locked = false;
	}

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_cond_signal: Signalling on cond_var: %p and coyote res_id: %d \n", ptr, cond_obj->coyote_resource_id);
#endif

	return 0;
}

int FFI_pthread_cond_broadcast(void* ptr){

	assert(hash_map != NULL && "FFI_pthread_cond_broadcast: Initialize the hash map first\n");

	llu cond_key = (llu)ptr;

	// First check whether the conditional variable are in the map or not
	std::unordered_map<llu, CoyoteLock*>::iterator it_cond = hash_map->find(cond_key);

	// If conditional variable is not in the map; check it in the lazy initialization list
	if(it_cond == hash_map->end()){

		check_and_init_cond(ptr);
		it_cond = hash_map->find(cond_key);
	}
	assert(it_cond != hash_map->end() && "FFI_pthread_cond_broadcast: conditional variable not in map\n");

	CoyoteLock* cond_obj = (*it_cond).second;
	assert(cond_obj->is_cond_var && "FFI_pthread_cond_broadcast: this is not a conditional variable");

	if(cond_obj->waitingOps->empty()){

		// If there is no one waiting, then just unlock it
		cond_obj->is_locked = false;
	}

	// Check whether there is someone waiting on this cond_var or not
	while(!cond_obj->waitingOps->empty()){

		// Get the last element and signal it
		size_t op_id = cond_obj->waitingOps->back();
		cond_obj->waitingOps->pop_back(); // remove the last element from the list

		cond_obj->is_locked = false; // Unlock it and signal the operation

#ifdef DEBUG_PTHREAD_API
	printf("In FFI_pthread_cond_broadcast: Signalling on cond_var: %p and coyote res_id: %ld \n", ptr, op_id);
#endif

		// It is the responsibility of this operation to lock the conditional variable again!
		// Not sure if instead of this,  I should do a single FFI_signal_resource() out side this loop
		FFI_signal_resource_to_op(cond_obj->coyote_resource_id, op_id);
	};

	return 0;
}

int FFI_pthread_cond_destroy(void* ptr){

	assert(hash_map != NULL && "FFI_pthread_cond_destroy: Initialize the hash map first\n");

	llu cond_key = (llu)ptr;

	// First check whether the conditional variable is in the map or not
	std::unordered_map<llu, CoyoteLock*>::iterator it_cond = hash_map->find(cond_key);

	// If conditional variable is not in the map; check it in the lazy initialization list
	if(it_cond == hash_map->end()){

		check_and_init_cond(ptr);
		it_cond = hash_map->find(cond_key);
	}

	assert(it_cond != hash_map->end() && "FFI_pthread_cond_destroy: conditional variable not in map\n");

	CoyoteLock* cond_obj = (*it_cond).second;
	assert(cond_obj->is_cond_var && "FFI_pthread_cond_destroy: this is not a conditional variable");

	hash_map->erase(it_cond);
	delete cond_obj;

	return 0;
}

} //End of Extern "C"

#if 0

/******************************************** CoyoteOps Start ****************************************/

// Contains definition of the type pthread_t
#include <pthread.h>

// Global counter
size_t total_operation_count = 1;

// Global map between Coyote operation id and pthread thread id. We have kept a boolean flag to indicate
// that a perticular mapping is valid or not
std::unordered_map<pthread_t, std::pair<size_t, bool>>* coyote_ops_hash_map = NULL;

// Synchronize operations on hash map!
int coyote_ops_hash_map_mutex = 0;

extern "C"{

void FFI_create_pthread_task(pthread_t tid){

	// Initialize the map
	if(coyote_ops_hash_map == NULL){
		coyote_ops_hash_map = new std::unordered_map<pthread_t, std::pair<size_t, bool>>();
		FFI_pthread_mutex_init(&coyote_ops_hash_map_mutex);
	}

	FFI_pthread_mutex_lock(&coyote_ops_hash_map_mutex);

	assert(coyote_ops_hash_map != NULL && "coyote_ops_hash_map is NULL!");

	std::unordered_map<pthread_t, std::pair<size_t, bool>>::iterator it = coyote_ops_hash_map->find(tid);

	// If this item is already in the map. Make sure the entry is invalid and remove it!
	if(it != coyote_ops_hash_map->end()){

		assert(((*it).second).second == false && "This entry is still valid." &&
		"Can not overwrite it!");

		coyote_ops_hash_map->erase(it);
	}

	// Insert in the hash map and create a new coyote operation
	bool rv = (coyote_ops_hash_map->insert({tid, {total_operation_count, true}})).second;
	assert(rv == true && "Insertion into hash map failed!");

	FFI_pthread_mutex_unlock(&coyote_ops_hash_map_mutex);

	FFI_create_operation(total_operation_count);

#ifdef DEBUG_PTHREAD_API
	printf("Created Coyote operation id: %lu \n", total_operation_count);
#endif

	total_operation_count++;
}

void FFI_start_pthread_task(pthread_t tid){

	FFI_pthread_mutex_lock(&coyote_ops_hash_map_mutex);

	assert(coyote_ops_hash_map != NULL &&
		"Wrong sequence of API calls; Initialize the coyote_op hash map first");

	std::unordered_map<pthread_t, std::pair<size_t, bool>>::iterator it = coyote_ops_hash_map->find(tid);
	assert(it != coyote_ops_hash_map->end() && "This pthread id is not registered in the map");

	size_t coyote_op_id = ((*it).second).first;

	assert(((*it).second).second == true && "This entry is invalid." &&
		"Can it be possible that this function is called after complete or join operation?");

	FFI_pthread_mutex_unlock(&coyote_ops_hash_map_mutex);

	FFI_start_operation(coyote_op_id);

#ifdef DEBUG_PTHREAD_API
	printf("Started Coyote operation id: %lu \n", coyote_op_id);
#endif
}

void FFI_complete_pthread_task(pthread_t tid){

	assert(coyote_ops_hash_map != NULL &&
		"Wrong sequence of API calls; Initialize the coyote_op hash map first");

	FFI_pthread_mutex_lock(&coyote_ops_hash_map_mutex);

	std::unordered_map<pthread_t, std::pair<size_t, bool>>::iterator it = coyote_ops_hash_map->find(tid);
	assert(it != coyote_ops_hash_map->end() && "This pthread id is not registered in the map");

	size_t coyote_op_id = ((*it).second).first;

	assert(((*it).second).second == true && "This entry is invalid." &&
		"Can it be possible that this function is called after complete or join operation?");

	// Mark this entry as invalid in the hash map
	it->second = {coyote_op_id, false};

	FFI_pthread_mutex_unlock(&coyote_ops_hash_map_mutex);

	FFI_complete_operation(coyote_op_id);
	// Don't delete the entry from the hash map as there can be a
	// join_operation call after complete_operation. Just mark the
	// entry as invalid.

#ifdef DEBUG_PTHREAD_API
	printf("Completed Coyote operation id: %lu \n", coyote_op_id);
#endif
}

void FFI_join_pthread_task(pthread_t tid){

	assert(coyote_ops_hash_map != NULL &&
		"Wrong sequence of API calls; Initialize the coyote_op hash map first");

	FFI_pthread_mutex_lock(&coyote_ops_hash_map_mutex); /* Lock the mutex */

	std::unordered_map<pthread_t, std::pair<size_t, bool>>::iterator it = coyote_ops_hash_map->find(tid);
	assert(it != coyote_ops_hash_map->end() && "This pthread id is not registered in the map");

	size_t coyote_op_id = ((*it).second).first;

	// Mark this entry as invalid in the hash map
	it->second = {coyote_op_id, false};

	FFI_pthread_mutex_unlock(&coyote_ops_hash_map_mutex); /* Unlock the mutex */

	FFI_join_operation(coyote_op_id);

	FFI_pthread_mutex_lock(&coyote_ops_hash_map_mutex); /* Lock the mutex */

	coyote_ops_hash_map->erase(it);

	FFI_pthread_mutex_unlock(&coyote_ops_hash_map_mutex); /* Unlock the mutex */

#ifdef DEBUG_PTHREAD_API
	printf("Joined Coyote operation id: %lu \n", coyote_op_id);
#endif
}

void clean_coyote_ops_hash_map(){

	if(coyote_ops_hash_map != NULL){

		coyote_ops_hash_map->clear();

		// Destroy and reset global variables
		FFI_pthread_mutex_destroy(&coyote_ops_hash_map_mutex);
		total_operation_count  =1;
	}
}

} // End of ectern "C"

/******************************************** CoyoteOps End ******************************************/

#endif
