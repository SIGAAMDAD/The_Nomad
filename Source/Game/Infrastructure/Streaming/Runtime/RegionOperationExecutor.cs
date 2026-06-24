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

namespace Nomad.Game.Infrastructure.Streaming
{
	internal sealed class RegionOperationExecutor
	{
		private readonly RegionGrid _grid;
		private readonly RegionStreamingSettings _settings;
		private readonly RegionOperationQueue _promotionQueue;
		private readonly RegionOperationQueue _demotionQueue;
		private readonly RegionTransitionPlanner _transitionPlanner;
		private readonly RegionSceneLoader _loader;
		private readonly RegionInstanceController _instances;

		public RegionOperationExecutor(
			RegionGrid grid,
			RegionStreamingSettings settings,
			RegionOperationQueue promotionQueue,
			RegionOperationQueue demotionQueue,
			RegionTransitionPlanner transitionPlanner,
			RegionSceneLoader loader,
			RegionInstanceController instances
		)
		{
			_grid = grid ?? throw new ArgumentNullException( nameof( grid ) );
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
			_promotionQueue = promotionQueue ?? throw new ArgumentNullException( nameof( promotionQueue ) );
			_demotionQueue = demotionQueue ?? throw new ArgumentNullException( nameof( demotionQueue ) );
			_transitionPlanner = transitionPlanner ?? throw new ArgumentNullException( nameof( transitionPlanner ) );
			_loader = loader ?? throw new ArgumentNullException( nameof( loader ) );
			_instances = instances ?? throw new ArgumentNullException( nameof( instances ) );
		}

		public int PumpPromotions( RegionStateTable state, int budget )
		{
			int ops = 0;

			while ( ops < budget && _promotionQueue.TryDequeue( out ushort index ) ) {
				ref RegionRecord r = ref state.Regions[index];
				r.Flags &= ~RegionFlags.QueuedPromotion;

				if ( ExecutePromotionStep( state, index ) ) {
					ops++;
				}

				_transitionPlanner.RefreshRegion( state, index );
			}

			return ops;
		}

		public int PumpDemotions( RegionStateTable state, int budget )
		{
			int ops = 0;

			while ( ops < budget && _demotionQueue.TryDequeue( out ushort index ) ) {
				ref RegionRecord r = ref state.Regions[index];
				r.Flags &= ~RegionFlags.QueuedDemotion;

				if ( ExecuteDemotionStep( state, index ) ) {
					ops++;
				}

				_transitionPlanner.RefreshRegion( state, index );
			}

			return ops;
		}

		private bool ExecutePromotionStep( RegionStateTable state, int index )
		{
			ref RegionRecord r = ref state.Regions[index];

			if ( (r.Flags & RegionFlags.NeedsLoad) != 0 ) {
				LoadNeededResource( state, index, ref r );
				return true;
			}

			if ( (r.Flags & RegionFlags.NeedsInstantiate) != 0 ) {
				InstantiateRegion( state, index, ref r );
				return true;
			}

			if ( (r.Flags & RegionFlags.NeedsVisual) != 0 ) {
				EnableVisual( state, index, ref r );
				return true;
			}

			if ( (r.Flags & RegionFlags.NeedsLight) != 0 ) {
				EnableLighting( state, index, ref r );
				return true;
			}

			if ( (r.Flags & RegionFlags.NeedsPhysics) != 0 ) {
				EnablePhysics( state, index, ref r );
				return true;
			}

			if ( (r.Flags & RegionFlags.NeedsGameplay) != 0 ) {
				EnableGameplay( state, index, ref r );
				return true;
			}

			return false;
		}

		private bool ExecuteDemotionStep( RegionStateTable state, int index )
		{
			ref RegionRecord r = ref state.Regions[index];

			if ( (r.Flags & RegionFlags.NeedsDowngrade) != 0 ) {
				Downgrade( state, index, ref r );
				return true;
			}

			if ( (r.Flags & RegionFlags.NeedsEvict) != 0 ) {
				Evict( state, index, ref r );
				return true;
			}

			return false;
		}

		private void LoadNeededResource( RegionStateTable state, int index, ref RegionRecord r )
		{
			RegionManifest manifest = state.Manifests[index];

			if ( !manifest.HasScene ) {
				r.Flags &= ~RegionFlags.NeedsLoad;
				return;
			}

			if ( state.SceneCache[index] != null ) {
				r.Flags |= RegionFlags.HasFullRes;
				r.Flags &= ~(RegionFlags.SceneLoadRequested | RegionFlags.SceneLoadFailed | RegionFlags.NeedsLoad);
				return;
			}

			RegionSceneLoadResult result = _loader.RequestOrPoll( manifest.ScenePath, out PackedScene scene );

			switch ( result ) {
				case RegionSceneLoadResult.Requested:
				case RegionSceneLoadResult.Pending:
					r.Flags |= RegionFlags.SceneLoadRequested;
					r.Flags &= ~RegionFlags.NeedsLoad;
					break;

				case RegionSceneLoadResult.Loaded:
					state.SceneCache[index] = scene;
					r.Flags |= RegionFlags.HasFullRes;
					r.Flags &= ~(RegionFlags.SceneLoadRequested | RegionFlags.SceneLoadFailed | RegionFlags.NeedsLoad);
					break;

				case RegionSceneLoadResult.Failed:
					r.Flags |= RegionFlags.SceneLoadFailed;
					r.Flags &= ~(RegionFlags.SceneLoadRequested | RegionFlags.NeedsLoad);
					break;
			}
		}

		private void InstantiateRegion( RegionStateTable state, int index, ref RegionRecord r )
		{
			PackedScene scene = state.SceneCache[index];
			if ( scene == null ) {
				r.Flags &= ~RegionFlags.NeedsInstantiate;
				r.Flags |= RegionFlags.NeedsLoad;
				return;
			}

			if ( state.Instances[index] != null && GodotObject.IsInstanceValid( state.Instances[index] ) ) {
				r.Flags |= RegionFlags.HasFullInstance;
				r.Flags &= ~RegionFlags.NeedsInstantiate;
				return;
			}

			StreamedRegionChunk instance = _instances.Instantiate( scene, state.Manifests[index] );
			state.Instances[index] = instance;

			r.Flags |= RegionFlags.HasFullInstance | RegionFlags.PhysicsOff | RegionFlags.IsResident;
			r.Flags &= ~RegionFlags.NeedsInstantiate;
		}

		private void EnableVisual( RegionStateTable state, int index, ref RegionRecord r )
		{
			StreamedRegionChunk instance = state.Instances[index];
			if ( !IsUsable( instance ) ) {
				r.Flags &= ~RegionFlags.HasFullInstance;
				r.Flags |= RegionFlags.NeedsInstantiate;
				return;
			}

			_instances.EnableVisual( instance );
			_instances.EnableHotAudio( instance, state.Manifests[index] );

			r.Flags |= RegionFlags.VisualFull;
			r.Flags &= ~RegionFlags.NeedsVisual;
		}

		private void EnableLighting( RegionStateTable state, int index, ref RegionRecord r )
		{
			StreamedRegionChunk instance = state.Instances[index];
			if ( !IsUsable( instance ) ) {
				r.Flags &= ~RegionFlags.HasFullInstance;
				r.Flags |= RegionFlags.NeedsInstantiate;
				return;
			}

			RegionFlags target = r.Flags & RegionFlags.TargetMask;

			if ( (target & RegionFlags.TargetActive) != 0 ) {
				_instances.EnableActiveLighting( instance );
				r.Flags |= RegionFlags.LightingFull;
				r.Flags &= ~RegionFlags.LightingNoShadows;
			} else {
				_instances.EnableHotLighting( instance, state.Manifests[index] );
				r.Flags |= RegionFlags.LightingNoShadows;
				r.Flags &= ~RegionFlags.LightingFull;
			}

			r.Flags &= ~RegionFlags.NeedsLight;
		}

		private void EnablePhysics( RegionStateTable state, int index, ref RegionRecord r )
		{
			StreamedRegionChunk instance = state.Instances[index];
			if ( !IsUsable( instance ) ) {
				r.Flags &= ~RegionFlags.HasFullInstance;
				r.Flags |= RegionFlags.NeedsInstantiate;
				return;
			}

			_instances.EnablePhysics( instance );

			r.Flags |= RegionFlags.PhysicsFull;
			r.Flags &= ~(RegionFlags.PhysicsOff | RegionFlags.NeedsPhysics);
		}

		private void EnableGameplay( RegionStateTable state, int index, ref RegionRecord r )
		{
			StreamedRegionChunk instance = state.Instances[index];
			if ( !IsUsable( instance ) ) {
				r.Flags &= ~RegionFlags.HasFullInstance;
				r.Flags |= RegionFlags.NeedsInstantiate;
				return;
			}

			_instances.EnableGameplay( instance );

			r.Flags |= RegionFlags.GameplayActive;
			r.Flags &= ~RegionFlags.NeedsGameplay;
		}

		private void Downgrade( RegionStateTable state, int index, ref RegionRecord r )
		{
			StreamedRegionChunk instance = state.Instances[index];
			RegionFlags target = r.Flags & RegionFlags.TargetMask;
			bool wantsHot = (target & (RegionFlags.TargetHot | RegionFlags.TargetActive)) != 0;

			if ( IsUsable( instance ) ) {
				if ( wantsHot ) {
					_instances.DowngradeToHot( instance );
					r.Flags &= ~(RegionFlags.GameplayActive | RegionFlags.PhysicsFull | RegionFlags.LightingFull);
					r.Flags |= RegionFlags.PhysicsOff | RegionFlags.LightingNoShadows;
				} else {
					_instances.DowngradeBelowHot( instance );
					r.Flags &= ~(
						RegionFlags.GameplayActive |
						RegionFlags.PhysicsFull |
						RegionFlags.VisualFull |
						RegionFlags.VisualProxy |
						RegionFlags.LightingFull |
						RegionFlags.LightingNoShadows |
						RegionFlags.LightingStaticOnly
					);
					r.Flags |= RegionFlags.PhysicsOff;
				}
			} else {
				r.Flags &= ~(
					RegionFlags.HasFullInstance |
					RegionFlags.GameplayActive |
					RegionFlags.PhysicsFull |
					RegionFlags.VisualFull |
					RegionFlags.VisualProxy |
					RegionFlags.LightingFull |
					RegionFlags.LightingNoShadows |
					RegionFlags.LightingStaticOnly
				);
			}

			r.Flags &= ~RegionFlags.NeedsDowngrade;
		}

		private void Evict( RegionStateTable state, int index, ref RegionRecord r )
		{
			RegionFlags target = r.Flags & RegionFlags.TargetMask;
			bool wantsWarm = (target & (RegionFlags.TargetWarm | RegionFlags.TargetHot | RegionFlags.TargetActive)) != 0;

			StreamedRegionChunk instance = state.Instances[index];
			if ( IsUsable( instance ) ) {
				_instances.DowngradeBelowHot( instance );
				_instances.Free( instance );
			}

			state.Instances[index] = null;

			r.Flags &= ~(
				RegionFlags.HasFullInstance |
				RegionFlags.VisualFull |
				RegionFlags.VisualProxy |
				RegionFlags.VisualImposter |
				RegionFlags.PhysicsFull |
				RegionFlags.PhysicsOff |
				RegionFlags.LightingFull |
				RegionFlags.LightingNoShadows |
				RegionFlags.LightingStaticOnly |
				RegionFlags.GameplayActive |
				RegionFlags.IsResident
			);

			if ( !wantsWarm ) {
				state.SceneCache[index] = null;
				r.Flags &= ~(RegionFlags.HasFullRes | RegionFlags.SceneLoadRequested | RegionFlags.SceneLoadFailed);
			}

			r.Flags &= ~RegionFlags.NeedsEvict;
		}

		private static bool IsUsable( StreamedRegionChunk instance )
		{
			return instance != null && GodotObject.IsInstanceValid( instance );
		}
	};
};
