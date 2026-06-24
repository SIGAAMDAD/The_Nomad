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
using Nomad.Game.Sdk.Npc;
using Nomad.Game.Sdk.Renown;

namespace Nomad.Game.Application.Gameplay.Renown
{
	internal sealed class FactionInstance : IFactionInstance
	{
		public FactionInstanceId Id => _instanceId;
		private readonly FactionInstanceId _instanceId;

		public FactionDefinition Definition => _definition;
		private readonly FactionDefinition _definition;

		public IMercenaryMaster Owner => _owner;
		private IMercenaryMaster _owner;

		public FactionInstance( FactionInstanceId instanceId, FactionDefinition definition )
		{
			_definition = definition ?? throw new ArgumentNullException( nameof( definition ) );
			_instanceId = instanceId;
		}

		public IReadOnlyCollection<ContractInstanceId> GetActiveContracts()
		{
			return null;
		}
	};
};
