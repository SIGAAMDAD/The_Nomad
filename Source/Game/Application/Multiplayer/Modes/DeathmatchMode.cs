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
		public override string ModeName => "Bloodbath";
		public override Mode Mode => Mode.Deathmatch;

		public IGameEvent<NewDeathmatchLeaderEventArgs> NewDeathmatchLeader {
			get {
				throw new System.NotImplementedException();
			}
		}
		public DeathmatchMode( INetworkSessionService sessionService, INetworkRpcBus rpcBus, INetworkEventBus eventBus, IGameEventRegistryService eventFactory )
			: base( sessionService, rpcBus, eventBus, eventFactory )
		{
		}
	};
};
