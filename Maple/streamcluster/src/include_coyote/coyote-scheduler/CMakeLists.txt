﻿cmake_minimum_required (VERSION 3.8)
project ("coyote" CXX)

set(CMAKE_CXX_STANDARD 17)

enable_testing()

if(CMAKE_TESTING_ENABLED AND NOT MSVC)
    find_package(Threads REQUIRED)
endif()

add_compile_options(-g -ggdb)

if(MSVC)
    add_compile_options(/permissive-)
    add_compile_options(/utf-8)
endif()

add_subdirectory(src)
if(CMAKE_TESTING_ENABLED)
    add_subdirectory(test)
endif()
