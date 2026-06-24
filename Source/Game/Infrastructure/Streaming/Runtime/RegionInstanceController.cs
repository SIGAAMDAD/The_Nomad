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
using Nomad.Game.Application.Gameplay.Traversal;

namespace Nomad.Game.Infrastructure.Streaming
{
	internal sealed class RegionInstanceController
	{
		private readonly RegionGrid _grid;
		private readonly Node3D _regionSceneRoot;
		private readonly RegionStreamingSettings _settings;
		private readonly ITraversalDatabaseRegistry _traversalRegistry;

		public RegionInstanceController( RegionGrid grid, Node3D regionSceneRoot, RegionStreamingSettings settings, ITraversalDatabaseRegistry traversalRegistry = null )
		{
			_grid = grid ?? throw new ArgumentNullException( nameof( grid ) );
			_regionSceneRoot = regionSceneRoot ?? throw new ArgumentNullException( nameof( regionSceneRoot ) );
			_settings = settings ?? throw new ArgumentNullException( nameof( settings ) );
			_traversalRegistry = traversalRegistry;
		}

		public StreamedRegionChunk Instantiate( PackedScene scene, RegionManifest manifest )
		{
			StreamedRegionChunk instance = scene.Instantiate<StreamedRegionChunk>();
			RegionId id = manifest.Id;

			instance.Initialize( id );
			instance.SetTraversalRegistry( _traversalRegistry );
			instance.SetTraversalDatabasePath( manifest.TraversalDatabasePath );
			instance.Name = $"Region_{id.X}_{id.Z}";
			instance.Position = _grid.RegionOrigin( id );

			_regionSceneRoot.AddChild( instance );

			instance.SetVisualEnabled( false );
			instance.SetPhysicsEnabled( false );
			instance.SetGameplayEnabled( false );
			instance.SetLightingEnabled( false );
			instance.SetLightingShadowsEnabled( false );
			instance.SetAudioEnabled( false );

			return instance;
		}

		public void EnableVisual( StreamedRegionChunk instance )
		{
			instance.SetVisualEnabled( true );
		}

		public void EnableHotLighting( StreamedRegionChunk instance, RegionManifest manifest )
		{
			if ( !_settings.EnableHotLighting || !manifest.EnableHotLighting ) {
				return;
			}

			instance.SetLightingEnabled( true );
			instance.SetLightingShadowsEnabled( false );
		}

		public void EnableActiveLighting( StreamedRegionChunk instance )
		{
			instance.SetLightingEnabled( true );
			instance.SetLightingShadowsEnabled( _settings.EnableActiveShadows );
		}

		public void EnableHotAudio( StreamedRegionChunk instance, RegionManifest manifest )
		{
			if ( _settings.EnableHotAudio && manifest.EnableHotAudio ) {
				instance.SetAudioEnabled( true );
			}
		}

		public void EnablePhysics( StreamedRegionChunk instance )
		{
			instance.SetPhysicsEnabled( true );
		}

		public void EnableGameplay( StreamedRegionChunk instance )
		{
			instance.SetGameplayEnabled( true );
		}

		public void DowngradeToHot( StreamedRegionChunk instance )
		{
			instance.SetGameplayEnabled( false );
			instance.SetPhysicsEnabled( false );
			instance.SetLightingShadowsEnabled( false );
		}

		public void DowngradeBelowHot( StreamedRegionChunk instance )
		{
			instance.SetGameplayEnabled( false );
			instance.SetPhysicsEnabled( false );
			instance.SetLightingShadowsEnabled( false );
			instance.SetLightingEnabled( false );
			instance.SetVisualEnabled( false );
			instance.SetAudioEnabled( false );
		}

		public void Free( StreamedRegionChunk instance )
		{
			if ( GodotObject.IsInstanceValid( instance ) ) {
				instance.QueueFree();
			}
		}
	};
};
