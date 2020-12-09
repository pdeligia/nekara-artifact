#include <stdio.h>
#include <coyote_c_ffi.h>

void* malloc(size_t x){
	if(FFI_next_boolean()){
		return malloc(x);
	} else {
		return NULL;
	}
}

void* calloc(size_t x, size_t y){
	if(FFI_next_boolean()){
		return calloc(x, y);
	} else {
		return NULL;
	}
}

void* realloc(void* t, size_t x){
	if(FFI_next_boolean()){
		return realloc(t, x);
	} else {
		return NULL;
	}
}
