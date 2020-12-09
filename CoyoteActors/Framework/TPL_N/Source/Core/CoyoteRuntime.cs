// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using Microsoft.CoyoteActors.Runtime;

namespace Microsoft.CoyoteActors
{
    /// <summary>
    /// The runtime for creating and executing actors.
    /// </summary>
    public static class CoyoteRuntime
    {
        /// <summary>
        /// Creates a new runtime.
        /// </summary>
        /// <returns>The created runtime.</returns>
        public static IActorRuntime Create()
        {
            return new ProductionRuntime(Configuration.Create());
        }

        /// <summary>
        /// Creates a new runtime with the specified <see cref="Configuration"/>.
        /// </summary>
        /// <param name="configuration">The runtime configuration to use.</param>
        /// <returns>The created runtime.</returns>
        public static IActorRuntime Create(Configuration configuration)
        {
            return new ProductionRuntime(configuration ?? Configuration.Create());
        }
    }
}
