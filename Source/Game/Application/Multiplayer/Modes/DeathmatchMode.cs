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
using Nomad.Networking.Session;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Events.Multiplayer;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Networking.Rpc;
using Nomad.Networking.Events;
using Nomad.Networking.Messaging;
using System.Collections.Generic;
using Nomad.Game.Domain.Events.Player;

namespace Nomad.Game.Application.Multiplayer.Modes
{
	/*
	===================================================================================

	DeathmatchMode

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class DeathmatchMode : ModeBase, IDeathmatchMode
	{
		private struct HostState
		{
			public SessionId SessionId { get; set; }
			public int[] Scoreboard { get; set; }
		};

		public override string ModeName => "Bloodbath";
		public override Mode Mode => Mode.Deathmatch;

		public IGameEvent<NewDeathmatchLeaderEventArgs> NewDeathmatchLeader {
			get {
				throw new System.NotImplementedException();
			}
		}

		private HostState _hostState;

		private readonly Dictionary<PeerId, int> _peerScoreboardIndex = new();

		public DeathmatchMode(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
			_hostState = new HostState();
		}

		/*
		===============
		TryGetPeerScore
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="peerId"></param>
		/// <param name="score"></param>
		/// <param name="rank"></param>
		/// <returns></returns>
		public bool TryGetPeerScore( PeerId peerId, out int score, out int rank )
		{
			score = 0;
			if ( !_peerScoreboardIndex.TryGetValue( peerId, out rank ) ) {
				return false;
			}

			score = _hostState.Scoreboard[rank];
			return true;
		}
	};
};
