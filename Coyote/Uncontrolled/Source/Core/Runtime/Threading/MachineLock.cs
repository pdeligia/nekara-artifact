﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Coyote.Runtime;

namespace Microsoft.Coyote.Threading
{
    /// <summary>
    /// A mutual exclusion lock that can be acquired asynchronously
    /// by a <see cref="Machine"/> or <see cref="MachineTask"/>.
    /// </summary>
    public class MachineLock
    {
        /// <summary>
        /// Unique id of the lock.
        /// </summary>
        public readonly ulong Id;

        /// <summary>
        /// Queue of tasks awaiting to acquire the lock.
        /// </summary>
        private readonly Queue<TaskCompletionSource<object>> Awaiters;

        /// <summary>
        /// True if the lock has been acquired, else false.
        /// </summary>
        protected internal bool IsAcquired;

        /// <summary>
        /// Initializes a new instance of the <see cref="MachineLock"/> class.
        /// </summary>
        internal MachineLock(ulong id)
        {
            this.Id = id;
            this.Awaiters = new Queue<TaskCompletionSource<object>>();
            this.IsAcquired = false;
        }

        /// <summary>
        /// Creates a new mutual exclusion lock.
        /// </summary>
        /// <returns>The mutual exclusion lock.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MachineLock Create() => MachineRuntime.CurrentScheduler.CreateLock();

        /// <summary>
        /// Tries to acquire the lock asynchronously, and returns a task that completes
        /// when the lock has been acquired. The returned task contains a releaser that
        /// releases the lock when disposed.
        /// </summary>
        public virtual MachineTask<Releaser> AcquireAsync()
        {
            lock (this.Awaiters)
            {
                if (!this.IsAcquired)
                {
                    this.IsAcquired = true;
                    return MachineTask.FromResult(new Releaser(this));
                }
                else
                {
                    var waiter = new TaskCompletionSource<object>();
                    this.Awaiters.Enqueue(waiter);
                    return waiter.Task.ContinueWith((_, state) => new Releaser((MachineLock)state), this,
                        CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).
                        ToMachineTask();
                }
            }
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        protected virtual void Release()
        {
            TaskCompletionSource<object> awaiter = null;
            lock (this.Awaiters)
            {
                if (this.Awaiters.Count > 0)
                {
                    awaiter = this.Awaiters.Dequeue();
                }
                else
                {
                    this.IsAcquired = false;
                }
            }

            if (awaiter != null)
            {
                awaiter.SetResult(null);
            }
        }

        /// <summary>
        /// Releases the acquired <see cref="MachineLock"/> when disposed.
        /// </summary>
        public struct Releaser : IDisposable
        {
            /// <summary>
            /// The acquired lock.
            /// </summary>
            private readonly MachineLock Lock;

            /// <summary>
            /// Initializes a new instance of the <see cref="Releaser"/> struct.
            /// </summary>
            internal Releaser(MachineLock taskLock)
            {
                this.Lock = taskLock;
            }

            /// <summary>
            /// Releases the acquired lock.
            /// </summary>
            public void Dispose() => this.Lock?.Release();
        }
    }
}
