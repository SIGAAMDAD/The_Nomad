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
using Nomad.Game.Sdk.Player.Animation;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Events.Player.Movement;

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
		private static readonly StringName IdleAnimationName = "Idle";
		private static readonly StringName RunAnimationName = "Run";
		private static readonly StringName BackpedalAnimationName = "Backpedal";
		private static readonly StringName StrafeLeftRunAnimationName = "StrafeLeftRun";
		private static readonly StringName StrafeRightRunAnimationName = "StrafeRightRun";
		private static readonly StringName Running180AnimationName = "Running180";
		private static readonly StringName SuddenStopAnimationName = "SuddenStop";

		private static readonly StringName LandSoftAnimationName = "LandSoft";
		private static readonly StringName LandHardAnimationName = "LandHard";
		private static readonly StringName JumpAnimationName = "Jump";

		private static readonly StringName TrueIdleAnimationName = "true_idle";
		private static readonly StringName RestingAtCheckpointAnimationName = "checkpoint_resting";

		private static readonly Vector2 HEAD_OFFSET_LEFT = new Vector2( 5.0f, 5.0f );
		private static readonly Vector2 HEAD_OFFSET_RIGHT = new Vector2( 5.0f, -5.0f );

		public PlayerAnimationState Current => _activeState;
		private PlayerAnimationState _activeState = PlayerAnimationState.Idle;

		private readonly PlayerId _playerId = PlayerId.Invalid;

		private readonly AnimationNodeStateMachinePlayback _playback;
		private readonly AnimationNodeStateMachine _stateMachine;
		private StringName _activeLocomotionAnimation = IdleAnimationName;

		private readonly IDisposable _stateChanged;
		private readonly IDisposable _locomotionCue;
		private readonly IDisposable _directionalLocomotion;

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
			IPlayerMovementController movementController,
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

			var animationTree = prefab.GetNode<AnimationTree>( "AnimationTree" );
			_stateMachine = animationTree.TreeRoot as AnimationNodeStateMachine;
			_playback = (AnimationNodeStateMachinePlayback)animationTree.Get( "parameters/playback" );
			_playback.Travel( IdleAnimationName );

			_stateChanged = eventFactory
				.GetEvent<PlayerStateChangedEventArgs>(
					PlayerStateChangedEventArgs.Name,
					PlayerStateChangedEventArgs.NameSpace
				)
				.Subscribe( OnStateChanged );

			_locomotionCue = eventFactory
				.GetEvent<PlayerLocomotionCueEventArgs>(
					PlayerLocomotionCueEventArgs.Name,
					PlayerLocomotionCueEventArgs.NameSpace
				)
				.Subscribe( OnLocomotionCue );

			_directionalLocomotion = eventFactory
				.GetEvent<PlayerDirectionalLocomotionEventArgs>(
					PlayerDirectionalLocomotionEventArgs.Name,
					PlayerDirectionalLocomotionEventArgs.NameSpace
				)
				.Subscribe( OnDirectionalLocomotion );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_stateChanged.Dispose();
			_locomotionCue.Dispose();
			_directionalLocomotion.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnLocomotionCue( in PlayerLocomotionCueEventArgs args )
		{
			switch ( args.Cue ) {
				case PlayerLocomotionCue.HardStart:
					break;

				case PlayerLocomotionCue.HardStop:
					_playback.Travel( SuddenStopAnimationName );
					break;

				case PlayerLocomotionCue.Reverse:
					TravelOneShotLocomotion( Running180AnimationName );
					break;
			}
		}

		private void OnDirectionalLocomotion( in PlayerDirectionalLocomotionEventArgs args )
		{
			if ( !args.IsMoving ) {
				return;
			}

			if ( IsOneShotLocomotionActive() ) {
				return;
			}

			StringName desiredAnimation = args.Direction switch {
				PlayerLocomotionDirection.Backward => BackpedalAnimationName,
				PlayerLocomotionDirection.StrafeLeft => StrafeLeftRunAnimationName,
				PlayerLocomotionDirection.StrafeRight => StrafeRightRunAnimationName,
				_ => RunAnimationName
			};

			TravelLocomotion( desiredAnimation );
		}

		private void OnStateChanged( in PlayerStateChangedEventArgs args )
		{
			switch ( args.NewState ) {
				case PlayerStateId.Idle:
					SetAnimationStatus( PlayerAnimationState.Idle );
					break;

				case PlayerStateId.Moving:
					if ( _activeState != PlayerAnimationState.Running ) {
						SetAnimationStatus( PlayerAnimationState.Running );
					}
					break;

				case PlayerStateId.RestingAtCheckpoint:
					SetAnimationStatus( PlayerAnimationState.RestingAtCheckpoint );
					break;
			}
		}

		private void SetAnimationStatus( PlayerAnimationState state )
		{
			StringName animationName = state switch {
				PlayerAnimationState.Idle => IdleAnimationName,
				PlayerAnimationState.Running => RunAnimationName,
				PlayerAnimationState.RestingAtCheckpoint => RestingAtCheckpointAnimationName,
				PlayerAnimationState.TrueIdle => TrueIdleAnimationName,
				_ => throw new ArgumentOutOfRangeException( nameof( state ) )
			};

			_playback.Travel( animationName );
			_activeLocomotionAnimation = animationName;

			_activeState = state;
		}

		private void TravelLocomotion( StringName animationName )
		{
			StringName resolvedAnimation = CanTravelTo( animationName )
				? animationName
				: RunAnimationName;

			if ( !CanTravelTo( resolvedAnimation ) || _activeLocomotionAnimation == resolvedAnimation ) {
				return;
			}

			_playback.Travel( resolvedAnimation );
			_activeLocomotionAnimation = resolvedAnimation;
			_activeState = PlayerAnimationState.Running;
		}

		private void TravelOneShotLocomotion( StringName animationName )
		{
			if ( !CanTravelTo( animationName ) ) {
				return;
			}

			_playback.Travel( animationName );
			_activeLocomotionAnimation = animationName;
			_activeState = PlayerAnimationState.Running;
		}

		private bool IsOneShotLocomotionActive()
		{
			StringName currentNode = _playback.GetCurrentNode();
			return currentNode == Running180AnimationName || currentNode == SuddenStopAnimationName;
		}

		private bool CanTravelTo( StringName animationName )
		{
			return _stateMachine == null || _stateMachine.HasNode( animationName );
		}
	};
};
