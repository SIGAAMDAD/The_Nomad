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

namespace Nomad.Game.Infrastructure.Streaming
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
			RegionManifest manifest = state.Manifests[index];

			RegionFlags queueBits = r.Flags & RegionFlags.QueueMask;
			r.Flags &= ~RegionFlags.PendingMask;

			RegionFlags target = r.Flags & RegionFlags.TargetMask;

			bool wantsWarm = HasAny( target, RegionFlags.TargetWarm | RegionFlags.TargetHot | RegionFlags.TargetActive );
			bool wantsHot = HasAny( target, RegionFlags.TargetHot | RegionFlags.TargetActive );
			bool wantsActive = HasAny( target, RegionFlags.TargetActive );

			if ( !manifest.HasScene ) {
				// Empty regions are valid. They should not keep stale runtime content.
				if ( HasAny( r.Flags, RegionFlags.HasFullInstance | RegionFlags.HasFullRes | RegionFlags.SceneLoadRequested ) ) {
					r.Flags |= RegionFlags.NeedsEvict;
				}

				EnqueueIfNeeded( ref r, index );
				return;
			}

			if ( wantsWarm && (r.Flags & RegionFlags.HasFullRes) == 0 && (r.Flags & RegionFlags.SceneLoadFailed) == 0 ) {
				r.Flags |= RegionFlags.NeedsLoad;
			}

			if ( wantsHot && (r.Flags & RegionFlags.HasFullRes) != 0 && (r.Flags & RegionFlags.HasFullInstance) == 0 ) {
				r.Flags |= RegionFlags.NeedsInstantiate;
			}

			if ( wantsHot && (r.Flags & RegionFlags.HasFullInstance) != 0 && (r.Flags & RegionFlags.VisualFull) == 0 ) {
				r.Flags |= RegionFlags.NeedsVisual;
			}

			if ( wantsHot && (r.Flags & RegionFlags.HasFullInstance) != 0 ) {
				bool wantsFullLight = wantsActive;
				bool hasRequiredLight = wantsFullLight
					? (r.Flags & RegionFlags.LightingFull) != 0
					: HasAny( r.Flags, RegionFlags.LightingNoShadows | RegionFlags.LightingFull );

				if ( !hasRequiredLight ) {
					r.Flags |= RegionFlags.NeedsLight;
				}
			}

			if ( wantsActive && (r.Flags & RegionFlags.HasFullInstance) != 0 && (r.Flags & RegionFlags.PhysicsFull) == 0 ) {
				r.Flags |= RegionFlags.NeedsPhysics;
			}

			if ( wantsActive && (r.Flags & RegionFlags.HasFullInstance) != 0 && (r.Flags & RegionFlags.GameplayActive) == 0 ) {
				r.Flags |= RegionFlags.NeedsGameplay;
			}

			bool hasActiveRuntime = HasAny( r.Flags, RegionFlags.PhysicsFull | RegionFlags.GameplayActive | RegionFlags.LightingFull );
			if ( !wantsActive && hasActiveRuntime ) {
				r.Flags |= RegionFlags.NeedsDowngrade;
			}

			bool hasHotRuntime = HasAny( r.Flags, RegionFlags.VisualFull | RegionFlags.VisualProxy | RegionFlags.LightingNoShadows | RegionFlags.LightingFull );
			if ( !wantsHot && hasHotRuntime ) {
				r.Flags |= RegionFlags.NeedsDowngrade;
			}

			if ( !wantsHot && (r.Flags & RegionFlags.HasFullInstance) != 0 ) {
				r.Flags |= RegionFlags.NeedsEvict;
			}

			if ( !wantsWarm && HasAny( r.Flags, RegionFlags.HasFullRes | RegionFlags.SceneLoadRequested | RegionFlags.SceneLoadFailed ) ) {
				r.Flags |= RegionFlags.NeedsEvict;
			}

			// Preserve queue bits that were present before pending recalculation.
			r.Flags |= queueBits;

			EnqueueIfNeeded( ref r, index );
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
	};
};
