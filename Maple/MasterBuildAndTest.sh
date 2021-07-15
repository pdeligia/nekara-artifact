#!/bin/bash

echo "*** This Master script will build and test all Maple Benchmarks instrumented with Nekara APIs. ***"


cd memcached-127/memcached-1.4.4/
sh BuildAndTest.sh


cd ../..
cd pbzip2-0.9.4/pbzip2-0.9.4/
sh BuildAndTest.sh

cd ../..
cd streamcluster/src/
sh BuildAndTest.sh


echo "*** This Master script for all Maple Benchmarks is completed***"
