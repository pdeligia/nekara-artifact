﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

namespace Microsoft.Coyote.Runtime
{
    /// <summary>
    /// The outcome when an actor throws an exception.
    /// </summary>
    public enum OnExceptionOutcome
    {
        /// <summary>
        /// Throw the exception causing the runtime to fail.
        /// </summary>
        ThrowException = 0,

        /// <summary>
        /// The exception was handled and Actor should continue execution.
        /// </summary>
        HandledException = 1,

        /// <summary>
        /// Halt the actor (do not throw the exception).
        /// </summary>
        HaltActor = 2
    }
}
