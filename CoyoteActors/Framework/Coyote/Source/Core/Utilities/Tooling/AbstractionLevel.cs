// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

namespace Microsoft.Coyote.Utilities
{
    /// <summary>
    /// Determine the level of abstraction used during exploration.
    /// </summary>
    public enum AbstractionLevel
    {
        /// <summary>
        /// Basic Coyote hash: state, inbox and next-action.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Only hash the actor inboxes during exploration.
        /// </summary>
        InboxOnly,

        /// <summary>
        /// Use the customized state abstraction during exploration.
        /// </summary>
        Custom,

        /// <summary>
        /// Use the default Coyote hash and any additional custom hash provided.
        /// </summary>
        Full
    }
}
