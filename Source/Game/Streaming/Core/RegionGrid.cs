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
using Godot;
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Streaming
{
	internal sealed class RegionGrid
	{
		public const int MAX_REGIONS_X = 32;
		public const int MAX_REGIONS_Z = 32;
		public const int MAX_REGIONS = MAX_REGIONS_X * MAX_REGIONS_Z;

		public int RegionSizeMeters { get; }
		public int MinX { get; }
		public int MinZ { get; }
		public int MaxX { get; }
		public int MaxZ { get; }

		public RegionGrid( RegionStreamingSettings settings )
		{
			settings.Validate();

			RegionSizeMeters = settings.RegionSizeMeters;
			MinX = settings.MinRegionX;
			MinZ = settings.MinRegionZ;
			MaxX = MinX + MAX_REGIONS_X - 1;
			MaxZ = MinZ + MAX_REGIONS_Z - 1;
		}

		public bool IsValid( RegionId id )
		{
			return id.X >= MinX && id.X <= MaxX && id.Z >= MinZ && id.Z <= MaxZ;
		}

		public int ToIndex( RegionId id )
		{
			return (id.Z - MinZ) * MAX_REGIONS_X + (id.X - MinX);
		}

		public bool TryToIndex( RegionId id, out int index )
		{
			if ( !IsValid( id ) ) {
				index = -1;
				return false;
			}

			index = ToIndex( id );
			return true;
		}

		public RegionId FromIndex( int index )
		{
			if ( index < 0 || index >= MAX_REGIONS ) {
				throw new ArgumentOutOfRangeException( nameof( index ) );
			}

			int localX = index % MAX_REGIONS_X;
			int localZ = index / MAX_REGIONS_X;

			return new RegionId( MinX + localX, MinZ + localZ );
		}

		public RegionId WorldToRegion( Vector3 world )
		{
			return WorldToRegion( world.X, world.Z );
		}

		public RegionId WorldToRegion( float worldX, float worldZ )
		{
			int x = (int)MathF.Floor( worldX / RegionSizeMeters );
			int z = (int)MathF.Floor( worldZ / RegionSizeMeters );

			x = Math.Clamp( x, MinX, MaxX );
			z = Math.Clamp( z, MinZ, MaxZ );

			return new RegionId( x, z );
		}

		public Vector3 RegionOrigin( RegionId id )
		{
			return new Vector3(
				id.X * RegionSizeMeters,
				0.0f,
				id.Z * RegionSizeMeters
			);
		}

		public Aabb RegionBounds( RegionId id )
		{
			return new Aabb(
				RegionOrigin( id ),
				new Vector3( RegionSizeMeters, 0.0f, RegionSizeMeters )
			);
		}

		public static int Chebyshev( RegionId a, RegionId b )
		{
			return Math.Max( Math.Abs( a.X - b.X ), Math.Abs( a.Z - b.Z ) );
		}

		public static RegionOffset[] BuildScanOffsets( int maxRadius )
		{
			RangeGuard.ThrowIfNegative( maxRadius, nameof( maxRadius ) );

			int size = maxRadius * 2 + 1;
			RegionOffset[] offsets = new RegionOffset[size * size];

			int n = 0;
			for ( int z = -maxRadius; z <= maxRadius; z++ ) {
				for ( int x = -maxRadius; x <= maxRadius; x++ ) {
					offsets[n++] = new RegionOffset( x, z );
				}
			}

			Array.Sort( offsets, static ( a, b ) => {
				int ringCompare = a.ChebyshevDistance.CompareTo( b.ChebyshevDistance );
				if ( ringCompare != 0 ) {
					return ringCompare;
				}

				return a.ManhattanDistance.CompareTo( b.ManhattanDistance );
			} );

			return offsets;
		}
	};
};
