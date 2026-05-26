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
using System.Collections.Generic;
using Godot;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Prefabs;
using Nomad.Game.Application.Gameplay.Player;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	/*
	===================================================================================

	PlayerWalkEffects

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerWalkEffects : IDisposable
	{
		private const int MAX_STEPS = 24;
		private const int DEQUEUE_LIMIT = 3;

		private readonly PlayerPrefab _prefab;
		private readonly IPlayerStateReader _stateReader;

		//
		// Footsteps
		//

		private readonly Queue<Transform2D> _steps = new Queue<Transform2D>( MAX_STEPS );
		private readonly MultiMeshInstance2D _mesh;

		//
		// Independent Engine vfx
		//

		private readonly GpuParticles2D _dustPuff;

		private bool _isDisposed = false;

		/*
		===============
		PlayerWalkEffects
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="prefab"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PlayerWalkEffects( PlayerPrefab prefab, IPlayerStateReader stateReader )
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_stateReader = stateReader ?? throw new ArgumentNullException( nameof( stateReader ) );

			_dustPuff = prefab.GetNode<GpuParticles2D>( "Animations/LegAnimator/DustPuff" );

			// TODO: use framework to create this
			var texture = ResourceLoader.Load<Texture2D>( "res://Assets/Textures/Environment/footstep.png" );
			_mesh = new MultiMeshInstance2D() {
				Name = nameof( PlayerWalkEffects ),
				Texture = texture,
				Modulate = new Color( 1.0f, 1.0f, 1.0f, 0.75f ),
				Multimesh = new MultiMesh() {
					Mesh = new QuadMesh() {
						Size = new Vector2( 10.0f, -4.0f )
					},
					VisibleInstanceCount = 0,
					InstanceCount = MAX_STEPS,
				}
			};

			var legAnimation = _prefab.GetNode<AnimatedSprite2D>( "Animations/LegAnimator" );
			legAnimation.AnimationLooped += OnLegAnimationLooped;

			// attach the mesh to a detached transform otherwise we'll have the transforms following the player.
			var sceneObject = new Node();
			sceneObject.AddChild( _mesh );
			_prefab.AddChild( sceneObject );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			var legAnimation = _prefab.GetNode<AnimatedSprite2D>( "Animations/LegAnimator" );
			legAnimation.AnimationLooped -= OnLegAnimationLooped;

			_prefab.RemoveChild( _mesh.GetParent() );
			_mesh.QueueFree();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnLegAnimationLooped
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnLegAnimationLooped()
		{
			if ( _stateReader.Current != PlayerStateId.Moving ) {
				return;
			}

			_dustPuff.Emitting = true;

			Vector2 position = _prefab.GlobalPosition;
			Transform2D transform = new Transform2D(
				rotation: 0.0f,
				origin: new Vector2( position.X, position.Y + 24.0f )
			);

			CheckCapacity();

			_mesh.Multimesh.SetInstanceTransform2D( _mesh.Multimesh.VisibleInstanceCount++, transform );
			_steps.Enqueue( transform );
		}

		/*
		===============
		CheckCapacity
		===============
		*/
		/// <summary>
		/// Checks the current number of allocated footsteps and removes 3 of the oldest from the queue
		/// if we're overflowing with feet.
		/// </summary>
		private void CheckCapacity()
		{
			if ( _steps.Count < MAX_STEPS ) {
				return;
			}

			for ( int i = 0; i < DEQUEUE_LIMIT; i++ ) {
				_steps.Dequeue();
				_mesh.Multimesh.VisibleInstanceCount--;
			}

			int instance = 0;
			foreach ( var step in _steps ) {
				_mesh.Multimesh.SetInstanceTransform2D( instance++, step );
			}
		}
	};
};
