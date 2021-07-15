#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>

int main(int argc, char** argv){

	char *p =  NULL;
	snprintf(p, 23, "some_long_string_here\n");
	return 0;
}
