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
using System.Runtime.CompilerServices;

namespace Nomad.Game.Streaming
{
	internal sealed class RegionTransitionPlanner
	{
		private readonly RegionOperationQueue _promotionQueue;
		private readonly RegionOperationQueue _demotionQueue;

		public RegionTransitionPlanner( RegionOperationQueue promotionQueue, RegionOperationQueue demotionQueue )
		{
			_promotionQueue = promotionQueue ?? throw new ArgumentNullException( nameof( promotionQueue ) );
			_demotionQueue = demotionQueue ?? throw new ArgumentNullException( nameof( demotionQueue ) );
		}

		public void RefreshAll( RegionStateTable state )
		{
			for ( int i = 0; i < state.Regions.Length; i++ ) {
				RefreshRegion( state, i );
			}
		}

		public void RefreshRegion( RegionStateTable state, int index )
		{
			ref RegionRecord r = ref state.Regions[index];

			RegionFlags flags = r.Flags;
			RegionFlags queueBits = flags & RegionFlags.QueueMask;

			flags &= ~RegionFlags.PendingMask;

			RegionManifest manifest = state.Manifests[index];

			if ( !manifest.HasScene ) {
				r.Flags = RefreshEmptyRegionFlags( flags );
				EnqueueIfNeeded( ref r, index );
				return;
			}

			RegionFlags target = flags & RegionFlags.TargetMask;

			bool wantsWarm = HasAny( target, RegionFlags.TargetWarm | RegionFlags.TargetHot | RegionFlags.TargetActive );
			bool wantsHot = HasAny( target, RegionFlags.TargetHot | RegionFlags.TargetActive );
			bool wantsActive = HasAny( target, RegionFlags.TargetActive );

			RegionFlags pending =
				ComputeUpgradeWorkFlags( flags, wantsWarm, wantsHot, wantsActive ) |
				ComputeDowngradeWorkFlags( flags, wantsWarm, wantsHot, wantsActive );

			// Preserve queue bits that were present before pending recalculation.
			r.Flags = flags | pending | queueBits;

			EnqueueIfNeeded( ref r, index );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static RegionFlags RefreshEmptyRegionFlags( RegionFlags flags )
		{
			return flags | FlagIf(
				HasAny( flags, RegionFlags.HasFullInstance | RegionFlags.HasFullRes | RegionFlags.SceneLoadRequested ),
				RegionFlags.NeedsEvict
			);
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static RegionFlags ComputeUpgradeWorkFlags(
			RegionFlags flags,
			bool wantsWarm,
			bool wantsHot,
			bool wantsActive
		)
		{
			bool hasFullRes = HasBit( flags, RegionFlags.HasFullRes );
			bool hasFullInstance = HasBit( flags, RegionFlags.HasFullInstance );
			bool sceneLoadFailed = HasBit( flags, RegionFlags.SceneLoadFailed );

			bool needsLoad = wantsWarm && !hasFullRes && !sceneLoadFailed;
			bool needsInstantiate = wantsHot && hasFullRes && !hasFullInstance;
			bool needsVisual = wantsHot && hasFullInstance && !HasBit( flags, RegionFlags.VisualFull );
			bool needsLight = wantsHot && hasFullInstance && !HasRequiredLighting( flags, wantsActive );
			bool needsPhysics = wantsActive && hasFullInstance && !HasBit( flags, RegionFlags.PhysicsFull );
			bool needsGameplay = wantsActive && hasFullInstance && !HasBit( flags, RegionFlags.GameplayActive );

			return
				FlagIf( needsLoad, RegionFlags.NeedsLoad ) |
				FlagIf( needsInstantiate, RegionFlags.NeedsInstantiate ) |
				FlagIf( needsVisual, RegionFlags.NeedsVisual ) |
				FlagIf( needsLight, RegionFlags.NeedsLight ) |
				FlagIf( needsPhysics, RegionFlags.NeedsPhysics ) |
				FlagIf( needsGameplay, RegionFlags.NeedsGameplay );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static RegionFlags ComputeDowngradeWorkFlags(
			RegionFlags flags,
			bool wantsWarm,
			bool wantsHot,
			bool wantsActive
		)
		{
			bool hasActiveRuntime = HasAny(
				flags,
				RegionFlags.PhysicsFull | RegionFlags.GameplayActive | RegionFlags.LightingFull
			);

			bool hasHotRuntime = HasAny(
				flags,
				RegionFlags.VisualFull |
				RegionFlags.VisualProxy |
				RegionFlags.LightingNoShadows |
				RegionFlags.LightingFull
			);

			bool hasFullInstance = HasBit( flags, RegionFlags.HasFullInstance );

			bool hasWarmRuntime = HasAny(
				flags,
				RegionFlags.HasFullRes |
				RegionFlags.SceneLoadRequested |
				RegionFlags.SceneLoadFailed
			);

			return
				FlagIf( !wantsActive && hasActiveRuntime, RegionFlags.NeedsDowngrade ) |
				FlagIf( !wantsHot && hasHotRuntime, RegionFlags.NeedsDowngrade ) |
				FlagIf( !wantsHot && hasFullInstance, RegionFlags.NeedsEvict ) |
				FlagIf( !wantsWarm && hasWarmRuntime, RegionFlags.NeedsEvict );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool HasRequiredLighting( RegionFlags flags, bool wantsActive )
		{
			return wantsActive
				? HasBit( flags, RegionFlags.LightingFull )
				: HasAny( flags, RegionFlags.LightingNoShadows | RegionFlags.LightingFull );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool HasBit( RegionFlags flags, RegionFlags bit )
		{
			return (flags & bit) != 0;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static RegionFlags FlagIf( bool condition, RegionFlags flag )
		{
			return condition ? flag : 0;
		}

		private void EnqueueIfNeeded( ref RegionRecord r, int index )
		{
			if ( (r.Flags & RegionFlags.PendingDemotionMask) != 0 ) {
				TryEnqueueDemotion( ref r, index );
				return;
			}

			if ( (r.Flags & RegionFlags.PendingPromotionMask) != 0 ) {
				TryEnqueuePromotion( ref r, index );
			}
		}

		private void TryEnqueuePromotion( ref RegionRecord r, int index )
		{
			if ( (r.Flags & RegionFlags.QueuedPromotion) != 0 ) {
				return;
			}

			if ( _promotionQueue.TryEnqueue( (ushort)index ) ) {
				r.Flags |= RegionFlags.QueuedPromotion;
			}
		}

		private void TryEnqueueDemotion( ref RegionRecord r, int index )
		{
			if ( (r.Flags & RegionFlags.QueuedDemotion) != 0 ) {
				return;
			}

			if ( _demotionQueue.TryEnqueue( (ushort)index ) ) {
				r.Flags |= RegionFlags.QueuedDemotion;
			}
		}

		private static bool HasAny( RegionFlags flags, RegionFlags mask )
		{
			return (flags & mask) != 0;
		}
	}
}
