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
using Nomad.Game.Domain.Events.Multiplayer;

namespace Nomad.Game.Domain.Interfaces.Multiplayer
{
	public interface IVoteService
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Multiplayer" )]
		[EventPayload( "InitiatorId", typeof( PeerId ), Order = 1 )]
		[EventPayload( "VictimId", typeof( PeerId ), Order = 2 )]
		IGameEvent<VoteKickStartedEventArgs> VoteKickStarted { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Multiplayer" )]
		IGameEvent<VoteCancelledEventArgs> VoteCancelled { get; }
	};
};
