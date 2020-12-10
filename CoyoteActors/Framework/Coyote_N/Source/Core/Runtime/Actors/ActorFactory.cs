// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Coyote.Runtime
{
    /// <summary>
    /// Factory for creating actors.
    /// </summary>
    internal static class ActorFactory
    {
        /// <summary>
        /// Cache storing actor constructors.
        /// </summary>
        private static readonly Dictionary<Type, Func<Actor>> ActorConstructorCache =
            new Dictionary<Type, Func<Actor>>();

        /// <summary>
        /// Creates a new <see cref="Actor"/> of the specified type.
        /// </summary>
        /// <param name="type">Type of the actor.</param>
        /// <returns>The created actor.</returns>
        public static Actor Create(Type type)
        {
            lock (ActorConstructorCache)
            {
                if (!ActorConstructorCache.TryGetValue(type, out Func<Actor> constructor))
                {
                    constructor = Expression.Lambda<Func<Actor>>(
                        Expression.New(type.GetConstructor(Type.EmptyTypes))).Compile();
                    ActorConstructorCache.Add(type, constructor);
                }

                return constructor();
            }
        }
    }
}
