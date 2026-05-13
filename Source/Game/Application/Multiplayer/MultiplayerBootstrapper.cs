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
using Nomad.Core.OnlineServices;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Application.Multiplayer.Profile;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer
{
	internal static class MultiplayerBootstrapper
	{
		public static MultiplayerCoordinator Initialize( IServiceRegistry registry, IServiceLocator locator )
		{
			var logger = locator.GetService<ILoggerService>();
			var eventFactory = locator.GetService<IGameEventRegistryService>();
			var sessionService = locator.GetService<INetworkSessionService>();
			var rpcBus = locator.GetService<INetworkRpcBus>();
			var eventBus = locator.GetService<INetworkEventBus>();
			var messageRegistry = locator.GetService<INetworkMessageRegistry>();

			var votingService = new VotingService(
				sessionService,
				rpcBus,
				eventBus,
				messageRegistry,
				eventFactory
			);
			var lobbyWaitingRoom = new LobbyWaitingRoomService(
				sessionService,
				rpcBus,
				eventBus,
				messageRegistry,
				eventFactory,
				votingService
			);
			var localPlayerProfileService = new LocalPlayerProfileService(
				new PeerId( Constants.LOCAL_GUID ), "Unknown", locator.GetService<IOnlinePlatformService>().Stats, logger
			);

			registry.AddSingleton<IVotingService>( votingService );
			registry.AddSingleton<ILobbyWaitingRoomService>( lobbyWaitingRoom );
			registry.AddSingleton<ILocalPlayerProfileService>( localPlayerProfileService );

			return new MultiplayerCoordinator(
				localPlayerProfileService,
				lobbyWaitingRoom,
				votingService
			);
		}
	};
};
