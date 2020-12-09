// Compile: g++ -std=c++11 -shared -fPIC -I./ -g -o libmock_libevent.so mock_libevent.cpp -lcoyote_c_ffi -lcoyote -L./
// Mock of LibEvent APIs using coyote
#ifndef MOCKLIBEVENT
#define MOCKLIBEVENT

#include <event.h>
#include <map>
#include <unistd.h>
#include <assert.h>
#include <queue>
#include <pthread.h>

#define EXECUTION_COYOTE_CONTROLLED
#ifndef EXECUTION_COYOTE_CONTROLLED

	#define FFI_pthread_mutex_init(x, y) pthread_mutex_init(x,y)
	#define FFI_pthread_mutex_lock(x) pthread_mutex_lock(x)
	#define FFI_pthread_mutex_unlock(x) pthread_mutex_unlock(x)
	#define FFI_pthread_cond_init(x, y) pthread_cond_init(x, y)
	#define FFI_pthread_cond_wait(x, y) pthread_cond_wait(x, y)
	#define FFI_pthread_cond_signal(x) pthread_cond_signal(x)
	#define FFI_pthread_cond_destroy(x) pthread_cond_destroy(x)
	#define FFI_pthread_mutex_destroy(x) pthread_mutex_destroy(x)
	#define FFI_schedule_next()

	// Insert locks at all access to the global maps and variables
	pthread_mutex_t global_lock = PTHREAD_MUTEX_INITIALIZER;
	#define MAP_LOCK() pthread_mutex_lock(&global_lock);
	#define MAP_UNLOCK() pthread_mutex_unlock(&global_lock);

#else

	extern "C"{

		void FFI_schedule_next();
		void FFI_create_scheduler();
		int FFI_pthread_mutex_init(void* mutex_ptr, void* attr);
		int FFI_pthread_mutex_lock(void *ptr);
		int FFI_pthread_mutex_unlock(void *ptr);
		int FFI_pthread_cond_init(void* ptr, void* attr);
		int FFI_pthread_cond_wait(void* cond, void* mutex);
		int FFI_pthread_cond_signal(void* cond);
		int FFI_pthread_cond_destroy(void* ptr);
		int FFI_pthread_mutex_destroy(void* ptr);

		#define MAP_UNLOCK()
		#define MAP_LOCK()
	}
#endif

static int count_event_set = 0;
static void* dispatcher_event = NULL;
static void* dispatcher_event_base = NULL;

static std::map<int, void*>* map_fd_to_event  = NULL;
static std::map<void*, void*>* map_event_to_mocked_event = NULL;
static std::map<void*, void*>* map_eventbase_to_event = NULL;
static std::map<void*, void*>* map_event_to_lock = NULL;
static std::queue<void*>* queue_eventbase_to_event = NULL;

static void (*clock_handler)(int, short int, void*) = NULL;

struct mocked_event{

	void* orig_event;
	int sfd;
	int which;
	void (*callback_method)(int, short, void *);
	void* args;

	mocked_event(void *ev, void (*event_handler)(int, short, void *), int sfd_o, int which_o, void *arg_o){

		orig_event = ev;
		callback_method  = event_handler;
		sfd = sfd_o;
		which  = which_o;
		args = arg_o;
	}
};

struct worker_locks{

	pthread_mutex_t lock;
	pthread_cond_t cond;
	bool is_lock;
	bool restart;
	bool is_signaled;

	worker_locks(){
		FFI_pthread_mutex_init(&lock, NULL);
		FFI_pthread_cond_init(&cond, NULL);
		is_lock = true;
		restart = false;
		is_signaled = false;
	}
	~worker_locks(){
		FFI_pthread_mutex_destroy(&lock);
		FFI_pthread_cond_destroy(&cond);
	}
};

struct active_event{

	bool isEventActive;
	void* ev;

	active_event(bool temp, void* ev_){

		isEventActive = temp;
		ev = ev_;
	}

	void set_active(){
		isEventActive = true;
	}

	void set_inactive(){
		isEventActive = false;
	}

	bool isActive(){
		return isEventActive;
	}
};

extern "C"{

void FFI_register_clock_handler(void (*clk_handle)(int, short int, void*)){
	MAP_LOCK();
	clock_handler = clk_handle;
	MAP_UNLOCK();
}

void FFI_clock_handler(){
	clock_handler(0,0,0);
}

void FFI_event_set(event* ev, int sfd, int flags, void (*event_handler)(int, short, void *), void *arg){

	FFI_schedule_next();

	// In case of a clock handler
	if(sfd == -1)
		return;

	MAP_LOCK();

	if (map_fd_to_event == NULL){
		map_fd_to_event  = new std::map<int, void *>();
	}

	if(map_event_to_mocked_event == NULL){
		map_event_to_mocked_event = new std::map<void*, void*>();
	}

	bool rv = (map_fd_to_event->insert({sfd, ev})).second;
	assert(rv && "Insertion to event map failed");

	// I don't knwo what to put in c->which
	mocked_event* m_ev = new mocked_event(ev, event_handler, sfd, -1, arg);

	rv = (map_event_to_mocked_event->insert({ev, m_ev})).second;
	assert(rv && "Insertion to mock event map failed");

	MAP_UNLOCK();
	// Create a mock event, even if the input sfd is -1. Otherwise, event_del will throw an error.
	//if(sfd == -1) return;

	//printf("Inserted an event corresponding to sfd: %d \n", sfd);
	// Now call the actual libevent routine

	// event_set(ev, sfd, flags, event_handler, arg);
	return;
}

int FFI_event_add(struct event *ev, struct timeval *tv){

	FFI_schedule_next();
	return 0;
}

int FFI_event_base_set(struct event_base* base, struct event* ev){

	FFI_schedule_next();

	MAP_LOCK();

	if(map_event_to_mocked_event->find(ev) != map_event_to_mocked_event->end()){

		// This is dispatcher_thread
		if(map_eventbase_to_event == NULL){
			map_eventbase_to_event = new std::map<void*, void*>();
		}

		if(map_event_to_lock == NULL){
			map_event_to_lock = new std::map<void*, void*>();
		}

		if(map_event_to_lock->find(ev) == map_event_to_lock->end()){

			worker_locks *wl = new worker_locks();
			map_event_to_lock->insert({ev, wl});
		}

		std::map<void*, void*>::iterator it_clock = map_eventbase_to_event->find(base);

		// If this is already in the map. One event base can have multiple events!
		if(it_clock != map_eventbase_to_event->end()){

			//mocked_event* clk = (mocked_event *)((map_event_to_mocked_event->find( (it_clock)->second ))->second);
			//clock_handler = clk->callback_method;

			map_eventbase_to_event->erase(it_clock);
		}

		bool rv = (map_eventbase_to_event->insert({base, ev})).second;
		assert(rv && " Unable to insert into map_eventbase_to_event");
	}

	MAP_UNLOCK();
	//return event_base_set(base, ev);
	return 0;
}

int FFI_event_del(event* ev){

	FFI_schedule_next();

	MAP_LOCK();
	std::map<void*, void*>::iterator it = map_event_to_mocked_event->find(ev);

	// This can happen in the case of clock handler
	if(it == map_event_to_mocked_event->end()){
		MAP_UNLOCK();
		return 0;
	}

	mocked_event* m_ev = (mocked_event*)(it->second);

	//if(m_ev->sfd != -1)
	//	printf("Deleting an event corresponding to: %d \n", m_ev->sfd);

	std::map<int, void*>::iterator it2 = map_fd_to_event->find(m_ev->sfd);
	assert(it2 != map_fd_to_event->end() && "Interesting! file descriptor not found");

	map_fd_to_event->erase(it2);
	delete m_ev;
	map_event_to_mocked_event->erase(it);

	MAP_UNLOCK();
	return 0;
}

int FFI_event_base_loop(void* ev_base, int flags){

	FFI_schedule_next();

	// This will be used in case of a restart
	mocked_event cache_m_ev(0, 0, 0, 0, 0);
	bool isRestarted = false;

	// After restarting, we will come here
	start:

	// Dispatcher thread. Flags = 1 means EVLOOP_ONCE!
	if(flags == 1){

		MAP_LOCK();
		std::map<void*, void*>::iterator it = map_eventbase_to_event->find(ev_base);
		assert(it != map_eventbase_to_event->end() && "Couldn't find in event_base map");

		std::map<void*, void*>::iterator it1 = map_event_to_mocked_event->find( it->second );
		assert(it1 != map_event_to_mocked_event->end());

		mocked_event* m_ev = (mocked_event*)(it1->second);
		MAP_UNLOCK();

		FFI_schedule_next();
		m_ev->callback_method(m_ev->sfd, m_ev->which, m_ev->args);

		FFI_schedule_next();
		clock_handler(0, 0, 0);

		return 0;

	} else { /* Worker threads */

		MAP_LOCK();
		std::map<void*, void*>::iterator it = map_eventbase_to_event->find(ev_base);
		assert(it != map_eventbase_to_event->end());

		std::map<void*, void*>::iterator it2 = map_event_to_lock->find(it->second);
		assert(it2 != map_event_to_lock->end());

		worker_locks *wl_original = (worker_locks*)(it2->second);
		worker_locks *wl = wl_original;
		MAP_UNLOCK();

		// If it is not restarted, wait for someone to signal me
		if(!isRestarted){

			// If it is already signaled, leave this.
			FFI_pthread_mutex_lock(&(wl->lock));
			if(!(wl->is_signaled)){
				FFI_pthread_cond_wait(&(wl->cond), &(wl->lock));
			}
			FFI_pthread_mutex_unlock(&(wl->lock));

			// We can safely set the is_signaled bool var to false, after reading it.
			wl->is_signaled = false;

		}else{

			// Otherwise, enter the while loop
			isRestarted = false;
		}

		while(wl->is_lock){

			MAP_LOCK();
			std::map<void*, void*>::iterator it = map_eventbase_to_event->find(ev_base);
			assert(it != map_eventbase_to_event->end());

			std::map<void*, void*>::iterator it2 = map_event_to_lock->find(it->second);
			assert(it2 != map_event_to_lock->end());

			wl = (worker_locks*)(it2->second);
			MAP_UNLOCK();

			if(!(wl->is_lock)) {break;}

			MAP_LOCK();
			std::map<void*, void*>::iterator i3 = map_event_to_mocked_event->find(it->second);

			// This event has terminated!
			if(i3 != map_event_to_mocked_event->end()){

				mocked_event* m_ev = (mocked_event*)(i3->second);
				MAP_UNLOCK();

				if(cache_m_ev.callback_method == NULL)
					cache_m_ev = *m_ev;

				FFI_schedule_next();
				m_ev->callback_method(m_ev->sfd, m_ev->which, m_ev->args);
				// Don't call clock_handler from worker thread, that would be a disaster
				//clock_handler(0, 0, 0);
			}
			else{

				MAP_UNLOCK();
				break;
			}
		}

		// For final registration
		FFI_pthread_mutex_lock(&(wl_original->lock));
		if(!(wl_original->is_signaled)){
			FFI_pthread_cond_wait(&(wl_original->cond), &(wl_original->lock));
		}
		FFI_pthread_mutex_unlock(&(wl_original->lock));
		wl_original->is_signaled = false;

		if(wl_original->restart != true)
			return 0;

		// If we want to restart
		{

			MAP_LOCK();
			// First erase the iterator
			std::map<void*, void*>::iterator it = map_eventbase_to_event->find(ev_base);
			if(it != map_eventbase_to_event->end()){
				map_eventbase_to_event->erase(it);
			}

			MAP_UNLOCK();
			cache_m_ev.callback_method(cache_m_ev.sfd, cache_m_ev.which, cache_m_ev.args);

			// Now spin loop untill another thread re-inserts this eventbase in the map
			while(map_eventbase_to_event->find(ev_base) == map_eventbase_to_event->end()){
				FFI_schedule_next();
			}

			isRestarted = true;
			goto start;
		}

		return 0;
	}
}

int FFI_event_base_loopexit(void* ev_base, void* args){

		FFI_schedule_next();

		MAP_LOCK();
		std::map<void*, void*>::iterator it = map_eventbase_to_event->find(ev_base);
		assert(it != map_eventbase_to_event->end());

		std::map<void*, void*>::iterator it2 = map_event_to_lock->find(it->second);
		assert(it2 != map_event_to_lock->end());

		worker_locks *wl = (worker_locks*)(it2->second);
		MAP_UNLOCK();

		wl->is_lock = false;
		FFI_pthread_cond_signal(&(wl->cond));

		return 0;
}

ssize_t FFI_event_write(int fd, const void* buff, size_t count, int sfd_pipe){

	FFI_schedule_next();

	MAP_LOCK();
	std::map<int, void*>::iterator it = map_fd_to_event->find(fd);

	if(sfd_pipe != -1){

		it = map_fd_to_event->find(sfd_pipe);
	}
	MAP_UNLOCK();

	// If this is not an event fd
	if(it == map_fd_to_event->end()){
		return -1;
	}

	ssize_t retval = write(fd, buff, count);

	MAP_LOCK();
	std::map<void *, void *>::iterator it2 = map_event_to_lock->find(it->second);
	assert(it2 != map_event_to_lock->end());

	worker_locks* wl = (worker_locks*)(it2->second);
	MAP_UNLOCK();

	if(((char*)(buff))[0] == 'r'){
		wl->restart = true;
	}

	// Signal the worker!!!!!
	FFI_pthread_mutex_lock(&(wl->lock));
	wl->is_signaled = true;
	FFI_pthread_cond_signal(&(wl->cond));
	FFI_pthread_mutex_unlock(&(wl->lock));

	FFI_schedule_next();

	return retval;
}

void FFI_event_reset_all(){

	count_event_set = 0;
	dispatcher_event = NULL;
	dispatcher_event_base = NULL;

	delete map_fd_to_event;
	map_fd_to_event  = NULL;

	delete map_event_to_mocked_event;
	map_event_to_mocked_event = NULL;

	delete map_eventbase_to_event;
	map_eventbase_to_event = NULL;

	delete map_event_to_lock;
	map_event_to_lock = NULL;

	delete queue_eventbase_to_event;
	queue_eventbase_to_event = NULL;
}
} /* End of Extern 'C'*/

#endif /* MOCKLIBEVENT */
