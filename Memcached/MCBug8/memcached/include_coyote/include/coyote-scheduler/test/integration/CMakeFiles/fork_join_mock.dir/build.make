# CMAKE generated file: DO NOT EDIT!
# Generated by "Unix Makefiles" Generator, CMake Version 3.10

# Delete rule output on recipe failure.
.DELETE_ON_ERROR:


#=============================================================================
# Special targets provided by cmake.

# Disable implicit rules so canonical targets will work.
.SUFFIXES:


# Remove some rules from gmake that .SUFFIXES does not remove.
SUFFIXES =

.SUFFIXES: .hpux_make_needs_suffix_list


# Suppress display of executed commands.
$(VERBOSE).SILENT:


# A target that is always out of date.
cmake_force:

.PHONY : cmake_force

#=============================================================================
# Set environment variables for the build.

# The shell in which to execute make rules.
SHELL = /bin/sh

# The CMake executable.
CMAKE_COMMAND = /usr/bin/cmake

# The command to remove a file.
RM = /usr/bin/cmake -E remove -f

# Escaping for special characters.
EQUALS = =

# The top-level source directory on which CMake was run.
CMAKE_SOURCE_DIR = /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration

# The top-level build directory on which CMake was run.
CMAKE_BINARY_DIR = /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration

# Include any dependencies generated for this target.
include CMakeFiles/fork_join_mock.dir/depend.make

# Include the progress variables for this target.
include CMakeFiles/fork_join_mock.dir/progress.make

# Include the compile flags for this target's objects.
include CMakeFiles/fork_join_mock.dir/flags.make

CMakeFiles/fork_join_mock.dir/fork_join_mock.o: CMakeFiles/fork_join_mock.dir/flags.make
CMakeFiles/fork_join_mock.dir/fork_join_mock.o: fork_join_mock.cc
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green --progress-dir=/home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration/CMakeFiles --progress-num=$(CMAKE_PROGRESS_1) "Building CXX object CMakeFiles/fork_join_mock.dir/fork_join_mock.o"
	/usr/bin/c++  $(CXX_DEFINES) $(CXX_INCLUDES) $(CXX_FLAGS) -o CMakeFiles/fork_join_mock.dir/fork_join_mock.o -c /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration/fork_join_mock.cc

CMakeFiles/fork_join_mock.dir/fork_join_mock.i: cmake_force
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green "Preprocessing CXX source to CMakeFiles/fork_join_mock.dir/fork_join_mock.i"
	/usr/bin/c++ $(CXX_DEFINES) $(CXX_INCLUDES) $(CXX_FLAGS) -E /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration/fork_join_mock.cc > CMakeFiles/fork_join_mock.dir/fork_join_mock.i

CMakeFiles/fork_join_mock.dir/fork_join_mock.s: cmake_force
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green "Compiling CXX source to assembly CMakeFiles/fork_join_mock.dir/fork_join_mock.s"
	/usr/bin/c++ $(CXX_DEFINES) $(CXX_INCLUDES) $(CXX_FLAGS) -S /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration/fork_join_mock.cc -o CMakeFiles/fork_join_mock.dir/fork_join_mock.s

CMakeFiles/fork_join_mock.dir/fork_join_mock.o.requires:

.PHONY : CMakeFiles/fork_join_mock.dir/fork_join_mock.o.requires

CMakeFiles/fork_join_mock.dir/fork_join_mock.o.provides: CMakeFiles/fork_join_mock.dir/fork_join_mock.o.requires
	$(MAKE) -f CMakeFiles/fork_join_mock.dir/build.make CMakeFiles/fork_join_mock.dir/fork_join_mock.o.provides.build
.PHONY : CMakeFiles/fork_join_mock.dir/fork_join_mock.o.provides

CMakeFiles/fork_join_mock.dir/fork_join_mock.o.provides.build: CMakeFiles/fork_join_mock.dir/fork_join_mock.o


# Object files for target fork_join_mock
fork_join_mock_OBJECTS = \
"CMakeFiles/fork_join_mock.dir/fork_join_mock.o"

# External object files for target fork_join_mock
fork_join_mock_EXTERNAL_OBJECTS =

fork_join_mock: CMakeFiles/fork_join_mock.dir/fork_join_mock.o
fork_join_mock: CMakeFiles/fork_join_mock.dir/build.make
fork_join_mock: CMakeFiles/fork_join_mock.dir/link.txt
	@$(CMAKE_COMMAND) -E cmake_echo_color --switch=$(COLOR) --green --bold --progress-dir=/home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration/CMakeFiles --progress-num=$(CMAKE_PROGRESS_2) "Linking CXX executable fork_join_mock"
	$(CMAKE_COMMAND) -E cmake_link_script CMakeFiles/fork_join_mock.dir/link.txt --verbose=$(VERBOSE)

# Rule to build all files generated by this target.
CMakeFiles/fork_join_mock.dir/build: fork_join_mock

.PHONY : CMakeFiles/fork_join_mock.dir/build

CMakeFiles/fork_join_mock.dir/requires: CMakeFiles/fork_join_mock.dir/fork_join_mock.o.requires

.PHONY : CMakeFiles/fork_join_mock.dir/requires

CMakeFiles/fork_join_mock.dir/clean:
	$(CMAKE_COMMAND) -P CMakeFiles/fork_join_mock.dir/cmake_clean.cmake
.PHONY : CMakeFiles/fork_join_mock.dir/clean

CMakeFiles/fork_join_mock.dir/depend:
	cd /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration && $(CMAKE_COMMAND) -E cmake_depends "Unix Makefiles" /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration /home/udit/coyote-scheduler-pct-ffi/coyote-scheduler/test/integration/CMakeFiles/fork_join_mock.dir/DependInfo.cmake --color=$(COLOR)
.PHONY : CMakeFiles/fork_join_mock.dir/depend

