/* Foreign function interface for Coyote scheduler
*  APIs for C language.
*  This file should be included in the C program which will use the Coyote Scheduler APIs.
*/

#ifndef COYOTE_C_FFI
#define COYOTE_C_FFI

// Required by some old compilers!
#include <assert.h>
#include <stdbool.h>
#include <stddef.h>

#define INTERCEPT_HEAP_ALLOCATORS

// FFI for Coyote create_scheduler(void) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_create_scheduler();
#else
	#define FFI_create_scheduler()
#endif

// FFI for Coyote create_scheduler(size_t seed) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_create_scheduler_w_seed(size_t);
#else
	#define FFI_create_scheduler_w_seed(x)
#endif

// FFI for Coyote create_scheduler("RandomStrategy") API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_create_scheduler_rand();
#else
	#define FFI_create_scheduler_rand()
#endif

// FFI for Coyote create_scheduler("PCTStrategy") API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_create_scheduler_pct();
#else
	#define FFI_create_scheduler_pct()
#endif

// FFI for Coyote create_scheduler("DFSStrategy") API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_create_scheduler_dfs();
#else
	#define FFI_create_scheduler_dfs()
#endif

// For deleting the scheduler instance
#ifndef DISABLE_COYOTE_FFI
	void FFI_delete_scheduler();
#else
	#define FFI_delete_scheduler()
#endif

// FFI for Coyote attach_scheduler(void) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_attach_scheduler();
#else
	#define FFI_attach_scheduler()
#endif

// FFI for Coyote detach_scheduler(void) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_detach_scheduler();
#else
	#define FFI_detach_scheduler()
#endif

// Just asserts that scheduler didn't encountered any error.
// Asserts that scheduler->error_code() == ErrorCode::Success
#ifndef DISABLE_COYOTE_FFI
	void FFI_scheduler_assert();
#else
	#define FFI_scheduler_assert()
#endif

// FFI for Coyote create_operation(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_create_operation(size_t);
#else
	#define FFI_create_operation(x)
#endif

// FFI for Coyote start_operation(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_start_operation(size_t);
#else
	#define FFI_start_operation(x)
#endif

// FFI for Coyote join_operation(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_join_operation(size_t);
#else
	#define FFI_join_operation(x)
#endif

// FFI for Coyote join_operations(size_t*, size_t, bool) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_join_operations(const size_t*, size_t, bool);
#else
	#define FFI_join_operations(x, y, z)
#endif

// FFI for Coyote complete_operation(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_complete_operation(size_t id);
#else
	#define FFI_complete_operation(x)
#endif

// FFI for Coyote create_resource(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_create_resource(size_t id);
#else
	#define FFI_create_resource(x)
#endif

// FFI for Coyote wait_resource(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_wait_resource(size_t id);
#else
	#define FFI_wait_resource(x)
#endif

// FFI for Coyote wait_scheduler(size_t*, size_t, bool) API call
#ifndef DISABLE_COYOTE_FFI
	void FFT_wait_resources(const size_t* resource_ids, size_t size, bool wait_all);
#else
	#define FFT_wait_resources(x, y, z)
#endif

// FFI for Coyote signal_resource(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_signal_resource(size_t id);
#else
	#define FFI_signal_resource(x)
#endif

// FFI for Coyote signal_resource(size_t, size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_signal_resource_to_op(size_t id, size_t op_id);
#else
	#define FFI_signal_resource_to_op(x, y)
#endif

// FFI for Coyote delete_resource(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_delete_resource(size_t id);
#else
	#define FFI_delete_resource(x)
#endif

// FFI for Coyote schedule_next(void) API call
#ifndef DISABLE_COYOTE_FFI
	void FFI_schedule_next();
#else
	#define FFI_schedule_next()
#endif

// FFI for Coyote next_boolean(void) API call
#ifndef DISABLE_COYOTE_FFI
	bool FFI_next_boolean();
#else
	#define FFI_next_boolean() (assert(0 && "Should not be called with DISABLE_COYOTE_FFI"); return 0;)
#endif

// FFI for Coyote next_integer(size_t) API call
#ifndef DISABLE_COYOTE_FFI
	size_t FFI_next_integer(size_t max_value);
#else
	#define FFI_next_integer(x) (assert(0 && "Should not be called with DISABLE_COYOTE_FFI"); return 0;)
#endif

// FFI for Coyote seed(void) API call
#ifndef DISABLE_COYOTE_FFI
	size_t FFI_seed();
#else
	#define FFI_seed() (assert(0 && "Should not be called with DISABLE_COYOTE_FFI"); return 0;)
#endif

// FFI for Coyote error_code(void) API call
#ifndef DISABLE_COYOTE_FFI
	size_t FFI_error_code();
#else
	#define FFI_error_code() (assert(0 && "Should not be called with DISABLE_COYOTE_FFI"); return 0;)
#endif

// FFI for Coyote get_operation_id(void) API call
#ifndef DISABLE_COYOTE_FFI
	size_t FFI_get_operation_id();
#else
	#define FFI_get_operation_id() (assert(0 && "Should not be called with DISABLE_COYOTE_FFI"); return 0;)
#endif

// This method should be called after pthread_mutex_init with mutex pointer as param
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_mutex_init(void* mutex_ptr, void* attr);
#else
	#define FFI_pthread_mutex_init(x, y)
#endif

#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_mutex_lazy_init(void* mutex_ptr);
#else
	#define FFI_pthread_mutex_lazy_init(x)
#endif

// Drop-in replacement of pthread_mutex_lock.
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_mutex_lock(void* mutex_ptr);
#else
	#define FFI_pthread_mutex_lock(x)
#endif

// Drop-in replacement of pthread_mutex_trylock.
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_mutex_trylock(void* mutex_ptr);
#else
	#define FFI_pthread_mutex_trylock(x)
#endif

// Not a custom pthread API. Just used for asserting certain properties
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_mutex_is_lock(void* mutex_ptr);
#else
	#define FFI_pthread_mutex_is_lock(x)
#endif

// Drop-in replacement of pthread_mutex_unlock.
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_mutex_unlock(void* mutex_ptr);
#else
	#define FFI_pthread_mutex_unlock(x)
#endif

// This method should be called after pthread_mutex_destroy with mutex pointer as param
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_mutex_destroy(void* mutex_ptr);
#else
	#define FFI_pthread_mutex_destroy(x)
#endif

// These are some old, incorrect and misleading function names that I have used in some benchmarks
#define FFI_coyote_register_mutex(x) FFI_pthread_mutex_init(x)
#define FFI_mock_pthread_lock(x) FFI_pthread_mutex_lock(x)
#define FFI_mock_pthread_unlock(x) FFI_pthread_mutex_unlock(x)
#define FFI_coyote_destroy_mutex(x) FFI_pthread_mutex_destroy(x)

// This method should be called after pthread_cond_init with conditional variable pointer as param
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_cond_init(void* cond_var, void* attr);
#else
	#define FFI_pthread_cond_init(x, y)
#endif

#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_cond_lazy_init(void* cond_ptr);
#else
	#define FFI_pthread_cond_lazy_init(x)
#endif

// Drop-in replacement of pthread_cond_wait.
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_cond_wait(void* cond_ptr, void* mtx);
#else
	#define FFI_pthread_cond_wait(x)
#endif

// Drop-in replacement of pthread_cond_signal.
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_cond_signal(void* cond_ptr);
#else
	#define FFI_pthread_cond_signal(x)
#endif

// Drop-in replacement of pthread_cond_broadcast.
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_cond_broadcast(void* cond_ptr);
#else
	#define FFI_pthread_cond_broadcast(x)
#endif

// This method should be called after pthread_cond_destroy with conditional variable pointer as param
#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_cond_destroy(void* cond_ptr);
#else
	#define FFI_pthread_cond_destroy(x)
#endif

#ifndef DISABLE_COYOTE_FFI
	int FFI_pthread_create(void*, void*, void *(*start)(void *), void*);
#else
	#define FFI_pthread_create(x, y, z, a)
#endif

#ifndef DISABLE_COYOTE_FFI
	void FFI_set_state_read();
#else
	#define FFI_set_state_read()
#endif

#ifndef DISABLE_COYOTE_FFI
	void FFI_set_state_write();
#else
	#define FFI_set_state_write()
#endif

#ifdef INTERCEPT_HEAP_ALLOCATORS

#ifndef DISABLE_COYOTE_FFI
	void* FFI_malloc(size_t);
#else
	#define FFI_malloc(x)
#endif

#ifndef DISABLE_COYOTE_FFI
	void* FFI_calloc(size_t, size_t);
#else
	#define FFI_calloc(x, y)
#endif

#ifndef DISABLE_COYOTE_FFI
	void* FFI_realloc(void*, size_t);
#else
	#define FFI_realloc(x, y)
#endif

#ifndef DISABLE_COYOTE_FFI
	void FFI_free(void*);
#else
	#define FFI_free(x)
#endif

#ifndef DISABLE_COYOTE_FFI
	void FFI_free_all(void);
#else
	#define FFI_free_all()
#endif

#endif

#endif // COYOTE_C_FFI
