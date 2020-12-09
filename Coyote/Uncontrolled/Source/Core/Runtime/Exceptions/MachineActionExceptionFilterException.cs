// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Coyote.Runtime
{
    /// <summary>
    /// The exception that is thrown by the Coyote runtime upon an actor action failure.
    /// </summary>
    internal sealed class ActorActionExceptionFilterException : RuntimeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActorActionExceptionFilterException"/> class.
        /// </summary>
        /// <param name="message">Message</param>
        internal ActorActionExceptionFilterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorActionExceptionFilterException"/> class.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner exception</param>
        internal ActorActionExceptionFilterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
