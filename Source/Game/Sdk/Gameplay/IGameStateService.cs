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
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Events.Gameplay;

namespace Nomad.Game.Sdk.Gameplay
{
	/// <summary>
	/// 
	/// </summary>
	public interface IGameStateService : IDisposable
	{
		GameState Current { get; set; }

		[Event( nameSpace: "Nomad.Game.Sdk.Events.Gameplay", PayloadName = "GameStateChangedEventArgs" )]
		[EventPayload( "PrevState", typeof( GameState ), Order = 1 )]
		[EventPayload( "CurrentState", typeof( GameState ), Order = 2 )]
		IGameEvent<GameStateChangedEventArgs> StateChanged { get; }

		void SetState( GameState state );
	}
}
