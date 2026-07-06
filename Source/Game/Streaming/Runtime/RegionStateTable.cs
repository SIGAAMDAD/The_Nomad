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

namespace Nomad.Game.Streaming
{
	internal sealed class RegionStateTable
	{
		public RegionRecord[] Regions { get; } = new RegionRecord[RegionGrid.MAX_REGIONS];
		public RegionManifest[] Manifests { get; } = new RegionManifest[RegionGrid.MAX_REGIONS];
		public PackedScene[] SceneCache { get; } = new PackedScene[RegionGrid.MAX_REGIONS];
		public StreamedRegionChunk[] Instances { get; } = new StreamedRegionChunk[RegionGrid.MAX_REGIONS];

		public void Initialize( RegionGrid grid )
		{
			for ( int i = 0; i < Regions.Length; i++ ) {
				RegionId id = grid.FromIndex( i );

				Regions[i] = new RegionRecord {
					Index = (ushort)i,
					X = (short)id.X,
					Z = (short)id.Z,
					Flags = RegionFlags.TargetUnloaded,
					Distance = byte.MaxValue,
					PredictedDistance = byte.MaxValue,
					EstimatedCost = 0
				};
			}
		}

		public void ClearRuntimeState()
		{
			Array.Clear( SceneCache, 0, SceneCache.Length );
			Array.Clear( Instances, 0, Instances.Length );

			for ( int i = 0; i < Regions.Length; i++ ) {
				Regions[i].Flags &= RegionFlags.HasManifest | RegionFlags.TargetMask;
				Regions[i].Flags = (Regions[i].Flags & ~RegionFlags.TargetMask) | RegionFlags.TargetUnloaded | (Regions[i].Flags & RegionFlags.HasManifest);
			}
		}

		public void SetManifest( RegionGrid grid, RegionManifest manifest )
		{
			if ( !manifest.IsDefined ) {
				return;
			}

			if ( !grid.TryToIndex( manifest.Id, out int index ) ) {
				GD.PushWarning( $"Ignoring region manifest outside grid: {manifest.Id}" );
				return;
			}

			Manifests[index] = manifest;
			Regions[index].Flags |= RegionFlags.HasManifest;
			Regions[index].EstimatedCost = EstimateCost( manifest );
		}

		public void ClearManifest( RegionGrid grid, RegionId id )
		{
			if ( !grid.TryToIndex( id, out int index ) ) {
				return;
			}

			Manifests[index] = default;
			Regions[index].Flags &= ~RegionFlags.HasManifest;
			Regions[index].EstimatedCost = 0;
		}

		private static ushort EstimateCost( RegionManifest manifest )
		{
			long total = Math.Max( 0, manifest.EstimatedCPUBytes ) + Math.Max( 0, manifest.EstimatedGPUBytes );
			long kb = total / 1024;

			if ( kb <= 0 ) {
				return 0;
			}

			return (ushort)Math.Min( ushort.MaxValue, kb );
		}
	}
}
