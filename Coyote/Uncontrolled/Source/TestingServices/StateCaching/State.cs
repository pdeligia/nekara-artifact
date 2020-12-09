﻿// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Microsoft.Coyote.IO;

namespace Microsoft.Coyote.TestingServices.StateCaching
{
    /// <summary>
    /// Represents a snapshot of the program state.
    /// </summary>
    internal sealed class State
    {
        /// <summary>
        /// The fingerprint of the trace step.
        /// </summary>
        internal int Fingerprint { get; private set; }

        /// <summary>
        /// Map from monitors to their liveness status.
        /// </summary>
        internal readonly Dictionary<Monitor, MonitorStatus> MonitorStatus;

        /// <summary>
        /// Ids of the enabled machines. Only relevant
        /// if this is a scheduling trace step.
        /// </summary>
        internal readonly HashSet<ulong> EnabledMachineIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        internal State(int fingerprint, HashSet<ulong> enabledMachineIds, Dictionary<Monitor, MonitorStatus> monitorStatus)
        {
            this.Fingerprint = fingerprint;
            this.EnabledMachineIds = enabledMachineIds;
            this.MonitorStatus = monitorStatus;
        }

        /// <summary>
        /// Pretty prints the state.
        /// </summary>
        internal void PrettyPrint()
        {
            Debug.WriteLine($"Fingerprint: {this.Fingerprint}");
            foreach (var id in this.EnabledMachineIds)
            {
                Debug.WriteLine($"  Enabled machine id: {id}");
            }

            foreach (var m in this.MonitorStatus)
            {
                Debug.WriteLine($"  Monitor status: {m.Key.Id} is {m.Value}");
            }
        }
    }
}
