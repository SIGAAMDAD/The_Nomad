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
using Nomad.Game.Domain.Data.Multiplayer.Packets;
using Nomad.Game.Prefabs;
using Nomad.Networking.Events;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer
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
		private readonly INetworkSessionService _sessionService;
		private readonly INetworkEventBus _eventBus;
		private readonly INetworkRpcBus _rpcBus;

		public NetworkPlayer( Guid guid, PlayerPrefab prefab, IServiceRegistry scope, IServiceLocator locator, IGameEventRegistryService eventFactory, ILoggerService logger )
			: base( guid, prefab, scope, eventFactory, logger )
		{
			_rpcBus = locator.GetService<INetworkRpcBus>();
			_eventBus = locator.GetService<INetworkEventBus>();
			_sessionService = locator.GetService<INetworkSessionService>();

			_rpcBus.Register<UserInputCommand>( OnUserInputCommand );
		}

		private void OnUserInputCommand( in NetworkRpcContext context, in UserInputCommand rpc )
		{
			if ( context.FromClient ) {
			}
		}
	};
};
