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
using Nomad.Game.Sdk.Renown.Faction;
using Nomad.Game.Sdk.Renown.Contract;

namespace Nomad.Game.Application.Gameplay.Renown.Contract
{
	internal sealed class ContractInstance : IContractInstance
	{
		public IFactionInstance? Owner => _owner;
		private readonly IFactionInstance _owner;

		public ContractInstanceId InstanceId => _instanceId;
		private readonly ContractInstanceId _instanceId;

		public ContractDefinition Definition => _definition;
		private readonly ContractDefinition _definition;

		public ContractParameters Parameters => _parameters;
		private readonly ContractParameters _parameters;

		public ContractStatus Status => _status;
		private ContractStatus _status = ContractStatus.Pending;

		public ContractInstance( IFactionInstance owner, ContractDefinition definition )
		{
			_owner = owner ?? throw new ArgumentNullException( nameof( owner ) );
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
		}
	};
};
