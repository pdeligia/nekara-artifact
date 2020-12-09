﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

namespace Microsoft.Coyote.Runtime
{
    /// <summary>
    /// Handles the <see cref="IActorRuntime.OnEventDropped"/> event.
    /// </summary>
    public delegate void OnEventDroppedHandler(Event e, ActorId target);
}
