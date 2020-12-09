// Compile this as: g++ -std=c++11 -shared -fPIC -I./ -g -o libcoyotest.so test_basic_store_get.cpp -g -L/home/udit/memcached_2020/memcached/include_coyote/include -lcoyote_c_ffi -lcoyote
#include <test_template.h>

using namespace std;

extern "C"{

bool CT_is_socket(int fd){

	// Check if this is in the map of allocated connections
	if(map_fd_to_conn->find(fd) != map_fd_to_conn->end()){

		return true;
	}
	else {

		return false;
	}
}

ssize_t CT_socket_write(int fd, void* buff, int count){

	assert(CT_is_socket(fd) && "This is not the socket we have opened!");

	map<int, void*>::iterator it = map_fd_to_conn->find(fd);
	conn* obj = (conn*)(it->second);

	string st = obj->get_next_cmd();
	char msg[1024];

	// Convert string object to C strings and copy it info the buffer
	strcpy(msg, st.c_str());
	memcpy(buff, msg, strlen(msg));

	return strlen(msg);
}

ssize_t CT_socket_recvmsg(int fd, struct msghdr *msg, int flags){

	assert(CT_is_socket(fd));

	// Get the conn object corresponding to this fd
	map<int, void*>::iterator it = map_fd_to_conn->find(fd);
	conn* obj = (conn*)(it->second);

	// Store the incomming response in the vector
	obj->store_kv_response((char*)(msg->msg_iov->iov_base));

	printf("Recieved on connection number %d, msg: %s", fd, (char*)(msg->msg_iov->iov_base));

	return strlen((char*)(msg->msg_iov->iov_base));
}

int count_num_sockets = 2;

int CT_new_socket(){

	if(global_conns == NULL){
		global_conns = new vector<conn*>();
	}


	if(count_num_sockets == 0){

		while(1){
			FFI_schedule_next();
		}
	}
	count_num_sockets--;

	conn* new_con = new conn();

	new_con->add_kv_cmd("set k1 01 0 1\r\n");
	new_con->add_kv_cmd("4\r\n");
	new_con->set_expected_kv_resp("STORED\r\n");

	//..
	// Wrapper for get and set
	new_con->add_kv_cmd("get k1\r\n");

	global_conns->push_back(new_con);

	return new_con->conn_id;
}

// Test main method
int CT_main( int (*run_iteration)(int, char**), int argc, char** argv ){

	FFI_create_scheduler();

	int num_iter = 10;

	// Lights, Camera, Action!
	for(int j = 0; j < num_iter; j++){

		printf("Starting iteration #%d \n", j);

		FFI_attach_scheduler();

		run_iteration(argc, argv);

		FFI_detach_scheduler();
		FFI_scheduler_assert();

	}

	FFI_delete_scheduler();

	return 0;
}

} /* extern "C" */