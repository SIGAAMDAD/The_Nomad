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
using Nomad.Game.Domain.Data.Multiplayer.Modes;
using Nomad.Game.Domain.Events.Multiplayer;

namespace Nomad.Game.Domain.Interfaces.Multiplayer
{
	public interface IDuelMode
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Multiplayer" )]
		[EventPayload( "WinnerId", typeof( PeerId ), Order = 1 )]
		[EventPayload( "LoserId", typeof( PeerId ), Order = 2 )]
		[EventPayload( "WasTie", typeof( bool ), Order = 3 )]
		IGameEvent<DuelRoundEndEventArgs> DuelRoundEnd { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Multiplayer" )]
		IGameEvent<DuelRoundBeginEventArgs> DuelRoundBegin { get; }

		DuelInstanceData Snapshot { get; }

		bool TryBeginRound();
		bool TryEndRound( PeerId winnerId, PeerId loserId, bool wasTie );
	};
};
