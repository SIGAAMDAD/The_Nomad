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
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;

namespace Nomad.Game.Domain.Interfaces.Player
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPlayerSpawnService : IDisposable
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "PlayerSpawnResultEventArgs" )]
		[EventPayload( "RequestId", typeof( Guid ), Order = 1 )]
		[EventPayload( "PlayerId", typeof( Guid ), Order = 2 )]
		[EventPayload( "Success", typeof( bool ), Order = 3 )]
		IGameEvent<PlayerSpawnResultEventArgs> SpawnResultsReady { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "PlayerSpawnRequestedEventArgs" )]
		[EventPayload( "RequestId", typeof( Guid ), Order = 1 )]
		[EventPayload( "Context", typeof( PlayerSpawnContext ), Order = 2 )]
		IGameEvent<PlayerSpawnRequestedEventArgs> SpawnRequested { get; }
	};
};
