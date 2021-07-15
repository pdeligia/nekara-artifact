#ifndef COYOTE_MC_REDEF
#define COYOTE_MC_REDEF

#include <sys/socket.h>

#define EXECUTION_COYOTE_CONTROLLED

/****************************** Reset Global variables **************************/
#ifdef IN_LOGGER_FILE

static logger *logger_stack_head;
static logger *logger_stack_tail;
static unsigned int logger_count;
static volatile int do_run_logger_thread;
pthread_mutex_t logger_stack_lock;
pthread_mutex_t logger_atomics_mutex;
int watcher_count;
static uint64_t logger_gid;

pthread_mutex_t logger_block_mutex = PTHREAD_MUTEX_INITIALIZER;
pthread_cond_t logger_block_cond = PTHREAD_COND_INITIALIZER;

void logger_watcher_reset();

void reset_logger_globals(){

    logger_stack_head = NULL;
    logger_stack_tail = NULL;
    logger_count = 0;
    do_run_logger_thread = 1;
    FFI_pthread_mutex_lazy_init(&logger_stack_lock);
    FFI_pthread_mutex_lazy_init(&logger_atomics_mutex);
    watcher_count = 0;
    logger_gid = 0;
    logger_watcher_reset();
}
#endif /*IN_LOGGER_FILE*/

#ifdef IN_MEMCACHED_FILE

#pragma GCC diagnostic ignored "-Wredundant-decls"

static volatile bool allow_new_conns;
void *ext_storage;
static conn *listen_conn;
static int stop_main_loop;
extern conn **conns;
static int do_run_conn_timeout_thread;
static bool monotonic;
volatile int slab_rebalance_signal;
extern int optind;
extern pthread_cond_t logger_block_cond;
extern pthread_cond_t lru_maintainer_block_cond;

void reset_memcached_globals(){

	allow_new_conns = true;
	ext_storage = NULL;
	listen_conn = NULL;
	stop_main_loop = 0;
	slab_rebalance_signal = 0;
	monotonic = false;
	do_run_conn_timeout_thread = 0;
	conns = NULL;
	optind = 0;
}
#endif /*IN_MEMCACHED_FILE*/

#ifdef IN_THREAD_FILE

#pragma GCC diagnostic ignored "-Wredundant-decls"

pthread_mutex_t atomics_mutex;
pthread_mutex_t conn_lock;
static int init_count;
static int last_thread;
static pthread_mutex_t stats_lock;
static uint32_t item_lock_count;
unsigned int item_lock_hashpower;

void reset_worker_thread(void);

void reset_thread_globals(){

	FFI_pthread_mutex_lazy_init(&atomics_mutex);
	FFI_pthread_mutex_lazy_init(&conn_lock);
	FFI_pthread_mutex_lazy_init(&stats_lock);
	init_count = 0;
	last_thread = -1;
	item_lock_count = 0;
	item_lock_hashpower = 0;
	reset_worker_thread();
}
#endif /*IN_THREAD_FILE*/

#ifdef IN_ASSOC_FILE

static volatile int do_run_maintenance_thread;
static unsigned int expand_bucket;
static bool expanding;
int hash_bulk_move;
unsigned int hashpower;
static pthread_cond_t maintenance_cond;
static pthread_mutex_t maintenance_lock;
static item** old_hashtable;
static item** primary_hashtable;

// For storing hash values of the keys
static uint32_t *hv_vector = NULL;
static int hv_counter = 0;

#define COYOTE_CONTROLLED

void FFI_store_hv(uint32_t HashValue);
uint64_t FFI_get_item_hash(item* it);

void reset_assoc_globals(){

	do_run_maintenance_thread = 1;
	expand_bucket = 0;
	expanding = false;
    hash_bulk_move = 1;
    hashpower = 16;
    FFI_pthread_cond_lazy_init(&maintenance_cond);
    FFI_pthread_mutex_lazy_init(&maintenance_lock);
    old_hashtable = NULL;
    primary_hashtable = NULL;

    if(!hv_vector){
    	free(hv_vector);
    	hv_vector = NULL;
    }
    hv_counter = 0;
}

pthread_mutex_t hv_vector_lock = PTHREAD_MUTEX_INITIALIZER;

#ifdef EXECUTION_COYOTE_CONTROLLED
	#define LOCK_HV_VECTOR() FFI_pthread_mutex_lock(&hv_vector_lock);
	#define UNLOCK_HV_VECTOR() FFI_pthread_mutex_unlock(&hv_vector_lock);
#else
	#define LOCK_HV_VECTOR() pthread_mutex_lock(&hv_vector_lock);
	#define UNLOCK_HV_VECTOR() pthread_mutex_unlock(&hv_vector_lock);
#endif

void FFI_store_hv(uint32_t HashValue){

	assert(hv_counter < 4096 && "Total keys stored is more than 64");

	if(hv_vector == NULL){
		hv_vector = (uint32_t*)malloc(sizeof(uint32_t)*4096);
		hv_counter = 0;
	}

	LOCK_HV_VECTOR();
	hv_vector[hv_counter] = HashValue;
	hv_counter++;
	UNLOCK_HV_VECTOR();
}
#endif /*IN_ASSOC_FILE*/

#ifdef IN_CRAWLER_FILE

static int crawler_count;
static volatile int do_run_lru_crawler_thread;
static pthread_cond_t lru_crawler_cond;
static int lru_crawler_initialized;
static pthread_mutex_t lru_crawler_lock;

void reset_crawler_globals(){

	crawler_count = 0;
	do_run_lru_crawler_thread = 0;
	FFI_pthread_cond_lazy_init(&lru_crawler_cond);
	FFI_pthread_mutex_lazy_init(&lru_crawler_lock);
	lru_crawler_initialized = 0;
}
#endif /*IN_CRAWLER_FILE*/

#ifdef IN_ITEMS_FILE

#pragma GCC diagnostic ignored "-Wredundant-decls"

static pthread_mutex_t bump_buf_lock;
static uint64_t cas_id;
static pthread_mutex_t cas_id_lock;
static volatile int do_run_lru_maintainer_thread;
static int lru_maintainer_initialized;
static pthread_mutex_t lru_maintainer_lock;
static int stats_sizes_buckets;
static uint64_t stats_sizes_cas_min;
static unsigned int *stats_sizes_hist;
static pthread_mutex_t stats_sizes_lock;
static item *tails[256];
static item *heads[256];

static void reset_lru_bumps(void);

void reset_items_globals(){

	FFI_pthread_mutex_lazy_init(&bump_buf_lock);
	FFI_pthread_mutex_lazy_init(&cas_id_lock);
	FFI_pthread_mutex_lazy_init(&lru_maintainer_lock);
	FFI_pthread_mutex_lazy_init(&stats_sizes_lock);
	cas_id = 0;
	do_run_lru_maintainer_thread = 0;
	lru_maintainer_initialized = 0;
	stats_sizes_buckets = 0;
	stats_sizes_cas_min = 0;
	stats_sizes_hist = NULL;

	for(int i = 0; i < 255; i++){
		tails[i] = NULL;
		heads[i] = NULL;
	}

	reset_lru_bumps();
}
#endif /*IN_ITEMS_FILE*/

#ifdef IN_SLABS_FILE

static volatile int do_run_slab_rebalance_thread;
static size_t mem_avail;
static void *mem_base;
static void *mem_current;
static size_t mem_limit;
static bool mem_limit_reached;
static size_t mem_malloced;
static pthread_cond_t slab_rebalance_cond;
static pthread_mutex_t slabs_lock;
static pthread_mutex_t slabs_rebalance_lock;
static void *storage;

#pragma GCC diagnostic ignored "-Wredundant-decls"
static int power_largest;

static void reset_slab_classes(void);

void reset_slabs_globals(){

	do_run_slab_rebalance_thread = 1;
	mem_avail = 0;
	mem_base = NULL;
	mem_current = NULL;
	mem_limit = 0;
	mem_limit_reached = false;
	mem_malloced = 0;
	power_largest = 0;
	FFI_pthread_cond_lazy_init(&slab_rebalance_cond);
	FFI_pthread_mutex_lazy_init(&slabs_lock);
	FFI_pthread_mutex_lazy_init(&slabs_rebalance_lock);
	storage = NULL;
	reset_slab_classes();
}
#endif /*IN_SLABS_FILE*/

#ifdef EXECUTION_COYOTE_CONTROLLED
// pthread APIs
#define pthread_mutex_init(x, y) FFI_pthread_mutex_init(x, y)
#define pthread_mutex_lock(x) FFI_pthread_mutex_lock(x)
#define pthread_mutex_trylock(x) FFI_pthread_mutex_trylock(x)
#define pthread_mutex_unlock(x) FFI_pthread_mutex_unlock(x)
#define pthread_mutex_destroy(x) FFI_pthread_mutex_destroy(x)

#define pthread_cond_init(x, y) FFI_pthread_cond_init(x, y)
#define pthread_cond_wait(x, y) FFI_pthread_cond_wait(x, y)
#define pthread_cond_signal(x) FFI_pthread_cond_signal(x)
#define pthread_cond_broadcast(x) FFI_pthread_cond_signal(x)
#define pthread_cond_destory(x) FFI_pthread_cond_signal(x)

// MC specific defines
#undef mutex_lock
#define mutex_lock(x) FFI_pthread_mutex_lock(x)
#undef mutex_unlock
#define mutex_unlock(x) FFI_pthread_mutex_unlock(x)
#undef THR_STATS_LOCK
#define THR_STATS_LOCK(x) FFI_pthread_mutex_lock(x)
#undef THR_STATS_UNLOCK
#define THR_STATS_UNLOCK(x) FFI_pthread_mutex_unlock(x)

#define pthread_create(x, y, z, a) FFI_pthread_create(x, y, z, a)
#define pthread_join(x, y) FFI_pthread_join(x, y)

#endif /*EXECUTION_COYOTE_CONTROLLED*/
/*********************************************** LibEvent APIs **********************************************/

#define event_base_loop(x, y) FFI_event_base_loop(x, y)
#define event_base_set(x, y) FFI_event_base_set(x, y)
#define event_set(x, y, z, a, b) FFI_event_set(x, y, z, a, b)
#define event_del(x) FFI_event_del(x)
#define event_add(x, y) FFI_event_add(x, y)

#define event_base_loopexit(x, y) FFI_event_base_loopexit(x, y);

/*********************************************** System calls **********************************************/

// Make it non-blocking. Assign a file descriptor. Set incomming connection params
#define accept(x, y, z) FFI_accept(x, y, z)
#define getpeername(x, y, z) FFI_getpeername(x, y, z)
#define write(x, y, z) FFI_write(x, y, z)
#define read(x, y, z) FFI_read(x, y, z)
#define sendmsg(x, y, z) FFI_sendmsg(x, y, z)
#define recvfrom(a, b, c, d, e, f) FFI_recvfrom(a, b, c, d, e, f)
#define fcntl(x, ...) FFI_fcntl(x, __VA_ARGS__)
#define pipe(x) FFI_pipe(x)
#define poll(x, y, z) FFI_poll(x, y, z)
#define close(x) FFI_close(x)

#ifndef IOV_MAX
# define IOV_MAX 1024
/* GNU/Hurd don't set MAXPATHLEN
 * http://www.gnu.org/software/hurd/hurd/porting/guidelines.html#PATH_MAX_tt_MAX_PATH_tt_MAXPATHL */
#ifndef MAXPATHLEN
#define MAXPATHLEN 4096
#endif
#endif

// We doen't yet support accept4() and get_opt_long() sys call while coyote testing
#undef HAVE_ACCEPT4
#undef HAVE_GETOPT_LONG

#define main(x, y) run_coyote_iteration(x, y)

#ifdef EXECUTION_COYOTE_CONTROLLED
	#define usleep(x) {usleep(0); FFI_schedule_next();}
#endif

#define setbuf(x, y) { setbuf(x, y); FFI_register_clock_handler(clock_handler); FFI_register_main_stop(&stop_main_loop); FFI_schedule_next();}

// Intercept all the heap allocators to release heap after every iteration
#define malloc(x) FFI_malloc(x)
#define calloc(x, y) FFI_calloc(x, y)
#define realloc(x, y) FFI_realloc(x, y)
#define free(x) FFI_free(x)

#ifdef EXECUTION_COYOTE_CONTROLLED
// Temporarily disable perror(). Ideally, we should not disable it as it can hide some error messages.
#define perror(x)
#endif

// Remove schedule nexts if execution is not being controlled
#ifndef EXECUTION_COYOTE_CONTROLLED
	#define FFI_schedule_next()
#endif

#define COYOTE_2019_BUGS // For introducing data race bugs
// #define INJECTED_BUG_WORKER_WORKER // See injected Bug#1
//#define INJECTED_BUG2 // slab_grow_list will replace the old pointer while slab_rebalancer thread will still use the old pointer
//#define INJECTED_BUG3 // Bug between slab rebalancer and worker threads. See do_slabs_newslab().

#endif /* COYOTE_MC_REDEF */