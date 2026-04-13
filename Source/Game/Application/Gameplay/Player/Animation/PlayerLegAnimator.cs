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
using System.Numerics;
using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Player.Animation {
	/*
	===================================================================================
	
	PlayerLegAnimator

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerLegAnimator : PlayerAnimator {
		public override IGameEvent<PlayerAnimationStateChangedEventArgs> AnimationStateChanged => _animationStateChanged;
		private readonly IGameEvent<PlayerAnimationStateChangedEventArgs> _animationStateChanged;

		private PlayerAnimationState _state = PlayerAnimationState.Idle;
		private Vector2 _lastVelocity = Vector2.Zero;
		private bool _isBackpedaling = false;

		private Godot.GpuParticles2D _dustPuff;

		public PlayerLegAnimator() {
			var eventFactory = GameEventRegistry.Instance;

			_animationStateChanged = eventFactory.GetEvent<PlayerAnimationStateChangedEventArgs>( $"Leg:{Id}:{EventNames.PLAYER_ANIMATION_STATE_CHANGED}", EventNames.NAMESPACE );
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

			animator = prefab.FindChild<EngineAnimatedSprite2D>( "LegAnimator" );
			animator.AnimationLooped.Subscribe( OnLooped );

			_dustPuff = animator.GetNode<Godot.GpuParticles2D>( "DustPuff" );
		}

		/*
		===============
		OnLooped
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnLooped( in EmptyEventArgs args ) {
			if ( _state == PlayerAnimationState.Moving ) {
				_dustPuff.Emitting = true;
			}
		}

		/*
		===============
		OnPlayerMovementChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		protected override void OnPlayerMovementChanged( in PlayerMovementChangedEventArgs args ) {
			if ( !args.IsMoving ) {
				animator.Play( "idle" );
				_state = PlayerAnimationState.Idle;
				return;
			}

			_state = PlayerAnimationState.Moving;
			// are we backpedaling?
			if ( !args.WalkingReverse ) {
				// sudden stop?
				if ( HasSuddenStop( args.OldVelocity, args.NewVelocity ) ) {
					animator.Play( "sudden_stop" );
				} else {
					animator.Play( "move" );
				}
			} else {
				animator.PlayBackwards( "move" );
			}
		}

		private static bool HasSuddenStop( Vector2 oldVelocity, Vector2 newVelocity ) {
			const float epsilon = 0.01f;

			if ( MathF.Abs( oldVelocity.X ) < epsilon || MathF.Abs( newVelocity.X ) < epsilon ) {
				return false;
			}
			return MathF.Sign( oldVelocity.X ) != MathF.Sign( newVelocity.X );
		}
	};
};