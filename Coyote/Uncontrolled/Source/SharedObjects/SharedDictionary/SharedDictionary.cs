﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Microsoft.Coyote.Runtime;
using Microsoft.Coyote.TestingServices.Runtime;

namespace Microsoft.Coyote.SharedObjects
{
    /// <summary>
    /// Shared dictionary that can be safely shared by multiple Coyote actors.
    /// </summary>
    public static class SharedDictionary
    {
        /// <summary>
        /// Creates a new shared dictionary.
        /// </summary>
        /// <param name="runtime">The actor runtime.</param>
        public static ISharedDictionary<TKey, TValue> Create<TKey, TValue>(IActorRuntime runtime)
        {
            if (runtime is ProductionRuntime)
            {
                return new ProductionSharedDictionary<TKey, TValue>();
            }
            else if (runtime is SystematicTestingRuntime testingRuntime)
            {
                return new MockSharedDictionary<TKey, TValue>(null, testingRuntime);
            }
            else
            {
                throw new RuntimeException("Unknown runtime object of type: " + runtime.GetType().Name + ".");
            }
        }

        /// <summary>
        /// Creates a new shared dictionary.
        /// </summary>
        /// <param name="comparer">The key comparer.</param>
        /// <param name="runtime">The actor runtime.</param>
        public static ISharedDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> comparer, IActorRuntime runtime)
        {
            if (runtime is ProductionRuntime)
            {
                return new ProductionSharedDictionary<TKey, TValue>(comparer);
            }
            else if (runtime is SystematicTestingRuntime testingRuntime)
            {
                return new MockSharedDictionary<TKey, TValue>(comparer, testingRuntime);
            }
            else
            {
                throw new RuntimeException("Unknown runtime object of type: " + runtime.GetType().Name + ".");
            }
        }
    }
}
