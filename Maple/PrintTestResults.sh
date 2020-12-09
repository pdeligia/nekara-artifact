#!/bin/bash

echo "*** This Master script will print the results of all Maple Benchmarks ***"

echo "For memcached:\n"
cat ./memcached-127/memcached-1.4.4/TestResults/result.txt

echo "\nFor PbZip2\n"
cat ./pbzip2-0.9.4/pbzip2-0.9.4/TestResults/result.txt

echo "\nFor StreamCluster\n"
cat ./streamcluster/src/TestResults/result.txt
