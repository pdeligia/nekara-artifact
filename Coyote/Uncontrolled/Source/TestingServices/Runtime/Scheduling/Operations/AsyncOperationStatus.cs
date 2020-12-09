﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

namespace Microsoft.Coyote.TestingServices.Scheduling
{
    /// <summary>
    /// The status of an asynchronous operation.
    /// </summary>
    public enum AsyncOperationStatus
    {
        /// <summary>
        /// The operation does not have a status yet.
        /// </summary>
        None = 0,

        /// <summary>
        /// The operation is enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// The operation is waiting for all of its dependencies to complete.
        /// </summary>
        BlockedOnWaitAll,

        /// <summary>
        /// The operation is waiting for any of its dependencies to complete.
        /// </summary>
        BlockedOnWaitAny,

        /// <summary>
        /// The operation is waiting to receive an event.
        /// </summary>
        BlockedOnReceive,

        /// <summary>
        /// The operation is waiting to acquire a resource.
        /// </summary>
        BlockedOnResource,

        /// <summary>
        /// The operation is completed.
        /// </summary>
        Completed
    }
}
