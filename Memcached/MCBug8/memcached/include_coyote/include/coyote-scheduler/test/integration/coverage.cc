// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#include <set>
#include <thread>
#include "test.h"

using namespace coyote;

constexpr auto WORK_THREAD_1_ID = 1;
constexpr auto WORK_THREAD_2_ID = 2;

Scheduler* scheduler;

std::string curr_trace;
std::set<std::string> coverage;

void work_1()
{
	scheduler->start_operation(WORK_THREAD_1_ID);

	for(int i = 0; i < 1500; i++){
		curr_trace += "1";
		scheduler->schedule_next();
		curr_trace += "2";
	}

	scheduler->complete_operation(WORK_THREAD_1_ID);
}

void work_2()
{
	scheduler->start_operation(WORK_THREAD_2_ID);

	for(int i = 0; i < 1500; i++){
		curr_trace += "3";
		scheduler->schedule_next();
		curr_trace += "4";
	}

	scheduler->complete_operation(WORK_THREAD_2_ID);
}

void run_iteration()
{
	scheduler->attach();

	scheduler->create_operation(WORK_THREAD_1_ID);
	std::thread t1(work_1);

	scheduler->create_operation(WORK_THREAD_2_ID);
	std::thread t2(work_2);

	scheduler->schedule_next();

	if (curr_trace.size() == 4)
	{
		coverage.insert(curr_trace);
	}

	scheduler->join_operation(WORK_THREAD_1_ID);
	scheduler->join_operation(WORK_THREAD_2_ID);

	t1.join();
	t2.join();

	scheduler->detach();
	assert(scheduler->error_code(), ErrorCode::Success);
}

int main()
{
	std::cout << "[test] started." << std::endl;
	auto start_time = std::chrono::steady_clock::now();

	try
	{
		scheduler = new Scheduler("PortfolioStrategy");

		for (int i = 0; i < 100; i++)
		{
#ifdef COYOTE_DEBUG_LOG
			std::cout << "[test] iteration " << i << std::endl;
#endif // COYOTE_DEBUG_LOG
			curr_trace = "";
			run_iteration();
		}

		delete scheduler;
	}
	catch (std::string error)
	{
		std::cout << "[test] failed: " << error << std::endl;
		return 1;
	}

	std::cout << "[test] done in " << total_time(start_time) << "ms." << std::endl;
	return 0;
}
