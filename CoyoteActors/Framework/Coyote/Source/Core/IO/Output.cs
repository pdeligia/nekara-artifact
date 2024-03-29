﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

namespace Microsoft.CoyoteActors.IO
{
    /// <summary>
    /// Static class implementing output methods.
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// The underlying logger.
        /// </summary>
        internal static ILogger Logger = new ConsoleLogger();

        /// <summary>
        ///  Writes the specified string value to the output stream.
        /// </summary>
        public static void Write(string value)
        {
            Logger.Write(value);
        }

        /// <summary>
        /// Writes the text representation of the specified array
        /// of objects to the output stream.
        /// </summary>
        public static void Write(string format, params object[] args)
        {
            Logger.Write(format, args);
        }

        /// <summary>
        /// Writes the specified string value, followed by the
        /// current line terminator, to the output stream.
        /// </summary>
        public static void WriteLine(string value)
        {
            Logger.WriteLine(value);
        }

        /// <summary>
        /// Writes the text representation of the specified array
        /// of objects, followed by the current line terminator, to
        /// the output stream.
        /// </summary>
        public static void WriteLine(string format, params object[] args)
        {
            Logger.WriteLine(format, args);
        }
    }
}
