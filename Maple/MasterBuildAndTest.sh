#!/bin/bash

if [ "$#" -ne 1 ]; then
    echo "Illegal number of parameters. Enter the number of iterations to run."
    exit
fi

echo "*** This Master script will build and test all Maple Benchmarks instrumented with Nekara APIs. ***"

cd memcached-127/memcached-1.4.4/
sh BuildAndTest.sh $1

cd ../..
cd pbzip2-0.9.4/pbzip2-0.9.4/
sh BuildAndTest.sh $1

cd ../..
cd streamcluster/src/
sh BuildAndTest.sh $1


echo "*** This Master script for all Maple Benchmarks is completed***"
