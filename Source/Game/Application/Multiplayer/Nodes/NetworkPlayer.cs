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
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Multiplayer.Nodes {
	public sealed partial class NetworkPlayer : PlayerBase {
		private readonly INetworkSessionService _sessionService;

		public NetworkPlayer( Guid guid, PlayerPrefab prefab, IServiceRegistry scope, IGameEventRegistryService eventFactory, ILoggerService logger, INetworkSessionService sessionService )
			: base( guid, prefab, scope, eventFactory, logger )
		{
			_sessionService = sessionService ?? throw new ArgumentNullException( nameof( sessionService ) );
		}
	};
};