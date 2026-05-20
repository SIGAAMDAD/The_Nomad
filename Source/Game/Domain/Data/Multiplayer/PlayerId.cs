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
using System.Diagnostics.CodeAnalysis;
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Entities;

namespace Nomad.Game.Domain.Data.Multiplayer
{
	/// <summary>
	///
	/// </summary>
	public readonly struct PlayerId : IEquatable<PlayerId>
	{
		public static PlayerId Invalid => new PlayerId( PeerId.Invalid );

		public Guid Guid => Id;
		public bool IsValid => this != Invalid;

		public readonly Guid Id;

		public PlayerId( PeerId value )
		{
			Id = value.Id;
		}

		public PlayerId( EntityId value )
		{
			Id = value.Id;
		}

		public void ThrowIfInvalid( string callingMethod )
		{
			if ( !IsValid ) {
				throw new InvalidOperationException( $"{callingMethod} given an invalid PlayerId!" );
			}
		}

		public override bool Equals( [NotNullWhen( true )] object? obj )
		{
			return obj is PlayerId other && Equals( other );
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public bool Equals( PlayerId other )
		{
			return Id == other.Id;
		}

		public static bool operator ==( PlayerId left, PlayerId right )
		{
			return left.Equals( right );
		}

		public static bool operator !=( PlayerId left, PlayerId right )
		{
			return !left.Equals( right );
		}

		public static bool operator ==( PlayerId left, PeerId right )
		{
			return left.Id.Equals( right );
		}

		public static bool operator !=( PlayerId left, PeerId right )
		{
			return !left.Id.Equals( right );
		}

		public static bool operator ==( PlayerId left, EntityId right )
		{
			return left.Id.Equals( right );
		}

		public static bool operator !=( PlayerId left, EntityId right )
		{
			return !left.Id.Equals( right );
		}
	};
};
