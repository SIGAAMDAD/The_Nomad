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

namespace Nomad.Game.Sdk.Multiplayer.Voting
{
	public readonly struct VoteId : IEquatable<VoteId>
	{
		public readonly Guid Id;

		public static readonly VoteId Invalid = new VoteId( Guid.Empty );

		public bool IsValid => Id != Guid.Empty;

		public VoteId( Guid id )
		{
			Id = id;
		}

		public bool Equals( VoteId other )
		{
			return Id == other.Id;
		}

		public override bool Equals( object obj )
		{
			return obj is VoteId other && Equals( other );
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return Id.ToString();
		}

		public static bool operator ==( VoteId left, VoteId right )
		{
			return left.Equals( right );
		}

		public static bool operator !=( VoteId left, VoteId right )
		{
			return !left.Equals( right );
		}
	}
}
