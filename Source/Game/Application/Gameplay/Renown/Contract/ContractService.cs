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
using System.Linq;
using Nomad.Game.Sdk.Events.World;
using Nomad.Game.Sdk.Renown.Contract;

namespace Nomad.Game.Application.Gameplay.Renown.Contract
{
	internal sealed class ContractService : IContractService
	{
		public IContractInstance? Current {
			get {
				throw new System.NotImplementedException();
			}
		}

		private readonly Dictionary<ContractInstanceId, ContractInstance> _contracts = new();

		public IReadOnlyList<ContractInstanceId> GetActiveContracts()
		{
			var active = new List<ContractInstanceId>();
			var contracts = GetSnapshot();

			foreach ( var contract in contracts ) {
				if ( contract.Status == ContractStatus.Active ) {
					active.Add( contract.InstanceId );
				}
			}

			return active;
		}

		public IReadOnlyList<ContractInstanceId> GetCompletedContracts()
		{
			var completed = new List<ContractInstanceId>();
			var contracts = GetSnapshot();

			foreach ( var contract in contracts ) {
				if ( contract.Status == ContractStatus.Completed ) {
					completed.Add( contract.InstanceId );
				}
			}

			return completed;
		}

		public IReadOnlyList<ContractInstanceId> GetFailedContracts()
		{
			var completed = new List<ContractInstanceId>();
			var contracts = GetSnapshot();

			foreach ( var contract in contracts ) {
				if ( contract.Status == ContractStatus.Completed ) {
					completed.Add( contract.InstanceId );
				}
			}

			return completed;
		}

		private List<ContractInstance> GetSnapshot()
		{
			return _contracts.Values.ToList();
		}

		private void OnMinuteChanged( in MinuteChangedEventArgs args )
		{

		}
	};
};
