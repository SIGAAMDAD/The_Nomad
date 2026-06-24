/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Renown.Contract;

namespace Nomad.Game.Sdk.Renown.Contract
{
    /// <summary>
    ///
    /// </summary>
    public interface IContractService
    {
        IContractInstance? Current { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Renown.Contract")]
        [EventPayload("ContractId", typeof(ContractInstanceId), Order = 1)]
        [EventPayload("Status", typeof(ContractStatus), Order = 2)]
        IGameEvent<ContractStatusChangedEventArgs> ContractStatusChanged { get; }

        IReadOnlyList<ContractInstanceId> GetActiveContracts();
        IReadOnlyList<ContractInstanceId> GetCompletedContracts();
        IReadOnlyList<ContractInstanceId> GetFailedContracts();
    }
}
