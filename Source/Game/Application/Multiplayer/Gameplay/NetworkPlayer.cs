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
using Nomad.Core.OnlineServices;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Prefabs;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer.Gameplay
{
	/*
	===================================================================================

	NetworkPlayer

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class NetworkPlayer : PlayerBase
	{
		private readonly MultiplayerObject _multiplayer;

		public NetworkPlayer( PeerId peerId, PlayerPrefab prefab, IServiceLocator locator, IGameEventRegistryService eventFactory, ILoggerService logger )
			: base( new PlayerId( peerId ), prefab, eventFactory, logger )
		{
			var sessionService = locator.GetService<INetworkSessionService>();
			var eventBus = locator.GetService<INetworkEventBus>();
			var rpcBus = locator.GetService<INetworkRpcBus>();
			var messageRegistry = locator.GetService<INetworkMessageRegistry>();

			_multiplayer = new MultiplayerObject( sessionService, rpcBus, eventBus, messageRegistry, eventFactory );
		}
	};
};
