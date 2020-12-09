// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace Microsoft.CoyoteActors.Threading
{
    /// <summary>
    /// Extension methods for <see cref="Task"/> and <see cref="Task{TResult}"/> objects.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Converts the specified <see cref="Task"/> into a <see cref="ActorTask"/>.
        /// </summary>
        public static ActorTask ToActorTask(this Task @this) => new ActorTask(@this);

        /// <summary>
        /// Converts the specified <see cref="Task{TResult}"/> into a <see cref="ActorTask{TResult}"/>.
        /// </summary>
        public static ActorTask<TResult> ToActorTask<TResult>(this Task<TResult> @this) =>
            new ActorTask<TResult>(@this);
    }
}
