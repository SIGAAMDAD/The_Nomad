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
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Entity;

namespace Nomad.Game.Domain.Interfaces.Player
{
	public interface IPlayerBase : IEntityBase
	{
		PlayerId PlayerId { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "PlayerDieEventArgs" )]
		[EventPayload( "PlayerId", typeof( PlayerId ), Order = 1 )]
		[EventPayload( "KillerId", typeof( EntityId ), Order = 2 )]
		IGameEvent<PlayerDieEventArgs> Die { get; }
	};
};
