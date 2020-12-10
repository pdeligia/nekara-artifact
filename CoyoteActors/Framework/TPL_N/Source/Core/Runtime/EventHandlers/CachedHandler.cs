// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.CoyoteActors.Runtime
{
    /// <summary>
    /// A actor handler that has been cached for performance optimization.
    /// </summary>
    internal class CachedHandler
    {
        internal readonly MethodInfo MethodInfo;
        internal readonly Delegate Handler;

        internal CachedHandler(MethodInfo methodInfo, Actor actor)
        {
            this.MethodInfo = methodInfo;

            // MethodInfo.Invoke catches the exception to wrap it in a TargetInvocationException.
            // This unwinds the stack before Actor.ExecuteAction's exception filter is invoked,
            // so call through the delegate instead (which is also much faster than Invoke).
            if (methodInfo.ReturnType == typeof(void))
            {
                this.Handler = Delegate.CreateDelegate(typeof(Action), actor, methodInfo);
            }
            else if (methodInfo.ReturnType == typeof(Task))
            {
                this.Handler = Delegate.CreateDelegate(typeof(Func<Task>), actor, methodInfo);
            }
        }
    }
}
