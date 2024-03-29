﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.CoyoteActors
{
    /// <summary>
    /// Attribute for declaring that a state of an actor
    /// is the start one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class StartAttribute : Attribute
    {
    }
}
