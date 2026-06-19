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
using Nomad.Game.Sdk.Events.Player.Movement;

namespace Nomad.Game.Infrastructure.Streaming
{
	internal sealed class WorldChunkStreamer
	{
		public bool HasUsableState => _focus.HasUsableState;
		public RegionGrid Grid => _grid;
		public RegionStateTable State => _state;

		private readonly Node3D _worldBase;
		private readonly RegionStreamingSettings _settings;
		private readonly RegionGrid _grid;
		private readonly RegionStateTable _state;
		private readonly StreamFocusState _focus;
		private readonly RegionPredictor _predictor;
		private readonly RegionTargetPlanner _targetPlanner;
		private readonly RegionTransitionPlanner _transitionPlanner;
		private readonly RegionOperationExecutor _executor;
		private readonly RegionOperationQueue _promotionQueue;
		private readonly RegionOperationQueue _demotionQueue;

		private ushort _streamTick;

		public WorldChunkStreamer( Node3D worldBase )
			: this( worldBase, worldBase, new RegionStreamingSettings() )
		{
		}

		public WorldChunkStreamer( Node3D worldBase, Node3D regionSceneRoot )
			: this( worldBase, regionSceneRoot, new RegionStreamingSettings() )
		{
		}

		public WorldChunkStreamer( Node3D worldBase, Node3D regionSceneRoot, RegionStreamingSettings settings )
		{
			_worldBase = worldBase ?? throw new ArgumentNullException( nameof( worldBase ) );
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
			_settings.Validate();

			_grid = new RegionGrid( _settings );
			_state = new RegionStateTable();
			_state.Initialize( _grid );

			_focus = new StreamFocusState();
			_predictor = new RegionPredictor( _grid, _settings );
			_targetPlanner = new RegionTargetPlanner( _settings );

			// Capacity intentionally exceeds 1024 so a full queue is distinguishable from empty.
			_promotionQueue = new RegionOperationQueue( RegionGrid.MAX_REGIONS * 2 );
			_demotionQueue = new RegionOperationQueue( RegionGrid.MAX_REGIONS * 2 );

			_transitionPlanner = new RegionTransitionPlanner( _promotionQueue, _demotionQueue );

			var loader = new RegionSceneLoader();
			var instanceController = new RegionInstanceController( _grid, regionSceneRoot ?? worldBase, _settings );
			_executor = new RegionOperationExecutor(
				_grid,
				_settings,
				_promotionQueue,
				_demotionQueue,
				_transitionPlanner,
				loader,
				instanceController
			);
		}

		public void HandlePlayerMovementChanged( in PlayerLocomotionCueEventArgs args )
		{
			_focus.Apply( args );
		}

		public void HandlePlayerCameraStatusChanged( in PlayerCameraStatusChangedEventArgs args )
		{
			_focus.Apply( args );
		}

		public void SetFocusDirect( Vector3 origin, Vector2 velocity, Vector2 moveInput, Vector3 cameraForward )
		{
			_focus.ApplyDirect( origin, velocity, moveInput, cameraForward );
		}

		public void SetManifest( RegionManifest manifest )
		{
			_state.SetManifest( _grid, manifest );
		}

		public void SetManifests( ReadOnlySpan<RegionManifest> manifests )
		{
			for ( int i = 0; i < manifests.Length; i++ ) {
				SetManifest( manifests[i] );
			}
		}

		public void LoadManifestTable( RegionManifestTable table )
		{
			if ( table == null ) {
				return;
			}

			foreach ( RegionManifestResource resource in table.Regions ) {
				if ( resource == null ) {
					continue;
				}

				SetManifest( resource.ToManifest() );
			}
		}

		public void LoadManifestTable( string resourcePath )
		{
			if ( string.IsNullOrEmpty( resourcePath ) ) {
				return;
			}

			RegionManifestTable table = ResourceLoader.Load<RegionManifestTable>( resourcePath );
			if ( table == null ) {
				GD.PushWarning( $"Failed to load region manifest table: {resourcePath}" );
				return;
			}

			LoadManifestTable( table );
		}

		public void TickLatest()
		{
			TickLatest( _settings.DefaultPromotionBudget, _settings.DefaultDemotionBudget );
		}

		public void TickLatest( int promotionBudget, int demotionBudget )
		{
			if ( !_focus.HasUsableState ) {
				return;
			}

			_streamTick++;

			RegionPrediction prediction = _predictor.Build( _focus );

			_targetPlanner.RefreshTargets( prediction, _state );
			_transitionPlanner.RefreshAll( _state );

			_executor.PumpDemotions( _state, Math.Max( 0, demotionBudget ) );
			_executor.PumpPromotions( _state, Math.Max( 0, promotionBudget ) );

			MarkTouched( prediction );
		}

		public void Shutdown()
		{
			for ( int i = 0; i < _state.Instances.Length; i++ ) {
				StreamedRegionChunk instance = _state.Instances[i];
				if ( instance != null && GodotObject.IsInstanceValid( instance ) ) {
					instance.QueueFree();
				}
			}

			_promotionQueue.Clear();
			_demotionQueue.Clear();
			_state.ClearRuntimeState();
		}

		public RegionStatus GetStatus( RegionId id )
		{
			if ( !_grid.TryToIndex( id, out int index ) ) {
				return RegionStatus.Unloaded;
			}

			RegionFlags flags = _state.Regions[index].Flags;

			if ( (flags & RegionFlags.SceneLoadFailed) != 0 ) {
				return RegionStatus.Failed;
			}

			if ( (flags & RegionFlags.GameplayActive) != 0 && (flags & RegionFlags.PhysicsFull) != 0 ) {
				return RegionStatus.Active;
			}

			if ( (flags & RegionFlags.VisualFull) != 0 && (flags & RegionFlags.HasFullInstance) != 0 ) {
				return RegionStatus.Hot;
			}

			if ( (flags & RegionFlags.HasFullRes) != 0 ) {
				return RegionStatus.Warm;
			}

			if ( (flags & RegionFlags.SceneLoadRequested) != 0 ) {
				return RegionStatus.Warming;
			}

			if ( (flags & RegionFlags.HasManifest) != 0 ) {
				return RegionStatus.Cold;
			}

			return RegionStatus.Unloaded;
		}

		private void MarkTouched( RegionPrediction prediction )
		{
			if ( !_grid.TryToIndex( prediction.CurrentRegion, out int index ) ) {
				return;
			}

			_state.Regions[index].LastTouchedTick = _streamTick;
			_state.Regions[index].LastUsedTick = _streamTick;
		}
	};
};
