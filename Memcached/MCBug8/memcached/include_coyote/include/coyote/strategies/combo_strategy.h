 // Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#ifndef COYOTE_COMBO_STRATEGY_H
#define COYOTE_COMBO_STRATEGY_H

#include "strategy.h"
#include "Exhaustive/dfs_strategy.h"
#include "Probabilistic/random_strategy.h"
#include "Probabilistic/pct_strategy.h"
#include "Probabilistic/probabilistic_random.h"

//#define DEBUG_COMBO_STRATEGY

#ifdef DEBUG_COMBO_STRATEGY
#include <iostream>
#endif

namespace coyote
{
	class ComboStrategy : public Strategy
	{
	private:
		Strategy* PrefixStrategy;
		Strategy* SuffixStrategy;
		long long unsigned prefixPathLength;
		// Description of prefix and suffix
		std::string prefixDesc;
		std::string suffixDesc;
		long long unsigned stepsCounter;

	public:

		ComboStrategy(std::string prefix, std::string suffix, long long unsigned prefixLen):
					prefixPathLength(prefixLen),
					prefixDesc(prefix),
					suffixDesc(suffix),
					stepsCounter(0)
		{

#ifdef DEBUG_COMBO_STRATEGY
			std::cout<<"ComboStrategy initialized with prefix as:"<<prefixDesc
			<<" ; suffix as: "<<suffixDesc<<"; prefixPathLength as: "<<prefixPathLength<<std::endl;
#endif
			// For prefix
			if (prefix.compare("DFSStrategy") == 0)
			{
				PrefixStrategy = new DFSStrategy();
			}
			else if (prefix.compare("PCTStrategy") == 0)
			{
				PrefixStrategy = new PCTStrategy();
			}
			else if (prefix.compare("RandomStrategy") == 0)
			{
				PrefixStrategy = new RandomStrategy(std::chrono::high_resolution_clock::now().time_since_epoch().count());
			}
			else if (prefix.compare("ProbabilisticRandomStrategy") == 0)
			{
				PrefixStrategy = new ProbabilisticRandomStrategy(std::chrono::high_resolution_clock::now().time_since_epoch().count());
			}
			else
			{
				throw "Wrong or unavailable selection of testing strategy.";
			}

			// For suffix
			if (suffix.compare("DFSStrategy") == 0)
			{
				SuffixStrategy = new DFSStrategy();
			}
			else if (suffix.compare("PCTStrategy") == 0)
			{
				SuffixStrategy = new PCTStrategy();
			}
			else if (suffix.compare("RandomStrategy") == 0)
			{
				SuffixStrategy = new RandomStrategy(std::chrono::high_resolution_clock::now().time_since_epoch().count());
			}
			else if (suffix.compare("ProbabilisticRandomStrategy") == 0)
			{
				SuffixStrategy = new ProbabilisticRandomStrategy(std::chrono::high_resolution_clock::now().time_since_epoch().count());
			}
			else
			{
				throw "Wrong or unavailable selection of testing strategy.";
			}
		}

		// Returns the next operation.
		size_t next_operation(Operations& operations)
		{

			if(stepsCounter >= prefixPathLength){
#ifdef DEBUG_COMBO_STRATEGY
			std::cout<<"ComboStrategy: running suffix now \n";
#endif
				return SuffixStrategy->next_operation(operations);
			}
			else{
#ifdef DEBUG_COMBO_STRATEGY
			std::cout<<"ComboStrategy: running prefix now \n";
#endif
				stepsCounter++;
				return PrefixStrategy->next_operation(operations);
			}
		}

		// Returns the next boolean choice.
		bool next_boolean()
		{
			if(stepsCounter >= prefixPathLength){
				return SuffixStrategy->next_boolean();
			}
			else{
				return PrefixStrategy->next_boolean();
			}
		}

		// Returns the next integer choice.
		int next_integer(int max_value)
		{
			if(stepsCounter >= prefixPathLength){
				return SuffixStrategy->next_integer(max_value);
			}
			else{
				return PrefixStrategy->next_integer(max_value);
			}
		}

		// Prepares the next iteration.
		void prepare_next_iteration()
		{
			stepsCounter = 0;
			PrefixStrategy->prepare_next_iteration();
			SuffixStrategy->prepare_next_iteration();
		}

		// Fair strategy or not
		bool is_fair()
		{
			if(stepsCounter >= prefixPathLength){
				return SuffixStrategy->is_fair();
			}
			else{
				return PrefixStrategy->is_fair();
			}
		}

		// Description about the strategy
		std::string get_description()
		{
			return "Using ComboStrategy with prefix as: " + prefixDesc + "And suffix as: "+
					suffixDesc + " prefix path length is: " + std::to_string(prefixPathLength) + "\n";
		}

		// seed
		size_t seed(){

			if(stepsCounter >= prefixPathLength){
				return SuffixStrategy->seed();
			}
			else{
				return PrefixStrategy->seed();
			}
		}
	};
}

#endif /* COYOTE_COMBO_STRATEGY_H */
