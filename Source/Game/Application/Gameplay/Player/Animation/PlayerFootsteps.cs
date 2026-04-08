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

using System.Collections.Generic;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.EngineUtils;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Prefabs;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	/*
	===================================================================================
	
	PlayerFootsteps
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerFootsteps : NomadBehaviour {
		private const int MAX_STEPS = 24;
		private const int DEQUEUE_LIMIT = 3;

		private PlayerPrefab _prefab;

		private readonly Queue<Transform2D> _steps = new( MAX_STEPS );
		private MultiMeshInstance2D _mesh;

		private bool _isMoving = false;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void OnInit() {
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();
			
			// TODO: use framework to create this
			var texture = ResourceLoader.Load<Texture2D>( "res://Assets/Textures/Effects/footstep.png" );
			_mesh = new MultiMeshInstance2D() {
				Name = nameof( PlayerFootsteps ),
				Texture = texture,
				Modulate = new Color( 1.0f, 1.0f, 1.0f, 0.75f ),
				Multimesh = new MultiMesh() {
					Mesh = new QuadMesh() {
						Size = new Vector2( 16.0f, -8.0f )
					},
					VisibleInstanceCount = 0,
					InstanceCount = MAX_STEPS,
				}
			};
			
			// attach the mesh to a detached transform otherwise we'll have the transforms following the player.
			var sceneObject = new Node();
			sceneObject.AddChild( _mesh );
			_prefab.AddChild( sceneObject );

			var legAnimation = _prefab.FindChild<EngineAnimatedSprite2D>( "LegAnimator" );
			legAnimation.AnimationLooped.Subscribe( OnLegAnimationLooped );

			var legAnimator = _prefab.GetComponent<PlayerLegAnimator>();
			legAnimator.AnimationStateChanged.Subscribe( OnAnimationStateChanged );
		}

		private void OnAnimationStateChanged( in PlayerAnimationStateChangedEventArgs args ) {
			_isMoving = args.NewAnimationId == new InternString( "move" );
		}

		/*
		===============
		OnLegAnimationLooped
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnLegAnimationLooped( in EmptyEventArgs args ) {
			if ( !_isMoving ) {
				return;
			}
			var position = _prefab.GlobalPosition;
			Transform2D transform = new Transform2D( 0.0f, new Vector2( position.X, position.Y + 24.0f ) );
			CheckCapacity();
			_mesh.Multimesh.VisibleInstanceCount++;
			_mesh.Multimesh.SetInstanceTransform2D( _steps.Count, transform );
			_steps.Enqueue( transform );
		}

		/*
		===============
		CheckCapacity
		===============
		*/
		/// <summary>
		/// Checks the current number of allocated footsteps and removes 3 of the oldest from the queue if we're overflowing with feet.
		/// </summary>
		private void CheckCapacity() {
			if ( _steps.Count < MAX_STEPS ) {
				return;
			}

			for ( int i = 0; i < DEQUEUE_LIMIT; i++ ) {
				_steps.Dequeue();
			}

			int instance = 0;
			foreach ( var step in _steps ) {
				_mesh.Multimesh.SetInstanceTransform2D( instance++, step );
			}
			_mesh.Multimesh.VisibleInstanceCount--;
		}
	};
};