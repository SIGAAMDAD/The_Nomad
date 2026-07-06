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

using System;
using System.Collections.Generic;
using System.Linq;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Renown.Contract;
using Nomad.Game.Sdk.Renown.Contract;

namespace Nomad.Game.Gameplay.Renown.Contract
{
	internal sealed class ContractService : IContractService, IDisposable
	{
		public IContractInstance? Current {
			get {
				throw new System.NotImplementedException();
			}
		}

		private readonly Dictionary<ContractInstanceId, ContractInstance> _contracts = new();

		private bool _isDisposed = false;

		public IGameEvent<ContractStatusChangedEventArgs> ContractStatusChanged => _contractStatusChanged;
		private readonly IGameEvent<ContractStatusChangedEventArgs> _contractStatusChanged = null;

		public ContractService( IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_contractStatusChanged = eventFactory
				.GetEvent<ContractStatusChangedEventArgs>(
					ContractStatusChangedEventArgs.Name,
					ContractStatusChangedEventArgs.NameSpace
				);
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_contractStatusChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public bool TryClaimReward( ContractInstanceId contractId )
		{
			if ( !_contracts.TryGetValue( contractId, out var contract ) ) {
				return false;
			}

			if ( contract.Status != ContractStatus.Completed ) {
				// cannot claim a contract if it isn't marked as completed.
				return false;
			}

			return true;
		}

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
	};
};
