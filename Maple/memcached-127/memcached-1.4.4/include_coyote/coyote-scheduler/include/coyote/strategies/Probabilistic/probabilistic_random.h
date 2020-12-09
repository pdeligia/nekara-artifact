// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#ifndef COYOTE_PROBABILISTIC_RANDOM_STRATEGY_H
#define COYOTE_PROBABILISTIC_RANDOM_STRATEGY_H

#include "../random.h"
#include "../strategy.h"
#include "../../operations/operations.h"

namespace coyote
{
	class ProbabilisticRandomStrategy : public Strategy
	{
	private:
		// The pseudo-random generator.
		Random generator;
		// The seed used by the current iteration.
		size_t iteration_seed;
		// ID of the currently scheduled operation
		size_t current_operation_id;
		// Number of schedule_next operations called
		long long unsigned step_counter;
		// Probability of returning the same operation
		unsigned int probability;
		// Maximum number of steps before the next probability change
		long long unsigned max_step_counter;
		// Should we have a fixed probability of returning the same operation?
		bool isProbabilityFixed;

	public:
		ProbabilisticRandomStrategy(size_t seed, bool, short unsigned int, long long unsigned) noexcept;
		ProbabilisticRandomStrategy(size_t seed) noexcept;

		ProbabilisticRandomStrategy(ProbabilisticRandomStrategy&& strategy) = delete;
		ProbabilisticRandomStrategy(ProbabilisticRandomStrategy const&) = delete;

		ProbabilisticRandomStrategy& operator=(ProbabilisticRandomStrategy&& strategy) = delete;
		ProbabilisticRandomStrategy& operator=(ProbabilisticRandomStrategy const&) = delete;

		// Returns the next operation.
		size_t next_operation(Operations& operations);

		// Returns the next boolean choice.
		bool next_boolean();

		// Returns the next integer choice.
		int next_integer(int max_value);

		// Returns the seed used in the current iteration.
		size_t seed();

		// Prepares the next iteration.
		void prepare_next_iteration();

		// Description about the strategy
		std::string get_description();

		// Fair strategy or not
		bool is_fair();
	};
}

#endif // COYOTE_PROBABILISTIC_RANDOM_STRATEGY_H
