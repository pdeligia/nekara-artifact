#!/bin/bash

N=100
echo "*** This script will build and test streamcluster, instrumented with Nekara APIs. ***"
echo "*** We will run the test case $N times and will report the average number of iterations in which bug could be triggered. ***"
echo "*** It will take around 10 minutes for this script to complete. Please press [Enter] to continue... ***"
read ignore

# Cleanup
sh Clean.sh
rm -r ./TestResults

# Build coyote-scheduler
cd include_coyote/coyote-scheduler && mkdir build && cd ./build && cmake -G Ninja .. && ninja -j3 && cp ./src/libcoyote.so ../../ && cd ../../

# Build coyote's FFI wrapper
g++ -std=c++11 -shared -fPIC -I./ -g -o libcoyote_c_ffi.so coyote_c_ffi.cpp -lcoyote -L./
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$PWD
cd ../

# Build streamcluster
make 2> /dev/null

# Test
# mkdir ./TestResults/

sum=0
# Run test for 100 iterations
for i in $(seq 1 $N);
	do
		# echo "\n\n **** Running test iteration # $i ****\n\n"
		rm nohup.out
		nohup ./streamcluster 2 5 1 10 10 5 none output.txt 2 
		sum=$((sum + $(cat nohup.out | grep -c "iteration")))
		# cat nohup.out | tail -n 1
done

avg=$(echo $sum / $N | bc -l | awk '{printf("%d\n",$1 + 0.5)}')

echo "\n\n\e[0;33m Average number of iterations in which bug could be triggered: $avg \e[0m"
