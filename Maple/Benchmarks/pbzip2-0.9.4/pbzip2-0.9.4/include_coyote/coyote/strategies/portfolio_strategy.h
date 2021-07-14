 // Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#ifndef COYOTE_PORTFOLIO_STRATEGY_H
#define COYOTE_PORTFOLIO_STRATEGY_H

#include "strategy.h"
#include "combo_strategy.h"
#include "Exhaustive/dfs_strategy.h"
#include "Probabilistic/random_strategy.h"
#include "Probabilistic/pct_strategy.h"
#include "Probabilistic/probabilistic_random.h"

//#define DEBUG_PORTFOLIO_STRATEGY

#ifdef DEBUG_PORTFOLIO_STRATEGY
#include <iostream>
#endif

namespace coyote
{
	class PortfolioStrategy : public Strategy
	{
	private:
		Strategy* random;
		Strategy* probabilistic_random;
		Strategy* fair_pct;

		// Which strategy to use in this iteration
		Strategy* current_strategy;
		// This should be between 0 and 2 (inclusive)
		short unsigned int iteration_counter;

	public:

		PortfolioStrategy()
		{

#ifdef DEBUG_PORTFOLIO_STRATEGY
			std::cout<<"Portfolio initialized"<<std::endl;
#endif
			random = new RandomStrategy(std::chrono::high_resolution_clock::now().time_since_epoch().count());
			probabilistic_random = new ProbabilisticRandomStrategy(std::chrono::high_resolution_clock::now().time_since_epoch().count());
			fair_pct = new ComboStrategy("PCTStrategy", "RandomStrategy", 1000);

			current_strategy = random;
			iteration_counter = 0;
		}

		// Returns the next operation.
		size_t next_operation(Operations& operations)
		{
			return current_strategy->next_operation(operations);
		}

		// Returns the next boolean choice.
		bool next_boolean()
		{
			return current_strategy->next_boolean();
		}

		// Returns the next integer choice.
		int next_integer(int max_value)
		{
			return current_strategy->next_integer(max_value);
		}

		// Prepares the next iteration.
		void prepare_next_iteration()
		{
			iteration_counter = (iteration_counter + 1) % 3;

			fair_pct->prepare_next_iteration();
			random->prepare_next_iteration();
			probabilistic_random->prepare_next_iteration();

#ifdef DEBUG_PORTFOLIO_STRATEGY
			std::cout<<"Portfolio: iteration_counter: "<<iteration_counter<<std::endl;
#endif
			switch(iteration_counter)
			{
				case 0:
					current_strategy = random;
					break;
				case 1:
					current_strategy = fair_pct;
					break;
				case 2:
					current_strategy = probabilistic_random;
					break;
				default:
					throw "Unknown testing strategy";
			}
		}

		bool is_fair()
		{
			return current_strategy->is_fair();
		}

		size_t seed()
		{
			return current_strategy->seed();
		}

		// Description about the strategy
		std::string get_description()
		{
			return "Using Portfolio strategy with Random, fair-pct and probabilistic_random in a round-robin manner\n";
		}
	};
}

#endif /* COYOTE_PORTFOLIO_STRATEGY_H */
