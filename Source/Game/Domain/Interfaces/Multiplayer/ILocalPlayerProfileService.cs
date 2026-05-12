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
using System.Threading;
using System.Threading.Tasks;
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Multiplayer.Profile;

namespace Nomad.Game.Domain.Interfaces.Multiplayer
{
	public interface ILocalPlayerProfileService : IDisposable
	{
		PeerId LocalPeerId { get; }
		PlayerProfileRecord CurrentProfile { get; }
		PlayerStatsRecord CurrentStats { get; }
		uint LocalRevision { get; }

		void SetLocalPlayer( PeerId peerId, string callsign );
		PlayerStatsRecord UpdateStats( Func<PlayerStatsRecord, PlayerStatsRecord> update );
		void SetStats( PlayerStatsRecord stats );

		ValueTask<PlayerProfileRecord> RefreshStatsFromOnlineAsync( CancellationToken ct = default );
		ValueTask<bool> PushStatsToOnlineAsync( CancellationToken ct = default );
	};
};
