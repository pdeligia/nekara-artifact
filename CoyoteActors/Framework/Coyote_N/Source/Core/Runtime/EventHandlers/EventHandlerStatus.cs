// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

namespace Microsoft.Coyote.Runtime
{
    /// <summary>
    /// The status of the actor event handler.
    /// </summary>
    internal enum EventHandlerStatus
    {
        /// <summary>
        /// The actor has dequeued an event.
        /// </summary>
        EventDequeued = 0,

        /// <summary>
        /// The actor has handled an event.
        /// </summary>
        EventHandled,

        /// <summary>
        /// The actor has dequeued an event that cannot be handled.
        /// </summary>
        EventUnhandled
    }
}
