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

using Nomad.Core.Util;

namespace Nomad.Game.Domain.Data.Multiplayer.Team
{
	public readonly struct TeamInfo
	{
		public TeamId Id { get; }
		public InternString Name { get; }
		public int MaxMembers { get; }
		public int MemberCount { get; }
		public bool IsPlayable { get; }

		public bool IsFull => MaxMembers > 0 && MemberCount >= MaxMembers;

		public TeamInfo( TeamId id, string name, int maxMembers, int memberCount, bool isPlayable )
		{
			Id = id;
			Name = name != null ? new InternString( name ) : InternString.Empty;
			MaxMembers = maxMembers;
			MemberCount = memberCount;
			IsPlayable = isPlayable;
		}
	};
};
