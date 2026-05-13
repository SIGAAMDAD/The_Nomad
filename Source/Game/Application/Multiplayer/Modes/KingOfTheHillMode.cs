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
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer.Modes;
using Nomad.Game.Domain.Data.Multiplayer.Team;
using Nomad.Game.Domain.Events.Multiplayer;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using Nomad.Networking.Rpc;
using Nomad.Networking.Session;

namespace Nomad.Game.Application.Multiplayer.Modes
{
	/*
	===================================================================================

	KingOfTheHillMode

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class KingOfTheHillMode : ModeBase, IKingOfTheHillMode
	{
		private struct HostState
		{
			public SessionId SessionId { get; set; }
			public TeamId HillOwner { get; set; }
			public uint RedTeamHillSeconds { get; set; }
			public uint BlueTeamHillSeconds { get; set; }
		};

		public override string ModeName => "King of the Hill";
		public override Mode Mode => Mode.KingOfTheHill;

		public IGameEvent<HillStatusChangedEventArgs> HillStatusChanged => _hillStatusChanged;
		private readonly IGameEvent<HillStatusChangedEventArgs> _hillStatusChanged = default;

		public KingOfTheHillInstanceData Snapshot => new KingOfTheHillInstanceData {
		};

		public KingOfTheHillMode(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
		}
	};
};
