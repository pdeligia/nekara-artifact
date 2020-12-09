#include <cassert>
#include <stdlib.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <cstring>
#include <vector>
#include <string>
#include <map>
#include <unistd.h>
#include <csignal>
#include <atomic>

extern "C"{
	#include <coyote_c_ffi.h>
}

using namespace std;

static int socket_counter = 200;
map<int, void*>* map_fd_to_conn = NULL;
int num_conn_registered = 0;

#define EXECUTION_COYOTE_CONTROLLED

//#define ENABLE_BLOCK_AND_SIGNAL

#ifdef EXECUTION_COYOTE_CONTROLLED
static volatile bool block_and_signal_is_blocked = false;
static long unsigned blocked_thread_id = 0;
#endif

struct conn{

	int conn_id; /* Unique dentifier of every connection */
	vector<string>* kv_response;
	vector<string>* kv_cmd;
	int output_counter;
	int input_counter;

	// We need a pair to indicate the type and value of response
	vector< pair<string, string>* >* expected_response;

	void add_kv_cmd(string ip){

		assert(kv_cmd != NULL);
		kv_cmd->push_back(ip);
	}

	string char_to_string(const char* inp){

		string a(inp);
		return a;
	}

	void set_key(const char* key, const char* val, int expr = 0, bool isReply = false, unsigned int size = 0){

		string base("set ");
		base = base + char_to_string(key);
		base = base + char_to_string(" 01 ") + to_string(expr);

		if(size)
			base = base + char_to_string(" ") + to_string(size);
		else
			base = base + char_to_string(" ") + to_string(strlen(val));

		if(isReply)
			base = base + char_to_string("\r\n") + char_to_string(val);
		else
			base = base + char_to_string(" noreply\r\n") + char_to_string(val);

		base = base + char_to_string("\r\n");

		add_kv_cmd(base);
	}

	void add_key(const char* key, const char* val, int expr = 0){

		string base("add ");
		base = base + char_to_string(key);
		base = base + char_to_string(" 01 ") + to_string(expr);
		base = base + char_to_string(" ") + to_string(strlen(val));
		base = base + char_to_string(" noreply\r\n") + char_to_string(val);
		base = base + char_to_string("\r\n");

		add_kv_cmd(base);
	}

	void delete_key(const char* key, bool reply = true){

		string base("delete ");
		base = base + char_to_string(key);
		if(!reply)
			base = base + char_to_string(" noreply");
		base = base + char_to_string("\r\n");

		add_kv_cmd(base);
	}

	void set_random_block(){

#ifdef EXECUTION_COYOTE_CONTROLLED

		int size = kv_cmd->size();
		int random_num = FFI_next_integer(size+1) % (size + 1);

		string st("BlockAndSignal");
		std::vector<string>::iterator it = kv_cmd->begin();

		kv_cmd->insert(it + random_num, st);
#endif
	}

	void incr_key(const char* key, const int val){

		string base("incr ");
		base = base + char_to_string(key);
		base = base + char_to_string(" ") + to_string(val);
		base = base + char_to_string("\r\n");

		add_kv_cmd(base);
	}

	void decr_key(const char* key, const int val){

		string base("decr ");
		base = base + char_to_string(key);
		base = base + char_to_string(" ") + to_string(val);
		base = base + char_to_string("\r\n");

		add_kv_cmd(base);
	}

	void append_key(const char* key, const char* val, int expr = 0){

		string base("append ");
		base = base + char_to_string(key);
		base = base + char_to_string(" 01 ") + to_string(expr);
		base = base + char_to_string(" ") + to_string(strlen(val));
		base = base + char_to_string("\r\n") + char_to_string(val);
		base = base + char_to_string("\r\n");

		add_kv_cmd(base);
	}

	void prepend_key(const char* key, const char* val, int expr = 0){

		string base("prepend ");
		base = base + char_to_string(key);
		base = base + char_to_string(" 01 ") + to_string(expr);
		base = base + char_to_string(" ") + to_string(strlen(val));
		base = base + char_to_string("\r\n") + char_to_string(val);
		base = base + char_to_string("\r\n");

		add_kv_cmd(base);
	}

	void get_mem_stats_and_assert(const char* type, const char* param, string val){

		string base("stats ");

		if(strcmp(type, "gen") != 0)
			base = base + char_to_string(type);

		base = base + char_to_string("\r\n");

		add_kv_cmd(base); // Add the command

		// In case of stats sizes_disable, just assert the final vala nd return
		if(strcmp(type, "sizes_disable") == 0){
			string resp(param);
			resp = resp + char_to_string(" ");
			resp = resp + val;
			set_expected_kv_resp(char_to_string("generic"), resp);
			return;
		}

		// Command to assert the result
		{

			string resp("stats ");
			resp = resp + char_to_string(type);

			string resp1("");
			resp1 = resp1 + char_to_string(param) + char_to_string(" ");

			// Don't put \r\n for stats items
			if(strcmp(type, "items") == 0)
				resp1 = resp1 + val;
			else
				resp1 = resp1 + val + char_to_string("\r\n");

			set_expected_kv_resp(resp, resp1);
		}
	}

	void get_and_assert_key(const char* key, const char* value){

		string base("get ");
		base = base + char_to_string(key);
		base = base + char_to_string("\r\n");

		add_kv_cmd(base);

		// Make sure the value have \r\n statements
		set_expected_kv_resp( char_to_string("get"), char_to_string(value) + char_to_string("\r\n"));
	}

	void get_key(const char* key){

		string base("get ");
		base = base + char_to_string(key);
		base = base + char_to_string("\r\n");

		add_kv_cmd(base);
	}

	string get_next_cmd(){

		restart:

		string retval;
		assert(kv_cmd != NULL && "Why will this ever happen?");

		if(kv_cmd->size()){

			vector<string>::iterator it = kv_cmd->begin();
			retval = *it;
			kv_cmd->erase(it);
		}else{

			__atomic_fetch_add(&num_conn_registered, 1, __ATOMIC_SEQ_CST);
			retval = string("quit\r\n");
		}

		// When ever a connection send a watch command, it is equivalent to dead
		if(retval == string("watch\n"))
			__atomic_fetch_add(&num_conn_registered, 1, __ATOMIC_SEQ_CST);

#ifdef EXECUTION_COYOTE_CONTROLLED
		if(retval == string("BlockAndSignal")){

#ifdef ENABLE_BLOCK_AND_SIGNAL

			if(block_and_signal_is_blocked == false){
				block_and_signal_is_blocked = true;
				blocked_thread_id = (long unsigned)pthread_self();

				FFI_create_resource(pthread_self());
				while(block_and_signal_is_blocked){
					FFI_wait_resource(pthread_self());
				}
				FFI_signal_resource(blocked_thread_id);
				FFI_delete_resource(blocked_thread_id);
				blocked_thread_id = 0;
			}
			else{

				block_and_signal_is_blocked = false;
				FFI_signal_resource(blocked_thread_id);
			}
#endif

			goto restart;
		}
#endif

		return retval;
	}

	void store_kv_response(char* str){

		string st(str);
		kv_response->push_back(st);
	}

	string get_kv_response(){

		assert(kv_response != NULL);
		return kv_response->back();
	}

	// Create a pair and store it in the vector!
	void set_expected_kv_resp(string type, string value){

		expected_response->push_back( new pair<string, string>(type, value) );
	}

	conn(){

		conn_id = socket_counter++;
		kv_response = new vector<string>();

		kv_cmd = new vector<string>();
		assert(kv_cmd != NULL);

		expected_response = new vector< pair<string, string>* >();
		output_counter = 0;
		input_counter = 0;

		if(map_fd_to_conn == NULL){
			map_fd_to_conn = new map<int, void*>();
		}

		bool rv = (map_fd_to_conn->insert({conn_id, this})).second;
		assert(rv && "Insertion to map_fd_to_conn failed!");
	}
	~conn(){

		delete kv_response;
		kv_response = 0;

		delete kv_cmd;
		kv_cmd = 0;

		delete expected_response;
		expected_response = 0;
	}
};

void shutdown_mc(){
	// Issue a SIGINT signal to the server
	std::raise(SIGINT);
}

extern "C"{

	ssize_t CT_socket_sendto(int socket, void* buffer, size_t length,
       int flags, struct sockaddr* address,
       socklen_t* address_len){

		assert(0 && "Not implemented");
		return -1;
	}
}
