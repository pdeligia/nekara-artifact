// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using Microsoft.CoyoteActors.Runtime;
using Microsoft.CoyoteActors.TestingServices.Runtime;

namespace Microsoft.CoyoteActors.SharedObjects
{
    /// <summary>
    /// Shared counter that can be safely shared by multiple Coyote actors.
    /// </summary>
    public static class SharedCounter
    {
        /// <summary>
        /// Creates a new shared counter.
        /// </summary>
        /// <param name="runtime">The actor runtime.</param>
        /// <param name="value">The initial value.</param>
        public static ISharedCounter Create(IActorRuntime runtime, int value = 0)
        {
            if (runtime is ProductionRuntime)
            {
                return new ProductionSharedCounter(value);
            }
            else if (runtime is SystematicTestingRuntime testingRuntime)
            {
                return new MockSharedCounter(value, testingRuntime);
            }
            else
            {
                throw new RuntimeException("Unknown runtime object of type: " + runtime.GetType().Name + ".");
            }
        }
    }
}
