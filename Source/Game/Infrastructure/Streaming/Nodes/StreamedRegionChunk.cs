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
using Nomad.Game.Prefabs;

namespace Nomad.Game.Infrastructure.Streaming
{
	internal sealed partial class StreamedRegionChunk : Node3D
	{
		[Export]
		private Node3D _visualRoot;

		[Export]
		private Node3D _physicsRoot;

		[Export]
		private Node3D _gameplayRoot;

		[Export]
		private Node3D _lightingRoot;

		[Export]
		private Node3D _audioRoot;

		[Export]
		private TraversalDatabase _traversalDatabase;

		[Export]
		private string _traversalDatabasePath = string.Empty;

		private ITraversalDatabaseRegistry _traversalRegistry;
		private int _traversalRegistrationId = -1;

		private CollisionShape3D[] _shapes = Array.Empty<CollisionShape3D>();
		private Light3D[] _lights = Array.Empty<Light3D>();
		private AudioStreamPlayer3D[] _audioPlayers = Array.Empty<AudioStreamPlayer3D>();

		public RegionId RegionId { get; private set; }

		public void Initialize( RegionId id )
		{
			RegionId = id;
		}

		public void SetTraversalRegistry( ITraversalDatabaseRegistry registry )
		{
			_traversalRegistry = registry;
		}

		public void SetTraversalDatabasePath( string path )
		{
			_traversalDatabasePath = path ?? string.Empty;
		}

		public override void _Ready()
		{
			base._Ready();

			_shapes = Collect<CollisionShape3D>( _physicsRoot );
			_lights = Collect<Light3D>( _lightingRoot );
			_audioPlayers = Collect<AudioStreamPlayer3D>( _audioRoot );

			SetVisualEnabled( false );
			SetPhysicsEnabled( false );
			SetGameplayEnabled( false );
			SetLightingEnabled( false );
			SetLightingShadowsEnabled( false );
			SetAudioEnabled( false );
		}

		public void SetVisualEnabled( bool enabled )
		{
			_visualRoot?.Visible = enabled;
		}

		public void SetLightingEnabled( bool enabled )
		{
			_lightingRoot?.Visible = enabled;
		}

		public void SetLightingShadowsEnabled( bool enabled )
		{
			foreach ( Light3D light in _lights ) {
				if ( GodotObject.IsInstanceValid( light ) ) {
					light.ShadowEnabled = enabled;
				}
			}
		}

		public void SetPhysicsEnabled( bool enabled )
		{
			foreach ( CollisionShape3D shape in _shapes ) {
				if ( GodotObject.IsInstanceValid( shape ) ) {
					shape.SetDeferred( CollisionShape3D.PropertyName.Disabled, !enabled );
				}
			}
		}

		public void SetGameplayEnabled( bool enabled )
		{
			if ( enabled ) {
				RegisterTraversalDatabase();
			} else {
				UnregisterTraversalDatabase();
			}

			if ( _gameplayRoot == null ) {
				return;
			}

			_gameplayRoot.ProcessMode = enabled
				? ProcessModeEnum.Inherit
				: ProcessModeEnum.Disabled;
		}

		public void SetAudioEnabled( bool enabled )
		{
			_audioRoot?.ProcessMode = enabled
				? ProcessModeEnum.Inherit
				: ProcessModeEnum.Disabled;

			foreach ( AudioStreamPlayer3D player in _audioPlayers ) {
				if ( !GodotObject.IsInstanceValid( player ) ) {
					continue;
				}

				if ( enabled ) {
					if ( player.Stream != null && !player.Playing ) {
						player.Play();
					}
				} else if ( player.Playing ) {
					player.Stop();
				}
			}
		}

		private void RegisterTraversalDatabase()
		{
			if ( _traversalRegistrationId >= 0 || _traversalRegistry == null ) {
				return;
			}

			TraversalDatabase database = ResolveTraversalDatabase();
			if ( database == null ) {
				return;
			}

			_traversalRegistrationId = _traversalRegistry.Register( RegionId, database );
		}

		private void UnregisterTraversalDatabase()
		{
			if ( _traversalRegistrationId < 0 || _traversalRegistry == null ) {
				_traversalRegistrationId = -1;
				return;
			}

			_traversalRegistry.Unregister( _traversalRegistrationId );
			_traversalRegistrationId = -1;
		}

		private TraversalDatabase ResolveTraversalDatabase()
		{
			if ( _traversalDatabase != null ) {
				return _traversalDatabase;
			}

			if ( string.IsNullOrEmpty( _traversalDatabasePath ) || !ResourceLoader.Exists( _traversalDatabasePath ) ) {
				return null;
			}

			_traversalDatabase = ResourceLoader.Load<TraversalDatabase>( _traversalDatabasePath );
			return _traversalDatabase;
		}

		private static T[] Collect<T>( Node root ) where T : Node
		{
			if ( root == null ) {
				return Array.Empty<T>();
			}

			global::Godot.Collections.Array<Node> nodes = root.FindChildren( "*", typeof( T ).Name, true, false );
			T[] result = new T[nodes.Count];

			for ( int i = 0; i < nodes.Count; i++ ) {
				result[i] = (T)nodes[i];
			}

			return result;
		}
	};
};
