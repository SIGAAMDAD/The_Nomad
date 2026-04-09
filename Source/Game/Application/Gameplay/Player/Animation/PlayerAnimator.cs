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
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Prefabs;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	/*
	===================================================================================
	
	PlayerAnimator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal abstract class PlayerAnimator : NomadBehaviour {
		public Guid Id { get; set; }

		protected PlayerPrefab prefab;
		protected EngineAnimatedSprite2D animator;

		public abstract IGameEvent<PlayerAnimationStateChangedEventArgs> AnimationStateChanged { get; }

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

			prefab = Object.CastAs<PlayerPrefab>();

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerMovementChangedEventArgs>( $"{Id}:{EventNames.PLAYER_MOVEMENT_CHANGED}", EventNames.NAMESPACE )
				.Subscribe( OnPlayerMovementChanged );
		}

		/*
		===============
		OnUpdate
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void OnUpdate( float delta ) {
			base.OnUpdate( delta );

			var velocity = prefab.Velocity;
			if ( velocity.X > 0.0f ) {
				Flip( false );
			} else if ( velocity.X < 0.0f ) {
				Flip( true );
			}
		}

		/*
		===============
		Flip
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="flip"></param>
		private void Flip( bool flip ) {
			animator.FlipH = flip;
		}

		protected abstract void OnPlayerMovementChanged( in PlayerMovementChangedEventArgs args );
	};
};