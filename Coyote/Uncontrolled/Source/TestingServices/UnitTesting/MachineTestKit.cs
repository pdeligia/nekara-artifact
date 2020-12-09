// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Coyote.TestingServices.Runtime;

namespace Microsoft.Coyote.TestingServices
{
    /// <summary>
    /// Provides methods for testing an actor of type <typeparamref name="T"/> in isolation.
    /// </summary>
    /// <typeparam name="T">The actor type to test.</typeparam>
    public sealed class ActorTestKit<T>
        where T : Actor
    {
        /// <summary>
        /// The actor testing runtime.
        /// </summary>
        private readonly ActorTestingRuntime Runtime;

        /// <summary>
        /// The instance of the actor being tested.
        /// </summary>
        public readonly T Actor;

        /// <summary>
        /// True if the actor has started its execution, else false.
        /// </summary>
        private bool IsRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorTestKit{T}"/> class.
        /// </summary>
        /// <param name="configuration">The runtime configuration to use.</param>
        public ActorTestKit(Configuration configuration)
        {
            configuration = configuration ?? Configuration.Create();
            this.Runtime = new ActorTestingRuntime(typeof(T), configuration);
            this.Actor = this.Runtime.Actor as T;
            this.IsRunning = false;
            this.Runtime.OnFailure += ex =>
            {
                this.Runtime.Logger.WriteLine(ex.ToString());
            };
        }

        /// <summary>
        /// Transitions the actor to its start state, passes the optional specified event
        /// and invokes its on-entry handler, if there is one available. This method returns
        /// a task that completes when the actor reaches quiescence (typically when the
        /// event handler finishes executing because there are not more events to dequeue,
        /// or when the actor asynchronously waits to receive an event).
        /// </summary>
        /// <param name="initialEvent">Optional event used during initialization.</param>
        /// <returns>Task that represents the asynchronous operation.</returns>
        public Task StartActorAsync(Event initialEvent = null)
        {
            this.Runtime.Assert(!this.IsRunning,
                string.Format("Actor '{0}' is already running.", this.Actor.Id));
            this.IsRunning = true;
            return this.Runtime.StartAsync(initialEvent);
        }

        /// <summary>
        /// Sends an event to the actor and starts its event handler. This method returns
        /// a task that completes when the actor reaches quiescence (typically when the
        /// event handler finishes executing because there are not more events to dequeue,
        /// or when the actor asynchronously waits to receive an event).
        /// </summary>
        /// <returns>Task that represents the asynchronous operation.</returns>
        public Task SendEventAsync(Event e)
        {
            this.Runtime.Assert(this.IsRunning,
                string.Format("Actor '{0}' is not running.", this.Actor.Id));
            return this.Runtime.SendEventAndExecuteAsync(this.Runtime.Actor.Id, e, null, Guid.Empty, null);
        }

        /// <summary>
        /// Invokes the actor method with the specified name, and passing the specified
        /// optional parameters. Use this method to invoke private methods of the actor.
        /// </summary>
        /// <param name="methodName">The name of the actor method.</param>
        /// <param name="parameters">The parameters to the method.</param>
        public object Invoke(string methodName, params object[] parameters)
        {
            MethodInfo method = this.GetMethod(methodName, false, null);
            return method.Invoke(this.Actor, parameters);
        }

        /// <summary>
        /// Invokes the actor method with the specified name and parameter types, and passing the
        /// specified optional parameters. Use this method to invoke private methods of the actor.
        /// </summary>
        /// <param name="methodName">The name of the actor method.</param>
        /// <param name="parameterTypes">The parameter types of the method.</param>
        /// <param name="parameters">The parameters to the method.</param>
        public object Invoke(string methodName, Type[] parameterTypes, params object[] parameters)
        {
            MethodInfo method = this.GetMethod(methodName, false, parameterTypes);
            return method.Invoke(this.Actor, parameters);
        }

        /// <summary>
        /// Invokes the asynchronous actor method with the specified name, and passing the specified
        /// optional parameters. Use this method to invoke private methods of the actor.
        /// </summary>
        /// <param name="methodName">The name of the actor method.</param>
        /// <param name="parameters">The parameters to the method.</param>
        public async Task<object> InvokeAsync(string methodName, params object[] parameters)
        {
            MethodInfo method = this.GetMethod(methodName, true, null);
            var task = (Task)method.Invoke(this.Actor, parameters);
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }

        /// <summary>
        /// Invokes the asynchronous actor method with the specified name and parameter types, and passing
        /// the specified optional parameters. Use this method to invoke private methods of the actor.
        /// </summary>
        /// <param name="methodName">The name of the actor method.</param>
        /// <param name="parameterTypes">The parameter types of the method.</param>
        /// <param name="parameters">The parameters to the method.</param>
        public async Task<object> InvokeAsync(string methodName, Type[] parameterTypes, params object[] parameters)
        {
            MethodInfo method = this.GetMethod(methodName, true, parameterTypes);
            var task = (Task)method.Invoke(this.Actor, parameters);
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }

        /// <summary>
        /// Uses reflection to get the actor method with the specified name and parameter types.
        /// </summary>
        /// <param name="methodName">The name of the actor method.</param>
        /// <param name="isAsync">True if the method is async, else false.</param>
        /// <param name="parameterTypes">The parameter types of the method.</param>
        private MethodInfo GetMethod(string methodName, bool isAsync, Type[] parameterTypes)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo method;
            if (parameterTypes is null)
            {
                method = this.Actor.GetType().GetMethod(methodName, bindingFlags);
            }
            else
            {
                method = this.Actor.GetType().GetMethod(methodName, bindingFlags,
                    Type.DefaultBinder, parameterTypes, null);
            }

            this.Runtime.Assert(method != null,
                string.Format("Unable to invoke method '{0}' in actor '{1}'.",
                methodName, this.Actor.Id));
            this.Runtime.Assert(method.GetCustomAttribute(typeof(AsyncStateActorAttribute)) is null != isAsync,
                string.Format("Must invoke {0}method '{1}' of actor '{2}' using '{3}'.",
                isAsync ? string.Empty : "async ", methodName, this.Actor.Id, isAsync ? "Invoke" : "InvokeAsync"));

            return method;
        }

        /// <summary>
        /// Asserts if the specified condition holds.
        /// </summary>
        public void Assert(bool predicate)
        {
            this.Runtime.Assert(predicate);
        }

        /// <summary>
        /// Asserts if the specified condition holds.
        /// </summary>
        public void Assert(bool predicate, string s, object arg0)
        {
            this.Runtime.Assert(predicate, s, arg0);
        }

        /// <summary>
        /// Asserts if the specified condition holds.
        /// </summary>
        public void Assert(bool predicate, string s, object arg0, object arg1)
        {
            this.Runtime.Assert(predicate, s, arg0, arg1);
        }

        /// <summary>
        /// Asserts if the specified condition holds.
        /// </summary>
        public void Assert(bool predicate, string s, object arg0, object arg1, object arg2)
        {
            this.Runtime.Assert(predicate, s, arg0, arg1, arg2);
        }

        /// <summary>
        /// Asserts if the specified condition holds.
        /// </summary>
        public void Assert(bool predicate, string s, params object[] args)
        {
            this.Runtime.Assert(predicate, s, args);
        }

        /// <summary>
        /// Asserts that the actor has transitioned to the state with the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="S">The type of the actor state.</typeparam>
        public void AssertStateTransition<S>()
            where S : ActorState
        {
            this.AssertStateTransition(typeof(S).FullName);
        }

        /// <summary>
        /// Asserts that the actor has transitioned to the state with the specified name
        /// (either <see cref="Type.FullName"/> or <see cref="MemberInfo.Name"/>).
        /// </summary>
        /// <param name="actorStateName">The name of the actor state.</param>
        public void AssertStateTransition(string actorStateName)
        {
            bool predicate = this.Actor.CurrentState.FullName.Equals(actorStateName) ||
                this.Actor.CurrentState.FullName.Equals(
                    this.Actor.CurrentState.DeclaringType.FullName + "+" + actorStateName);
            this.Runtime.Assert(predicate, string.Format("Actor '{0}' is in state '{1}', not in '{2}'.",
                this.Actor.Id, this.Actor.CurrentState.FullName, actorStateName));
        }

        /// <summary>
        /// Asserts that the actor is waiting (or not) to receive an event.
        /// </summary>
        public void AssertIsWaitingToReceiveEvent(bool isWaiting)
        {
            this.Runtime.Assert(this.Runtime.IsActorWaitingToReceiveEvent == isWaiting,
                "Actor '{0}' is {1}waiting to receive an event.",
                this.Actor.Id, this.Runtime.IsActorWaitingToReceiveEvent ? string.Empty : "not ");
        }

        /// <summary>
        /// Asserts that the actor inbox contains the specified number of events.
        /// </summary>
        /// <param name="numEvents">The number of events in the inbox.</param>
        public void AssertInboxSize(int numEvents)
        {
            this.Runtime.Assert(this.Runtime.ActorInbox.Size == numEvents,
                "Actor '{0}' contains '{1}' events in its inbox.",
                this.Actor.Id, this.Runtime.ActorInbox.Size);
        }
    }
}
