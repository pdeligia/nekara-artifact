﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

namespace Microsoft.CoyoteActors.TestingServices.Scheduling
{
    /// <summary>
    /// Interface for random number generators.
    /// </summary>
    public interface IRandomNumberGenerator
    {
        /// <summary>
        /// The seed currently used by the generator.
        /// </summary>
        int Seed { get; set; }

        /// <summary>
        /// Returns a non-negative random number.
        /// </summary>
        int Next();

        /// <summary>
        /// Returns a non-negative random number less than maxValue.
        /// </summary>
        /// <param name="maxValue">Exclusive upper bound</param>
        int Next(int maxValue);

        /// <summary>
        /// Returns a random floating-point number that is greater
        /// than or equal to 0.0, and less than 1.0.
        /// </summary>
        double NextDouble();
    }
}
