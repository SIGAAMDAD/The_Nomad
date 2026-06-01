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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	/*
	===================================================================================

	PlayerAnimationController

	===================================================================================
	*/
	/// <summary>
	/// <para>Handles all higher level animation routing. Sprite directionality and state coordination.</para>
	/// </summary>

	internal sealed class PlayerAnimationController : IPlayerAnimationController
	{
		private static readonly Vector2 HEAD_OFFSET_LEFT = new Vector2( 5.0f, 5.0f );
		private static readonly Vector2 HEAD_OFFSET_RIGHT = new Vector2( 5.0f, -5.0f );

		public PlayerAnimationState Current => _activeState;
		private PlayerAnimationState _activeState = PlayerAnimationState.Idle;

		private readonly PlayerId _playerId = PlayerId.Invalid;

		private readonly AnimatedSprite2D _torsoAnimator;
		private readonly Sprite2D _headAnimator;
		private readonly AnimatedSprite2D _legAnimator;
		private readonly PlayerWalkEffects _walkEffects;

		private readonly AnimatedSprite2D _leftHandAnimator;
		private readonly AnimatedSprite2D _rightHandAnimator;

		private readonly IDisposable _stateChanged;
		private readonly IDisposable _aimAngleChanged;
		private readonly IDisposable _locomotionCue;

		private bool _isDisposed = false;

		public IGameEvent<PlayerAnimationStateChangedEventArgs> AnimationStateChanged => _animationStateChanged;
		private readonly IGameEvent<PlayerAnimationStateChangedEventArgs> _animationStateChanged = null;

		/*
		===============
		PlayerAnimationController
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="prefab"></param>
		/// <param name="movementController"></param>
		/// <param name="aimReader"></param>
		/// <param name="stateReader"></param>
		/// <param name="eventFactory"></param>
		public PlayerAnimationController(
			PlayerId playerId,
			PlayerPrefab prefab,
			IMovementController movementController,
			IAimReader aimReader,
			IPlayerStateReader stateReader,
			IGameEventRegistryService eventFactory
		)
		{
			ArgumentGuard.ThrowIfNull( prefab, nameof( prefab ) );
			ArgumentGuard.ThrowIfNull( movementController, nameof( movementController ) );
			ArgumentGuard.ThrowIfNull( aimReader, nameof( aimReader ) );
			ArgumentGuard.ThrowIfNull( stateReader, nameof( stateReader ) );
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_playerId = playerId;

			_headAnimator = prefab.GetNode<Sprite2D>( "Animations/HeadSprite" );

			_legAnimator = prefab.GetNode<AnimatedSprite2D>( "Animations/LegAnimator" );

			_torsoAnimator = prefab.GetNode<AnimatedSprite2D>( "Animations/TorsoAnimator" );
			_walkEffects = new PlayerWalkEffects( prefab, stateReader );

			_leftHandAnimator = prefab.GetNode<AnimatedSprite2D>( "Animations/LeftArm/AnimatedSprite2D" );
			_leftHandAnimator.SpriteFrames = ResourceLoader.Load<SpriteFrames>( "res://Assets/Animations/Player/LeftArmFrames.tres" );

			_rightHandAnimator = prefab.GetNode<AnimatedSprite2D>( "Animations/RightArm/AnimatedSprite2D" );
			_rightHandAnimator.SpriteFrames = ResourceLoader.Load<SpriteFrames>( "res://Assets/Animations/Player/RightArmFrames.tres" );

			_animationStateChanged = eventFactory.GetEvent<PlayerAnimationStateChangedEventArgs>(
				PlayerAnimationStateChangedEventArgs.Name,
				PlayerAnimationStateChangedEventArgs.NameSpace
			);

			_stateChanged = stateReader.StateChanged.Subscribe( OnStateChanged );
			_locomotionCue = movementController.LocomotionCue.Subscribe( OnLocomotionCue );
			_aimAngleChanged = aimReader.AimAngleChanged.Subscribe( OnAimAngleChanged );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_walkEffects.Dispose();
			_stateChanged.Dispose();
			_locomotionCue.Dispose();
			_aimAngleChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnAimAngleChanged( in AimAngleChangedEventArgs args )
		{
			bool flip = args.NewDirection.X < 0.0f;

			_headAnimator.FlipV = flip;
			_torsoAnimator.FlipH = flip;
			_legAnimator.FlipH = flip;
			_leftHandAnimator.FlipV = flip;
			_rightHandAnimator.FlipV = flip;

			_headAnimator.Rotation = args.NewAngle;
			_headAnimator.Offset = flip ? HEAD_OFFSET_LEFT : HEAD_OFFSET_RIGHT;

			_leftHandAnimator.Rotation = args.NewAngle;
			_rightHandAnimator.Rotation = args.NewAngle;

			if ( flip ) {
				_leftHandAnimator.ZIndex = 2;
				_rightHandAnimator.ZIndex = 0;
			} else {
				_leftHandAnimator.ZIndex = 0;
				_rightHandAnimator.ZIndex = 2;
			}
		}

		private void OnLocomotionCue( in PlayerLocomotionCueEventArgs args )
		{
			switch ( args.Cue ) {
				case PlayerLocomotionCue.HardStart:
					break;

				case PlayerLocomotionCue.HardStop:
					_legAnimator.Play( "sudden_stop" );
					break;

				case PlayerLocomotionCue.Reverse:
					_legAnimator.PlayBackwards( "move" );
					break;
			}
		}

		private void OnStateChanged( in PlayerStateChangedEventArgs args )
		{
			switch ( args.NewState ) {
				case PlayerStateId.Idle:
					SetAnimationStatus( PlayerAnimationState.Idle );
					break;

				case PlayerStateId.Moving:
					SetAnimationStatus( PlayerAnimationState.Running );
					break;

				case PlayerStateId.RestingAtCheckpoint:
					SetAnimationStatus( PlayerAnimationState.RestingAtCheckpoint );
					break;
			}
		}

		private void SetAnimationStatus( PlayerAnimationState state )
		{
			string animationName = state switch {
				PlayerAnimationState.Idle => "idle",
				PlayerAnimationState.Running => "move",
				PlayerAnimationState.RestingAtCheckpoint => "checkpoint_resting",
				PlayerAnimationState.TrueIdle => "true_idle",
				_ => throw new ArgumentOutOfRangeException( nameof( state ) )
			};

			if (
				state == PlayerAnimationState.RestingAtCheckpoint ||
				state == PlayerAnimationState.TrueIdle ||
				state == PlayerAnimationState.Dying
			) {
				_torsoAnimator.Play( animationName );

				_legAnimator.Hide();
				_leftHandAnimator.Hide();
				_rightHandAnimator.Hide();
				_headAnimator.Hide();

				return;
			}

			_legAnimator.Show();
			_leftHandAnimator.Show();
			_rightHandAnimator.Show();
			_headAnimator.Show();

			_torsoAnimator.Play( animationName );
			_legAnimator.Play( animationName );
			_leftHandAnimator.Play( animationName );
			_rightHandAnimator.Play( animationName );

			_activeState = state;
		}
	};
};
