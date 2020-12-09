 // Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#ifndef COYOTE_TESTING_STRATEGY_H
#define COYOTE_TESTING_STRATEGY_H

#include "strategy.h"
#include "combo_strategy.h"
#include "portfolio_strategy.h"
#include "Exhaustive/dfs_strategy.h"
#include "Probabilistic/random_strategy.h"
#include "Probabilistic/pct_strategy.h"
#include "Probabilistic/probabilistic_random.h"

namespace coyote
{
	class TestingStrategy
	{
	private:
		Strategy* strategy;

	public:
		// Random Strategy
		TestingStrategy(size_t seed)
		{
			strategy = new RandomStrategy(seed);
		}

		TestingStrategy(std::string strat)
		{
			if (strat.compare("DFSStrategy") == 0)
			{
				strategy = new DFSStrategy();
			}
			else if (strat.compare("PCTStrategy") == 0)
			{
				strategy = new PCTStrategy();
			}
			else if (strat.compare("RandomStrategy") == 0)
			{
				strategy = new RandomStrategy(std::chrono::high_resolution_clock::now().time_since_epoch().count());
			}
			else if (strat.compare("ProbabilisticRandomStrategy") == 0)
			{
				strategy = new ProbabilisticRandomStrategy(std::chrono::high_resolution_clock::now().time_since_epoch().count());
			}
			else if (strat.compare("PortfolioStrategy") == 0)
			{
				strategy = new PortfolioStrategy();
			}
			else
			{
				throw "Wrong or unavailable selection of testing strategy.";
			}
		}

		TestingStrategy(std::string str, long long unsigned prefixLen)
		{
			if(str.compare("FairPCTStrategy") == 0)
			{
				strategy = new ComboStrategy("PCTStrategy", "RandomStrategy", prefixLen);
			}
			else
			{
				throw "Wrong or unavailable selection of testing strategy.";
			}
		}

		// Returns the next operation.
		size_t next_operation(Operations& operations)
		{
			return strategy->next_operation(operations);
		}

		// Returns the next boolean choice.
		bool next_boolean()
		{
			return strategy->next_boolean();
		}

		// Returns the next integer choice.
		int next_integer(int max_value)
		{
			return strategy->next_integer(max_value);
		}

		// Prepares the next iteration.
		void prepare_next_iteration()
		{
			return strategy->prepare_next_iteration();
		}

		// Fair strategy or not
		bool is_fair()
		{
			return strategy->is_fair();
		}

		// Description about the strategy
		std::string get_description()
		{
			return strategy->get_description();
		}

		size_t seed(){
			return strategy->seed();
		}
	};
}

#endif
