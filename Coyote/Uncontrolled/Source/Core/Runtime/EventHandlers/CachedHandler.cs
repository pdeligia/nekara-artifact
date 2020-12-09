﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Coyote.Threading;

namespace Microsoft.Coyote.Runtime
{
    /// <summary>
    /// A machine handler that has been cached for performance optimization.
    /// </summary>
    internal class CachedHandler
    {
        internal readonly MethodInfo MethodInfo;
        internal readonly Delegate Handler;

        internal CachedHandler(MethodInfo methodInfo, Machine machine)
        {
            this.MethodInfo = methodInfo;

            // MethodInfo.Invoke catches the exception to wrap it in a TargetInvocationException.
            // This unwinds the stack before Machine.ExecuteAction's exception filter is invoked,
            // so call through the delegate instead (which is also much faster than Invoke).
            if (methodInfo.ReturnType == typeof(void))
            {
                this.Handler = Delegate.CreateDelegate(typeof(Action), machine, methodInfo);
            }
            else if (methodInfo.ReturnType == typeof(MachineTask))
            {
                this.Handler = Delegate.CreateDelegate(typeof(Func<MachineTask>), machine, methodInfo);
            }
            else
            {
                this.Handler = Delegate.CreateDelegate(typeof(Func<Task>), machine, methodInfo);
            }
        }
    }
}
