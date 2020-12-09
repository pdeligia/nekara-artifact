﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#include "strategies/Probabilistic/probabilistic_random.h"

//#define DEBUG_PROBABILISTIC_RANDOM

namespace coyote
{
	ProbabilisticRandomStrategy::ProbabilisticRandomStrategy(size_t seed,
		bool _isProbabilityFixed = false, short unsigned int _probability = 5,
		long long unsigned _max_step_counter = 1000) noexcept :

		current_operation_id(0),
		step_counter(0),
		iteration_seed(seed),
		generator(seed),
		isProbabilityFixed(_isProbabilityFixed),
		max_step_counter(_max_step_counter)
	{

#ifdef DEBUG_PROBABILISTIC_RANDOM
		printf("ProbabilisticRandomStrategy: Initialized. With isProbabilityFixed as: %d \n", isProbabilityFixed);
#endif
		// If probability is fixed, use the probability given by the user
		if(isProbabilityFixed){
			// make sure that the input probability is between 1 and 10 (inclusive)
			probability =  (_probability % 11);
		}
		// Else, use the probability generated by the unique iteration seed. We did this to
		// ensure reproducibility of an iteration.
		else {
			probability = (generator.next() % 11);
			step_counter = (generator.next() % max_step_counter);
		}
	}

	ProbabilisticRandomStrategy::ProbabilisticRandomStrategy(size_t seed) noexcept :
		current_operation_id(0),
		step_counter(0),
		iteration_seed(seed),
		generator(seed),
		isProbabilityFixed(false),
		max_step_counter(1000)
	{

		probability = (generator.next() % 11);
		step_counter = (generator.next() % max_step_counter);
	}

	size_t ProbabilisticRandomStrategy::next_operation(Operations& operations)
	{

#ifdef DEBUG_PROBABILISTIC_RANDOM
		printf("ProbabilisticRandomStrategy: in next_operation. Probability is: %u \n", probability);
#endif

		// If the probability is fixed, don't change it. Otherwise, increment the probability by
		// 0.1 after every 'max_step_counter' steps.
		if(!isProbabilityFixed){

			step_counter ++;
			// Change the probability of returning the same operation
			if(step_counter >= max_step_counter){
				step_counter = 0;

#ifdef DEBUG_PROBABILISTIC_RANDOM
		printf("ProbabilisticRandomStrategy: Probability changed \n");
#endif
				// make sure the probability is always between 0 to 1 (inclusive)
				probability = (probability + 1) % 11;
			}
		}

		const size_t randn = generator.next();
		size_t index = 0;

		// We should schedule the same operation
		if((randn % 10) < probability || operations.size() == 1){

			// First check whether this operation is blocked or not
			bool is_current_operation_blocked = true;
			for(int i = 0; i < operations.size(true); i++){
				if(current_operation_id == operations[i]){
					is_current_operation_blocked = false;
					break;
				}
			}

			// If current operation is not blocked then we can schedule it
			if(!is_current_operation_blocked)
				return current_operation_id;

			// Otherwise schedule any other active operation
			current_operation_id = operations[ randn % operations.size() ];
			return current_operation_id;
		}

		// schedule some other, randomly selected, operation
		else {
			index = randn % operations.size();

			// If it is same as the current scheduled operation
			if(operations[index] == current_operation_id)
				index = (index + 1) % operations.size();

			current_operation_id = operations[index];
			return current_operation_id;
		}
	}

	bool ProbabilisticRandomStrategy::next_boolean()
	{
		return (generator.next() & 1) == 0;
	}

	int ProbabilisticRandomStrategy::next_integer(int max_value)
	{
		return generator.next() % max_value;
	}

	size_t ProbabilisticRandomStrategy::seed()
	{
		return iteration_seed;
	}

	void ProbabilisticRandomStrategy::prepare_next_iteration()
	{
		iteration_seed += 1;
		generator.seed(iteration_seed);
		current_operation_id = 0;
		step_counter = 0;

		// If probability is not fixed, chose any initial probability between 0 and 1.
		if(!isProbabilityFixed){
			probability = (generator.next() % 11);
			step_counter = (generator.next() % max_step_counter);
		}
	}

	bool ProbabilisticRandomStrategy::is_fair()
	{
		return true;
	}

	std::string ProbabilisticRandomStrategy::get_description()
	{
		return "Probabilistic  Strategy.";
	}
}
