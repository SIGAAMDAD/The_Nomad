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

using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	internal sealed class PlayerLegAnimator : PlayerAnimator {
		private EngineAnimatedSprite2D _animator;

		public override IGameEvent<PlayerAnimationStateChangedEventArgs> AnimationStateChanged => _animationStateChanged;
		private readonly IGameEvent<PlayerAnimationStateChangedEventArgs> _animationStateChanged;

		public PlayerLegAnimator() {
			var eventFactory = GameEventRegistry.Instance;

			_animationStateChanged = _animationStateChanged = eventFactory.GetEvent<PlayerAnimationStateChangedEventArgs>( $"Leg:{EventNames.PLAYER_ANIMATION_STATE_CHANGED}", EventNames.NAMESPACE );
		}

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

			_animator = prefab.FindChild<EngineAnimatedSprite2D>( "LegAnimator" );
		}

		protected override void Flip( bool flip ) {
			_animator.FlipH = flip;
		}

		/*
		===============
		PlayerStartMoving
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		protected override void OnPlayerStartMoving( in PlayerStartMovingEventArgs args ) {
			_animator.Play( "move" );

			// are we backpedaling?
			if ( ( _animator.FlipH && args.Velocity.X > 0.0f ) || ( !_animator.FlipH && args.Velocity.X < 0.0f ) ) {
				// sudden stop?
				if ( ( args.Velocity.X < 0.0f && prefab.Velocity.X > 0.0f ) || ( args.Velocity.X > 0.0f && prefab.Velocity.X < 0.0f ) ) {
					_animator.Play( "sudden_stop" );
				} else {
					_animator.Play( "move" );
				}
			} else {
				_animator.PlayBackwards( "move" );
			}
		}

		protected override void OnPlayerStopMoving( in EmptyEventArgs args ) {
			_animator.Play( "idle" );
		}
	};
};