// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using Microsoft.Coyote.Runtime;
using Microsoft.Coyote.TestingServices.Runtime;

namespace Microsoft.Coyote.TestingServices.Threading
{
    /// <summary>
    /// Abstract actor that can execute work asynchronously.
    /// </summary>
    internal abstract class WorkActor : AsyncActor
    {
        /// <summary>
        /// The id of the task that provides access to the completed work.
        /// </summary>
        internal abstract int AwaiterTaskId { get; }

        /// <summary>
        /// Id used to identify subsequent operations performed by this actor.
        /// </summary>
        protected internal override Guid OperationGroupId { get; set; } = Guid.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkActor"/> class.
        /// </summary>
        internal WorkActor(SystematicTestingRuntime runtime)
        {
            // var mid = new ActorId(this.GetType(), ActorTask.Name, runtime);
            var mid = new ActorId(this.GetType(), null, runtime);
            this.Initialize(runtime, mid);
        }

        /// <summary>
        /// Executes the work asynchronously.
        /// </summary>
        internal abstract Task ExecuteAsync();
    }
}
