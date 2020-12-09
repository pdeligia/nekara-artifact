// Compile this as: g++ -std=c++11 -shared -fPIC -I./ -g -o libcoyotest.so test_basic_store_get.cpp -g -L/home/udit/memcached_2020/memcached/include_coyote/include -lcoyote_c_ffi -lcoyote
#include <test_template.h>
#include <regex>
#include <fstream>
#include <iostream>

using namespace std;

vector<conn*>* global_conns;

volatile int temp_counter = 0;
volatile bool found_this_iteration = false;
// To disable printf statements
#define printf(x, ...)

char* get_key_name(int i, char prefix = ' '){

	assert( i > 0 && "key shouldn't be less than or equal to 0");

	if(prefix == ' '){

		string st("key_");
		st = st + to_string(i);

		char* retval = (char*)FFI_malloc( sizeof(char) * (st.length() + 10));
		strcpy(retval, st.c_str());

		return retval;
	}
	else{

		char* pre = (char*)FFI_malloc(sizeof(char)*5);
		pre[0] = prefix;
		pre[1] = '\0';

		string prefix(pre);
		string st("key_");
		st = prefix + st + to_string(i);

		char* retval = (char*)FFI_malloc( sizeof(char) * (st.length() + 10));
		strcpy(retval, st.c_str());

		return retval;
	}
}

extern "C"{

int count_num_sockets = 1;

ssize_t parse_meta_response(struct msghdr *msg, string rgx){

	int retval = 0;
	bool isFound = false;
	std::regex value(rgx.c_str());

	for(int i = 0; i < msg->msg_iovlen; i++){

		retval += msg->msg_iov[i].iov_len;
		string st((char*)(msg->msg_iov[i].iov_base));

		if(std::regex_match(st, value)){
			isFound = true;
		}
	}

	assert(isFound && "Value not found in the return string");

	return retval;
}

// This function will be called when we recieve response for this request
ssize_t parse_get_response(struct msghdr *msg, string value){

	ssize_t retval = 0;

	if(msg->msg_iovlen <= 1){

		char* msg1 = (char*)( ((struct iovec *)(msg->msg_iov))->iov_base);
		retval = strlen(msg1);

		// This will happen when the iten is not in the kv store
		assert( strcmp(msg1, "END\r\n") == 0);
	}
	else{ // Means that some value is returned!

		char* msg1 = (char*)(msg->msg_iov->iov_base);
		char* msg2 = (char*)(msg->msg_iov[1].iov_base);
		retval = msg->msg_iov->iov_len + msg->msg_iov[1].iov_len + strlen("END\r\n");

		string st(msg2);
		assert( st.find(value) != string::npos );
	}

	return retval;
}

// Used for direct comparision of KV store's response to given value
ssize_t parse_generic_response(struct msghdr *msg, string value){

	string response((char*)(msg->msg_iov->iov_base));

	if((response.find(value) == string::npos)){

		string error_string("SERVER_ERROR");
		if((response.find(error_string) != string::npos) && !found_this_iteration){
			temp_counter++;
			found_this_iteration = true;
		}
		//printf("COuld not store KV pair into MC. Memory full. %s \n", (char*)(msg->msg_iov->iov_base) );
	}
	//assert(response.find(value) != string::npos && "We can find the string 'value' in KV store's respone");

	return (msg->msg_iov->iov_len);
}

ssize_t parse_watch_response(const char* msg, string value){

	string response((char*)(msg));

	if(response.find(value) == string::npos ){

		return -1;
	}

	return strlen(msg);
}

ssize_t parse_stats_slabs_response(struct msghdr *msg, string value){

	int retval = 0;
	bool isFound = false;

	for(int i = 0; i < msg->msg_iovlen; i++){

		retval += msg->msg_iov[i].iov_len;
		string st((char*)(msg->msg_iov[i].iov_base));

		// Check whether the metadump contains the required string or not
		if(st.find(value) != string::npos){
			isFound = true;
		}
	}

	assert(isFound && "Value not found in the return string");

	return retval;
}

ssize_t parse_stats_items_response(struct msghdr *msg, string value){

	int retval = 0;
	bool isFound = false;

	for(int i = 0; i < msg->msg_iovlen; i++){

		retval += msg->msg_iov[i].iov_len;
		string st((char*)(msg->msg_iov[i].iov_base));

		// Check whether the metadump contains the required string or not
		if(st.find(value) != string::npos){
			isFound = true;
		}
	}

	assert(isFound && "Value not found in the return string");

	return retval;
}

ssize_t parse_stats_settings_response(struct msghdr *msg, string value){

	int retval = 0;
	bool isFound = false;

	for(int i = 0; i < msg->msg_iovlen; i++){

		retval += msg->msg_iov[i].iov_len;
		string st((char*)(msg->msg_iov[i].iov_base));

		// Check whether the metadump contains the required string or not
		if(st.find(value) != string::npos){
			isFound = true;
		}
	}

	assert(isFound && "Value not found in the return string");

	return retval;
}

ssize_t parse_stats_gen_response(struct msghdr *msg, string value){

	int retval = 0;
	bool isFound = false;

	for(int i = 0; i < msg->msg_iovlen; i++){

		retval += msg->msg_iov[i].iov_len;
		string st((char*)(msg->msg_iov[i].iov_base));

		// Check whether the metadump contains the required string or not
		if(st.find(value) != string::npos){
			isFound = true;
		}
	}

	assert(isFound && "Value not found in the return string");

	return retval;
}

ssize_t parse_lru_crawler_metadump_response(char* buff, string value){

	int total_key_count = 0;
	int retval = strlen(buff);

	string data(buff);
	string toSearch("key=");

	// Get the first occurrence
    size_t pos = data.find(toSearch);

    // Repeat till end is reached
    while( pos != std::string::npos)
    {
        // Add position to the vector
        total_key_count++;

        // Get the next occurrence from the current position
        pos = data.find(toSearch, pos + toSearch.size());
    }

    assert(value == to_string(total_key_count) && "Make sure the total number of keys are same");

	return retval;
}

// Need 21 connections
void set_workload_2019_bugs(conn* obj){

	// For bug in watcher_add function
	obj->add_kv_cmd("watch\n");
	obj->set_expected_kv_resp(obj->char_to_string("watch"), obj->char_to_string("OK\r\n"));

	obj->set_expected_kv_resp(obj->char_to_string("watch"), obj->char_to_string("102000"));

	// For bug in item_cachedump
	obj->add_kv_cmd("item cachedump\n");
	obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("OK\r\n"));
}

void set_random_workload(conn* c){

	// Random number between 1 and 50
	int max = rand() % 50 + 1;
    int sets = 0;

    for (int i = 1; i <= max; i++) {

        char* key = get_key_name(i);
        c->set_key(key, key);
        sets++;
    }

    int max_iteration = rand() % 500;
    for (int i = 1; i < max_iteration; i++) {

    	// Random number from 1 to max
    	int ran = rand() % max + 1;
    	char* key = get_key_name(ran);
    	int meth = rand() % 3;
    	int exp = rand() % 3;

    	if(meth == 0){

    		c->add_key(key, key, exp);

    	} else if(meth == 1){

    		c->delete_key(key);

    	} else {

    		c->set_key(key, key, exp);
    	}

    	ran = rand() % max + 1;
    	key = get_key_name(ran);

    	c->get_and_assert_key(key, key);
    }

    for (int i = 1; i <= max; i++) {

        char* key = get_key_name(i);
        c->get_key(key);
        c->set_expected_kv_resp("get", "\r\n");
    }
}

void set_workload_lru(conn* obj){

	for(int i = 1; i <= 3; i++){

		char* key = get_key_name(i, 'i');
		obj->set_key(key, "ok", 0, 1);
		obj->set_expected_kv_resp("generic", "STORED\r\n");
	}

	for(int i = 1; i <= 3; i++){

		char* key = get_key_name(i, 'l');
		obj->set_key(key, "ok", 3600, 1);
		obj->set_expected_kv_resp("generic", "STORED\r\n");
	}

	for(int i = 1; i <= 3; i++){

		char* key = get_key_name(i, 's');
		obj->set_key(key, "ok", 1, 1);
		obj->set_expected_kv_resp("generic", "STORED\r\n");
	}

	obj->get_mem_stats_and_assert("slabs", "1:used_chunks", to_string(9));

	sleep(1);

	{
		string lru_crawler("lru_crawler enable\r\n");
		obj->add_kv_cmd(lru_crawler);
		// It can fail also becoz LRU crawler might be running
		obj->set_expected_kv_resp( obj->char_to_string("generic"), obj->char_to_string("OK\r\n") );

		string lru_crawler1("lru_crawler crawl 1\r\n");

		for(int i =0; i < 3000; i++){
			obj->add_kv_cmd(lru_crawler1);
			obj->set_expected_kv_resp( obj->char_to_string("generic"), obj->char_to_string("OK\r\n") );
		}

		obj->get_mem_stats_and_assert("slabs", "1:used_chunks", to_string(6));
		obj->get_mem_stats_and_assert("items", "items:1:crawler_reclaimed", to_string(3));

		string lru_crawler2("lru_crawler metadump all\r\n");
		obj->add_kv_cmd(lru_crawler2);
		obj->set_expected_kv_resp( obj->char_to_string("lru_crawler metadump"),
								   obj->char_to_string("6") );
	}

	for(int i = 1; i <= 30; i++){

		char* skey = get_key_name(i, 's');
		char* lkey = get_key_name(i, 'l');
		char* ikey = get_key_name(i, 'i');

		// We can put anything here. This should be undef
		obj->get_and_assert_key(skey, "k");
		obj->get_and_assert_key(lkey, "ok");
		obj->get_and_assert_key(ikey, "ok");
	}

	string lru_crawler3("lru_crawler disable\r\n");
	obj->add_kv_cmd(lru_crawler3);
	obj->set_expected_kv_resp( obj->char_to_string("generic"), obj->char_to_string("OK\r\n") );

	// Again store the keys with TTL=1sec
	for(int i = 1; i <= 30; i++){

		char* key = get_key_name(i, 's');
		obj->set_key(key, "ok", 1, 1);
		obj->set_expected_kv_resp("generic", "STORED\r\n");
	}

	sleep(3);

	{
		string lru_crawler("lru_crawler enable\r\n");
		obj->add_kv_cmd(lru_crawler);
		obj->set_expected_kv_resp( obj->char_to_string("generic"), obj->char_to_string("OK\r\n") );

		string lru_crawler1("lru_crawler crawl 1\r\n");
		for(int i =0; i < 2000; i++){
			obj->add_kv_cmd(lru_crawler1);
			obj->set_expected_kv_resp( obj->char_to_string("generic"), obj->char_to_string("\r\n") );
		}
	}

	obj->get_mem_stats_and_assert("slabs", "1:used_chunks", to_string(6));
}

void set_workload_extstore(conn* obj){

	// Generate a latge workload
	char* long_val = (char*)FFI_malloc(sizeof(char) * 1000 * 5);
	memset(long_val, 'C', sizeof(char) * 1000 * 5); // Store a char per byte

	for(int i = 1; i <= 2; i++){

		char* key = get_key_name(i);
		obj->set_key(key, long_val, 0); // Store the keys for infinite time
	}

	// Pehaps we first need to make sure the extstore thread did its job

	for(int i = 1; i <= 2; i++){

		char* key = get_key_name(i);

		if(i < 1)
			obj->incr_key(key, 1); // increment the keys
		else
			obj->decr_key(key, 1); // Decrement the keys

		obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("CLIENT_ERROR cannot increment or decrement non-numeric value\r\n"));
	}

	// append a string to the values of a various keys
	for(int i = 1; i <= 2; i++){

		char* key = get_key_name(i);

		if(i <= 1)
			obj->append_key(key, "hello");
		else
			obj->prepend_key(key, "hello");

		obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
	}
}

void set_workload_slab_rebalance(conn* obj){

	// Make sure that slab_reassign option is set
	obj->get_mem_stats_and_assert("settings", "slab_reassign", obj->char_to_string("yes"));

	// This should consume the entire cache. Slab id is 23
	char* long_val = (char*)FFI_malloc(sizeof(char) * 1024 * 12);
	memset(long_val, 'x', sizeof(char) * 1024 * 12); // Store a char per byte
	long_val[(1024*12) - 1] = '\0';

	for(int i = 1; i <= 75; i++){

		char* key = get_key_name(i);
		obj->set_key(key, long_val, 0, true, 1024*12 - 1); // For infinite time
		obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
	}

	// This should take 1/4 of the total cache. Slab id: 19
	char* small_val = (char*)FFI_malloc(sizeof(char) * 1024 * 5);
	memset(small_val, 'y', sizeof(char) * 1024 * 5); // Store a char per byte
	small_val[(5*1024) - 1] = '\0';

	for(int i = 1; i <= 50; i++){

		char* key = get_key_name(i);
		obj->set_key(key, small_val, 0, true, 5*1024 - 1); // For infinite time
		obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
	}

	// TODO: Use RegEx here
	// These should fail!
	// obj->get_mem_stats_and_assert("items", "items", obj->char_to_string(""));
	//obj->get_stats_and_eval_regex("items", "*items:19:evicted [1-]");
	//obj->get_mem_stats_and_assert("items", "items:25:evicted", to_string(0));

	obj->add_kv_cmd("slabs reassign invalid1 invalid2\r\n");
	obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("CLIENT_ERROR bad command line format\r\n"));

	// Move pages from any valid slab class to slab 23
	obj->add_kv_cmd("slabs reassign 23 19\r\n");
	obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("\r\n"));

	// General
	//obj->get_mem_stats_and_assert("gen", "slabs_moved", to_string(1));

	obj->add_kv_cmd("slabs reassign 19 23\r\n");
	obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("\r\n"));

	//obj->get_mem_stats_and_assert("gen", "slabs_moved", to_string(1));
}

void set_workload_logger(conn* obj){

	static int prev_conn_id = obj->conn_id;

	// Watcher connectiobn
	if(prev_conn_id == obj->conn_id){

		obj->add_kv_cmd("watch\n");
		obj->set_expected_kv_resp(obj->char_to_string("watch"), obj->char_to_string("OK\r\n"));

		obj->set_expected_kv_resp(obj->char_to_string("watch"), obj->char_to_string("102000"));

	}else{
	// Worker connection

		obj->get_and_assert_key("foo", "END");

		// Have big key names
		for(int i = 100000; i <= 100100; i++){

			char* key = get_key_name(i);
			obj->get_and_assert_key(key, "END");
		}
	}
}

void set_large_workload(conn* obj){

	// Each value is of 4 KB
	char* long_val = (char*)FFI_malloc(sizeof(char) * 1000 * 4);
	memset(long_val, '1', sizeof(char) * 1000 * 4);

	for(int i = 1; i <= 500; i++){

		char* key = get_key_name(i);
		obj->set_key(key, long_val, 1);
	}

	for(int i = 1; i <= 500; i++){

		char* key = get_key_name(i, 's');
		obj->set_key(key, long_val, 10);
	}

	for(int i = 1; i <= 500; i++){

		char* key = get_key_name(i, 'i');
		obj->set_key(key, long_val, 0);
	}

	for(int i = 1; i <= 500; i++){

		char* key = get_key_name(i, 'i');
		obj->get_key(key);

		key = get_key_name(i, 's');
		obj->get_key(key);

		key = get_key_name(i);
		obj->get_key(key);
	}
}

void set_workload_meta_cmds(conn* obj){

	obj->add_kv_cmd("ma mo\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(.*)\r\n"));

    obj->add_kv_cmd("ma mo D1\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(.*)\r\n"));

    obj->add_kv_cmd("set mo 0 0 1\r\n1\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("\r\n"));

    obj->add_kv_cmd("ma mo\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(.*)\r\n"));

    obj->add_kv_cmd("set mo 0 0 1\r\nq\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("\r\n"));

    obj->add_kv_cmd("ma mo\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(CLIENT_ERROR|OK)(.*)\r\n"));

    obj->add_kv_cmd("ma key1 N90\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(OK)(.*)\r\n"));

    obj->add_kv_cmd("mg key1 s t v Ofoo k\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA 1[ ])(s[0-9][ ])(t(([1-8][0-9])|90)[ ])(Ofoo[ ])(.*)\r\n" ));

    obj->add_kv_cmd("ma mi N90 J13 v t\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA)(.*)\r\n(13|14|15|44|45|46|74|75|76)\r\n"));

    obj->add_kv_cmd("ma mi N90 J13 v t\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA)(.*)\r\n(14|15|16|44|45|46|74|75|76)\r\n"));

    obj->add_kv_cmd("ma mi N90 J13 v t D30\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA)(.*)\r\n(44|45|46|74|75|76)\r\n"));

    obj->add_kv_cmd("ma mi N90 J13 v t MD D30\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA)(.*)\r\n(44|45|46|14|15|16)\r\n"));

    obj->add_kv_cmd("ma mi N0 C99999 v\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(EX)(.*)\r\n"));
}

void set_workload_generic_testcase(conn* obj){

	static int prev_conn_id = obj->conn_id;

	// Worker connection
	obj->get_and_assert_key("foo", "END");

	obj->add_kv_cmd("ma mo\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(.*)\r\n"));

    obj->add_kv_cmd("ma mo D1\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(.*)\r\n"));

    obj->add_kv_cmd("set mo 0 0 1\r\n1\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("\r\n"));

    obj->add_kv_cmd("ma key1 N90\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(OK)(.*)\r\n"));

    obj->add_kv_cmd("mg key1 s t v Ofoo k\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("((VA 1[ ])(s[0-9][ ])(t(([1-8][0-9])|90)[ ])(Ofoo[ ])(.*)\r\n)|(EN(.*)\r\n)" ));

    obj->add_kv_cmd("ma mi N90 J13 v t\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA)(.*)\r\n(.*)\r\n"));

    obj->add_kv_cmd("ma mi N90 J13 v t\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA)(.*)\r\n(.*)\r\n"));

    obj->add_kv_cmd("ma mi N90 J13 v t D30\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA)(.*)\r\n(.*)\r\n"));

    obj->add_kv_cmd("ma mi N90 J13 v t MD D30\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("(VA)(.*)\r\n(.*)\r\n"));

    obj->add_kv_cmd("ma mi N0 C99999 v\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("meta"), obj->char_to_string("((.*)\r\n(.*)\r\n)|((.*)\r\n)"));

    obj->add_kv_cmd("flush_all\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("OK\r\n"));

    set_workload_slab_rebalance(obj);

    obj->add_kv_cmd("flush_all\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("OK\r\n"));

    set_random_workload(obj);

    obj->add_kv_cmd("flush_all\r\n");
    obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("OK\r\n"));

    //set_workload_lru(obj);
    //set_workload_extstore(obj);
    //TODO: Test misbehave command
}

/*
* a_1 a_2 b_1 b_2
* a_1 b_1 a_2 b_2
* a_1 b_1 b_2 a_2
* b_1 b_2 a_1 a_2
* b_1 a_1 b_2 a_2
* b_1 a_1 a_2 b_2
*/
void set_coverage_workload(conn* c){

	// 2 threads, trying to insert and get 4 unique, KV pairs
	int max = 4;

	// Each value is of 100 Bytes
	char* short_val = (char*)FFI_malloc(sizeof(char) * 100);
	memset(short_val, '1', sizeof(char) * 100);
	short_val[100 - 1] = '\0';

    // Each value is of 4 KB
	char* long_val = (char*)FFI_malloc(sizeof(char) * 4000);
	memset(long_val, '1', sizeof(char) * 4000);
	long_val[4000 - 1] = '\0';

	// Set KV pair
    for (int i = 1; i <= max; i++) {

    	if(i < (max / 2) + 1){
	        char* key = get_key_name(i * c->conn_id);
	        if(i%2)
	        	c->set_key(key, short_val, 0, true);
	        else
	        	c->set_key(key, short_val, 2, true);

	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
    	}
    	else{
    		char* key = get_key_name(i * c->conn_id);
    		if(i%2)
	        	c->set_key(key, long_val, 0, true);
	        else
	        	c->set_key(key, long_val, 2, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
    	}
    }

    // Get KV Pair
    for (int i = 1; i <= max; i++) {

    	if(i < (max / 2) + 1){
	        char* key = get_key_name(i * c->conn_id);
        	c->get_and_assert_key(key, short_val);
        	c->get_and_assert_key(key, short_val);
    	}
    	else{
			char* key = get_key_name(i * c->conn_id);
        	c->get_and_assert_key(key, long_val);
        	c->get_and_assert_key(key, long_val);
    	}
    }
}

void set_coverage_workload_large_items(conn* c){

	c->add_kv_cmd("slabs automove 2\r\n");
	c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("OK\r\n"));

	// 2 threads, trying to insert and get 4 unique, KV pairs
	int max = 20;

	// Each value is of 100 Bytes
	char* short_val = (char*)FFI_malloc(sizeof(char) * 100);
	memset(short_val, '1', sizeof(char) * 100);
	short_val[100 - 1] = '\0';

    // Each value is of 4 KB
	char* long_val = (char*)FFI_malloc(sizeof(char) * 4000);
	memset(long_val, '1', sizeof(char) * 4000);
	long_val[4000 - 1] = '\0';

	// Set KV pair
    for (int i = 1; i <= max; i++) {

    	if(i < 6){
    		//Slab id:4
	        char* key = get_key_name(i * c->conn_id);
	        c->set_key(key, short_val, 1, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
    	}
    	else{
    		//Slab id:18
    		char* key = get_key_name(i * c->conn_id);
	        c->set_key(key, long_val, 0, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
    	}
    }

    c->add_kv_cmd("slabs reassign 4 18\r\n");
	c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("\r\n"));

    /*
    // Get KV Pair
    for (int i = 1; i <= max; i++) {

        char* key = get_key_name(i * c->conn_id);
        c->get_and_assert_key(key, long_val);
    }
    */
}

void set_coverage_workload_lru(conn* c){

	// 2 threads, trying to insert and get 4 unique, KV pairs
	int max = 4;

	// Each value is of 10 Bytes
	char* short_val = (char*)FFI_malloc(sizeof(char) * 10);
	memset(short_val, '1', sizeof(char) * 10);
	short_val[10 - 1] = '\0';

    // Each value is of 1 KB
	char* long_val = (char*)FFI_malloc(sizeof(char) * 1000);
	memset(long_val, '1', sizeof(char) * 1000);
	long_val[1000 - 1] = '\0';

	// Set KV pair
    for (int i = 1; i <= max; i++) {

    	if(i < 101){
    		//Slab id:4
	        char* key = get_key_name(i * c->conn_id);
	        c->set_key(key, short_val, 1, false, 10);
	        //c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
    	}
    	else{
    		//Slab id:18
    		char* key = get_key_name(i * c->conn_id);
	        c->set_key(key, long_val, 0, false, 1000);
	        //c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
    	}
    }
}

void set_coverage_workload_kv_store(conn* c){

	static int tid = c->conn_id;
	// 2 threads, trying to insert 10 unique, KV pairs
	int max = 10;

	// Each value is of 10 Bytes
	char* short_val = (char*)FFI_malloc(sizeof(char) * 10);
	memset(short_val, '0', sizeof(char) * 10);
	short_val[10 - 1] = '\0';

    // Each value is of 1 KB
	char* long_val = (char*)FFI_malloc(sizeof(char) * 1000);
	memset(long_val, '1', sizeof(char) * 1000);
	long_val[1000 - 1] = '\0';

	if(tid == c->conn_id){

	    for (int i = 1; i <= max; i++) {

    		char* key = get_key_name(i);
	        c->set_key(key, long_val, 0, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
	    }
	} else {

		// Prepend key
	    for (int i = 1; i <= max; i++) {

    		char* key = get_key_name(i);
	        c->prepend_key(key, short_val, 0);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("\r\n"));
	    }
	}
}

void sucess_rate_for_lru_maintainer(conn* c){

	c->add_kv_cmd("slabs automove 0\r\n");
	c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("\r\n"));

	string lru_crawler("lru_crawler enable\r\n");
	c->add_kv_cmd(lru_crawler);
	c->set_expected_kv_resp( c->char_to_string("generic"), c->char_to_string("\r\n") );

	string lru_crawler2("lru_crawler crawl 3\r\n");
	c->add_kv_cmd(lru_crawler2);
	c->set_expected_kv_resp( c->char_to_string("generic"), c->char_to_string("OK\r\n") );

	string lru_crawler1("lru_crawler crawl -1\r\n");
	c->add_kv_cmd(lru_crawler1);
	c->set_expected_kv_resp( c->char_to_string("generic"), c->char_to_string("OK\r\n") );

	static int tid = c->conn_id;
	// 2 threads, trying to insert and get 4 unique, KV pairs
	int max = 2;

    // Each value is of 4 KB
	char* long_val = (char*)FFI_malloc(sizeof(char) * 4000);
	memset(long_val, '1', sizeof(char) * 4000);
	long_val[4000 - 1] = '\0';

	if(tid == c->conn_id){

        for (int i = 1; i <= max; i++) {

    		char* key = get_key_name(i);
	        c->set_key(key, long_val, 0, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
	    }

	    c->set_random_block();

	} else {

	    for (int i = 1; i <= max; i++) {

    		char* key = get_key_name(i);
	        c->set_key(key, long_val, 2, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
	    }

        c->set_random_block();
	}
}

void sucess_rate_for_slab_rebalancer(conn* obj){

	static int first_conn_id = obj->conn_id;

	// This should consume the entire cache. Slab id is 23
	char* long_val = (char*)FFI_malloc(sizeof(char) * 1024 * 12);
	memset(long_val, 'x', sizeof(char) * 1024 * 12); // Store a char per byte
	long_val[(1024*12) - 1] = '\0';

	// This should take 1/4 of the total cache. Slab id: 19
	char* small_val = (char*)FFI_malloc(sizeof(char) * 1024 * 5);
	memset(small_val, 'y', sizeof(char) * 1024 * 5); // Store a char per byte
	small_val[(5*1024) - 1] = '\0';

	if(first_conn_id == obj->conn_id){

		obj->add_kv_cmd("slabs automove 0\r\n");
		obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("OK\r\n"));

		// Make sure that slab_reassign option is set
		obj->get_mem_stats_and_assert("settings", "slab_reassign", obj->char_to_string("yes"));

		for(int i = 1; i <= 80; i++){

			char* key = get_key_name(i);
			obj->set_key(key, long_val, 0, true, 1024*12 - 1); // For infinite time
			obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
		}

		// Move pages from any valid slab class to slab 23
		obj->add_kv_cmd("slabs reassign 23 19\r\n");
		obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("OK\r\n"));

		for(int i = 81; i <= 90; i++){

			char* key = get_key_name(i);
			obj->set_key(key, small_val, 0, true, 5*1024 - 1); // For infinite time
			obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
		}

		for(int i = 141; i <= 150; i++){

			char* key = get_key_name(i);
			obj->set_key(key, long_val, 0, true, 12*1024 - 1); // For infinite time
			obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
		}
	}
	else
	{
		// This should trigger the corner case when you are trying to insert a KV pair
		// when slab reassigner thread is working
		for(int i = 1; i <= 50; i++){

			char* key = get_key_name(i*obj->conn_id);
			obj->set_key(key, long_val, 0, true, 1024*12 - 1); // For infinite time
			obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
		}

		/*
		for(int i = 51; i <= 200; i++){

			char* key = get_key_name(i*obj->conn_id);
			obj->set_key(key, small_val, 0, true, 12*1024 - 1); // For infinite time
			obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
		}
		*/
	}
}

void deadlock_bug_slab_rebalancer(conn* obj){

	// This should consume the entire cache. Slab id is 23
	char* long_val = (char*)FFI_malloc(sizeof(char) * 1024 * 12);
	memset(long_val, 'x', sizeof(char) * 1024 * 12); // Store a char per byte
	long_val[(1024*12) - 1] = '\0';

	// This should take 1/4 of the total cache. Slab id: 19
	char* small_val = (char*)FFI_malloc(sizeof(char) * 1024 * 5);
	memset(small_val, 'y', sizeof(char) * 1024 * 5); // Store a char per byte
	small_val[(5*1024) - 1] = '\0';

	obj->add_kv_cmd("slabs automove 0\r\n");
	obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("OK\r\n"));

	// Make sure that slab_reassign option is set
	obj->get_mem_stats_and_assert("settings", "slab_reassign", obj->char_to_string("yes"));

	for(int i = 1; i <= 130; i++){

		char* key = get_key_name(i);
		obj->set_key(key, long_val, 0, true, 1024*12 - 1); // For infinite time
		obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("STORED\r\n"));
	}

	// Move pages from any valid slab class to slab 23
	obj->add_kv_cmd("slabs reassign 23 19\r\n");
	obj->set_expected_kv_resp(obj->char_to_string("generic"), obj->char_to_string("OK\r\n"));
}

void set_coverage_workload_slab(conn* c){

	c->add_kv_cmd("slabs automove 2\r\n");
	c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("OK\r\n"));

	static int tid = c->conn_id;
	// 2 threads, trying to insert and get 4 unique, KV pairs
	int max = 10;

	// Each value is of 10 Bytes
	char* short_val = (char*)FFI_malloc(sizeof(char) * 10);
	memset(short_val, '1', sizeof(char) * 10);
	short_val[10 - 1] = '\0';

    // Each value is of 4 KB
	char* long_val = (char*)FFI_malloc(sizeof(char) * 4000);
	memset(long_val, '1', sizeof(char) * 4000);
	long_val[4000 - 1] = '\0';

	if(tid == c->conn_id){

        for (int i = 1; i <= max; i++) {

    		char* key = get_key_name(i);
	        c->set_key(key, long_val, 0, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
	    }

	    // In another slab
	    char* key = get_key_name(1000);
        c->set_key(key, key, 0, true);
        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));

	    c->set_random_block();

	} else {

		// Delete KV pair
	    for (int i = 1; i <= max; i++) {

    		char* key = get_key_name(i);
	        c->delete_key(key);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("\r\n"));
	    }

        c->set_random_block();
	}
}

void test_case_for_finding_injected_bug_1(conn* c){


	static int tid = c->conn_id;

	if(tid == c->conn_id){

		char* key = get_key_name(1);
        c->set_key(key, "1", 0, true);
        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));

        c->incr_key(key, 1);
		c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("\r\n"));

	    c->set_random_block();
	} else {

		char* key = get_key_name(1);

		c->incr_key(key, 1);
		c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("\r\n"));

        c->set_random_block();
	}
}

void set_coverage_workload_slab_equal_workload(conn* c){

	c->add_kv_cmd("slabs automove 2\r\n");
	c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("OK\r\n"));

	static int tid = c->conn_id;
	// 2 threads, trying to insert and get 4 unique, KV pairs
	int max = 5;

	// Each value is of 10 Bytes
	char* short_val = (char*)FFI_malloc(sizeof(char) * 10);
	memset(short_val, '1', sizeof(char) * 10);
	short_val[10 - 1] = '\0';

    // Each value is of 4 KB
	char* long_val = (char*)FFI_malloc(sizeof(char) * 4000);
	memset(long_val, '1', sizeof(char) * 4000);
	long_val[4000 - 1] = '\0';

	// One thread will insert keys from [1,5] and deleter keys from [6, 11]
	if(tid == c->conn_id){

	    for (int i = 1; i <= max; i++) {

    		char* key = get_key_name(i);
    		if(i%2)
	        	c->set_key(key, long_val, 0, true);
	        else
	        	c->set_key(key, short_val, 0, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
	    }

	    for (int i = max+1; i <= (2*max + 1); i++) {

    		char* key = get_key_name(i);
	        c->delete_key(key);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("\r\n"));
	    }

	// Second thread will insert keys from [6,11] and deleter keys from [1, 5]
	} else {

		for (int i = 1; i <= (max); i++) {

    		char* key = get_key_name(i);
	        c->delete_key(key);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("\r\n"));
	    }

		for (int i = max+1; i <= (2*max + 1); i++) {

    		char* key = get_key_name(i);
	        if(i%2)
	        	c->set_key(key, long_val, 0, true);
	        else
	        	c->set_key(key, short_val, 0, true);
	        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));
	    }
	}
}

void reproduce_stats_sizes_bug(conn* c){

	static int cid = c->conn_id;

	if(cid == c->conn_id){

		char* key = get_key_name(1);
		c->set_key(key, key, 0, true);
        c->set_expected_kv_resp(c->char_to_string("generic"), c->char_to_string("STORED\r\n"));

	}else{

		c->get_mem_stats_and_assert("sizes_disable","STAT sizes_status", c->char_to_string("disabled\r\n"));
	}
}

void init_sockets(){

	if(global_conns == NULL){
		global_conns = new vector<conn*>();
	}

	for(int i=0; i < count_num_sockets; i++){

		conn* new_con = new conn();
		//set_workload(new_con);  // <<--- General Stress testing of memcached
		//set_workload_lru(new_con); // <<--- For testing LRU crawler thread
		//set_workload_extstore(new_con); // <<-- For testing extstore thread
		//set_workload_slab_rebalance(new_con); // <<-- For slab rebalance thread
		//set_workload_logger(new_con); // <<-- For testing logger thread
		//set_workload_meta_cmds(new_con); // <<-- For testing meta commands
		//set_workload_generic_testcase(new_con); // <<-- For Generic test case
		//set_coverage_workload(new_con); // <<-- For coverage test case
		//set_coverage_workload_large_items(new_con);
		//reproduce_stats_sizes_bug(new_con);
		//set_coverage_workload_lru(new_con);
		//set_coverage_workload_slab(new_con);
		//test_case_for_finding_injected_bug_1(new_con);
		//set_coverage_workload_slab_equal_workload(new_con);
		//set_coverage_workload_kv_store(new_con);
		//sucess_rate_for_lru_maintainer(new_con);
		//sucess_rate_for_slab_rebalancer(new_con);
		deadlock_bug_slab_rebalancer(new_con);
		global_conns->push_back(new_con);
	}
}

void del_sockets(){

	for(int i = 0; i < count_num_sockets; i++){
		conn* c = global_conns->at(i);
		delete c;
		c = NULL;
	}

	delete global_conns;
	global_conns = NULL;
	num_conn_registered = 0;

	socket_counter = 200;
	delete map_fd_to_conn;
	map_fd_to_conn = NULL;

	found_this_iteration = false;
}

bool CT_is_socket(int fd){

	// Check if this is in the map of allocated connections
	return (map_fd_to_conn->find(fd) != map_fd_to_conn->end());
}

ssize_t CT_socket_write(int fd, void* buff, int count){

	assert(CT_is_socket(fd) && "This is not the socket we have opened!");

	map<int, void*>::iterator it = map_fd_to_conn->find(fd);
	conn* obj = (conn*)(it->second);

	string st = obj->get_next_cmd();
	//char msg[10240];

	// Convert string object to C strings and copy it info the buffer
	//strcpy(msg, st.c_str());
	//assert(strlen(st.c_str()) <= count);

	memcpy(buff, st.c_str(), strlen(st.c_str()));

	//printf("Thread id:%lu, reading command: %s \n", pthread_self(), st.c_str());
	return strlen(st.c_str());
}

ssize_t CT_socket_read(int fd, const void* buff, int count){

	assert(CT_is_socket(fd));

	conn* obj = NULL;

	{
		// Get the conn object corresponding to this fd
		map<int, void*>::iterator it = map_fd_to_conn->find(fd);
		obj = (conn*)(it->second);
	}

	vector< pair<string, string>* >::iterator it = obj->expected_response->begin();

	// Check if the container is not empty
	if(it != obj->expected_response->end()){
		pair<string, string>* p = *it;

		if(p->first == obj->char_to_string("lru_crawler metadump")){

			ssize_t retval = parse_lru_crawler_metadump_response((char*)buff, p->second);

			printf("Recieved on connection number %d, lru crawler metadump with keys: %s ", fd, (p->second).c_str());

			delete p;
			obj->expected_response->erase(it);

			return count;
		}

		if(p->first == obj->char_to_string("watch")){

			ssize_t retval = parse_watch_response((char*)buff, p->second);

			printf("Recieved on connection number %d, watch with keys: %s ", fd, (char*)buff);

			if(retval != -1){
				delete p;
				obj->expected_response->erase(it);
			}
			else
				return count; // Return only a fraction of te total data. This will cause the buffer to get full

			return count;
		}
	}
	else{

		// If the client is not expecting any response, then just exit this function!
		return 0;
	}

	assert(0);
	return 0;
}

ssize_t CT_socket_recvmsg(int fd, struct msghdr *msg, int flags){

	assert(CT_is_socket(fd));

	conn* obj = NULL;

	{
		// Get the conn object corresponding to this fd
		map<int, void*>::iterator it = map_fd_to_conn->find(fd);
		obj = (conn*)(it->second);
	}

	vector< pair<string, string>* >::iterator it = obj->expected_response->begin();

	// Check if the container is not empty
	if(it != obj->expected_response->end()){

		pair<string, string>* p = *it;

		if(p->first == obj->char_to_string("get")){

			ssize_t retval = parse_get_response(msg, p->second);

			printf("Recieved on connection number %d, msg: %s", fd, (char*)(msg->msg_iov->iov_base));

			delete p;
			obj->expected_response->erase(it);
			return retval;
		}

		if(p->first == obj->char_to_string("stats slabs")){

			ssize_t retval = parse_stats_slabs_response(msg, p->second);

			printf("Recieved on connection number %d, metadump with val: %s", fd, (p->second).c_str());

			delete p;
			obj->expected_response->erase(it);
			return retval;
		}

		if(p->first == obj->char_to_string("stats items")){

			ssize_t retval = parse_stats_items_response(msg, p->second);

			printf("Recieved on connection number %d, metadump with val: %s", fd, (p->second).c_str());

			delete p;
			obj->expected_response->erase(it);
			return retval;
		}

		if(p->first == obj->char_to_string("stats settings")){

			ssize_t retval = parse_stats_settings_response(msg, p->second);

			printf("Recieved on connection number %d, metadump with val: %s", fd, (p->second).c_str());

			delete p;
			obj->expected_response->erase(it);
			return retval;
		}

		if(p->first == obj->char_to_string("stats gen")){

			ssize_t retval = parse_stats_gen_response(msg, p->second);

			printf("Recieved on connection number %d, metadump with val: %s", fd, (p->second).c_str());

			delete p;
			obj->expected_response->erase(it);
			return retval;
		}

		if(p->first == obj->char_to_string("generic")){

			ssize_t retval = parse_generic_response(msg, p->second);

			printf("Recieved on connection number %d, msg: %s", fd, (char*)(msg->msg_iov->iov_base));

			delete p;
			obj->expected_response->erase(it);
			return retval;
		}

		if(p->first == obj->char_to_string("meta")){

			ssize_t retval = parse_meta_response(msg, p->second);

			printf("Recieved on connection number %d, msg: %s", fd, (char*)(msg->msg_iov->iov_base));

			delete p;
			obj->expected_response->erase(it);
			return retval;
		}

	}

	printf("Recieved on connection number %d, msg: %s", fd, (char*)(msg->msg_iov->iov_base));
    assert(0);
	return strlen((char*)(msg->msg_iov->iov_base));
}

int CT_new_socket(){

	static int i = 0;

	// Block the dispatcher thread, when we have already created all the sockets
	if(count_num_sockets == i){

		// If all workers threads have not completed their operations
		if(num_conn_registered != count_num_sockets){
			return 0;
		}
		else{
			// Close the server. Time to end this test case
			i = 0;
			return -1;
		}
	}

	conn* c = global_conns->at(i);
	i++;

	return c->conn_id;
}

int set_options(int argc, char** argv, char** new_argv){

	int i = 0;
	for(i = 0; i < argc; i++){
		memcpy(new_argv[i], argv[i], 500);
	}

	//no_hashexpand,no_modern
	//char new_opt[7][500] = {"-m", "2", "-t", "2", "-M", "-o", "hashpower=16,slab_reassign"}; // <-- For Coverage
	//char new_opt[6][30] = {"-m", "32", "-t", "2", "-o", "slab_reassign"}; // <-- Generic test case
	//char new_opt[6][30] = {"-m", "32", "-o", "no_modern", "-t", "1"}; // <-- For Lru crawler testcase
	//char new_opt[7][500] = {"-m", "64", "-U", "0", "-o", "ext_page_size=8,ext_wbuf_size=2,ext_threads=1,ext_io_depth=2,ext_item_size=512,ext_item_age=2,ext_recache_rate=10000,ext_max_frag=0.9,ext_path=/temp/extstore.1:64m,slab_automove=0,ext_compact_under=1"};
	char new_opt[8][200] = {"-m", "2", "-M", "-o", "hashpower=16,slab_reassign,no_lru_crawler,no_lru_maintainer,no_hashexpand", "-t", "1"};
	//char new_opt[4][100] = {"-m", "60", "-o", "watcher_logbuf_size=8"};
	int num_new_opt = 7;

	for(int j = 0; i < (argc + num_new_opt); i++, j++){

		memcpy(new_argv[i], new_opt[j], 500);
	}

	return argc + num_new_opt;
}

char file_name[200] = "/home/udit/memcached_2020/memcached/coyotest/memcached_coverage.txt";
bool is_file_init = false;

void store_to_file(int itr, int size){

	if(is_file_init ==  false){
		is_file_init = true;

		// Don't check the error code
		remove( file_name );

		// create a file now
		std::fstream file;
		file.open( file_name, ios::out);
		if(!file){
			std::cout<<"File not created!! \n";
		}
		file<<"x,y"<<endl;
		file.close();
	}

	std::ofstream file;
	file.open( file_name, std::ios_base::app);
	file<<itr<<","<<size<<endl;

	file.flush();
	file.close();
}

std::vector<uint64_t>* all_hv = NULL;
void check_and_add(uint64_t hv, int itr){

	if(all_hv == NULL){
		all_hv = new std::vector<uint64_t>();
	}

	std::vector<uint64_t>::iterator it = std::find(all_hv->begin(), all_hv->end(), hv);

	// If we havn't found this hv before, insert it!
	if(it == all_hv->end()){
		all_hv->push_back(hv);
	}

	int total_size = all_hv->size();
	store_to_file(itr, total_size);
}

void print_and_clear_hvs(int total_iter){

	assert(all_hv != NULL);

	printf("Total states %lu found in %d iterations\n", all_hv->size(), total_iter);

	delete all_hv;
	all_hv = NULL;
}

// Allow printfs from main function
#undef printf

// Test main method
int CT_main( int (*run_iteration)(int, char**), void (*reset_all_globals)(void), uint64_t (get_program_state)(void), int argc, char** argv ){

	//FFI_create_scheduler_w_seed(1603350760484341101);
	FFI_create_scheduler();

	int num_iter = 200;

	char **new_argv = (char **)malloc(50 * sizeof(char *));
	for(int i = 0; i < 50; i++){
		new_argv[i] = (char *)malloc(500 * sizeof(char));
	}

	int new_argc = set_options(argc, argv, new_argv);

	FILE *filePointer;
	// Lights, Camera, Action!
	for(int j = 0; j < num_iter; j++){

		FFI_attach_scheduler();

		printf("Starting iteration #%d seed: %lu \n", j, FFI_seed());

		filePointer = fopen("coyote_output.txt", "a+");
        	fprintf(filePointer, "starting iteration: %d\n", j);
        	fclose(filePointer);

		init_sockets();

		run_iteration(new_argc, new_argv);

		// Take the hash of all the subsystems
		uint64_t hash = get_program_state();
		check_and_add(hash, j);
		printf("Hash of this iteration is %lu \n", hash);
		printf("Number of OOMs found: %d \n", temp_counter);

		reset_all_globals(); // For resetting globals and libevent
		FFI_free_all(); // For heap allocations

		FFI_detach_scheduler();
		FFI_scheduler_assert();

		del_sockets(); // For resetting client connection sockets
	}

	FFI_delete_scheduler();
	print_and_clear_hvs(num_iter);

	printf("We could find the OOM error %d number of times\n", temp_counter);

	for(int i = 0; i < 50; i++){
		free(new_argv[i]);
	}
	free(new_argv);

	return 0;
}

} /* extern "C" */
