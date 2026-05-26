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
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Player;

namespace Nomad.Game.Sdk.Player.Stats
{
	/// <summary>
	///
	/// </summary>
	public interface IPlayerDerivedStatService : IDisposable
	{
		[Event( nameSpace: "Nomad.Game.Sdk.Events.Player", PayloadName = "PlayerDerivedStatChangedEventArgs" )]
		[EventPayload( "PlayerId", typeof( PlayerId ), Order = 1 )]
		[EventPayload( "NewValue", typeof( float ), Order = 2 )]
		[EventPayload( "OldValue", typeof( float ), Order = 3 )]
		[EventPayload( "StatId", typeof( DerivedStatType ), Order = 4 )]
		IGameEvent<PlayerDerivedStatChangedEventArgs> DerivedStatChanged { get; }

		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		float GetValue( DerivedStatType type );

		/// <summary>
		///
		/// </summary>
		void FlushDirty();
	}
}
