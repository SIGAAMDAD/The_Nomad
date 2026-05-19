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
using Nomad.Game.Domain.Data.Player.State;
using Nomad.Game.Domain.Events.Player;

namespace Nomad.Game.Domain.Interfaces.Player
{
	/// <summary>
	///
	/// </summary>
	public interface IPlayerStateReader
	{
		PlayerStateId Current { get; }
		bool IsIdle { get; }
		bool IsMoving { get; }
		bool IsDead { get; }
		bool IsRestingAtCheckpoint { get; }

		bool CanMove { get; }
		bool CanTakeInput { get; }
		bool CanTakeDamage { get; }

		[Event( nameSpace: "Nomad.Game.Domain.Events.Player", PayloadName = "PlayerStateChangedEventArgs" )]
		[EventPayload( "OldState", typeof( PlayerStateId ), Order = 1 )]
		[EventPayload( "NewState", typeof( PlayerStateId ), Order = 2 )]
		IGameEvent<PlayerStateChangedEventArgs> StateChanged { get; }
	};
};
