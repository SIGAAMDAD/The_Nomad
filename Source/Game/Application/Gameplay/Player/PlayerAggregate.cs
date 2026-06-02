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

using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Prefabs;
using Nomad.Game.Application.Gameplay.Player;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerAggregrate

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerAggregate : PlayerBase
	{
		private readonly IJournalService _journalService;
		private readonly IValdensBookService _valdensBookService;

		public PlayerAggregate( PlayerId playerId, PlayerPrefab prefab, int localPlayerIndex, IGameEventRegistryService eventFactory, ILoggerService logger )
			: base( playerId, prefab, localPlayerIndex, eventFactory, logger )
		{
		}
	};
};
