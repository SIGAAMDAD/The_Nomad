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
using Nomad.Core.Util;
using Nomad.Game.Sdk.Multiplayer.Profile;

namespace Nomad.Game.Multiplayer.Profile
{
	internal readonly struct PlayerProfileStatDescriptor
	{
		public string PropertyName { get; }
		public InternString StatKey { get; }
		public Func<PlayerStatsRecord, ulong> Read { get; }
		public Func<PlayerStatsRecord, ulong, PlayerStatsRecord> Write { get; }

		public PlayerProfileStatDescriptor(
			string propertyName,
			string statKey,
			Func<PlayerStatsRecord, ulong> read,
			Func<PlayerStatsRecord, ulong, PlayerStatsRecord> write
		)
		{
			PropertyName = propertyName;
			StatKey = new InternString( statKey );
			Read = read;
			Write = write;
		}
	}
}
