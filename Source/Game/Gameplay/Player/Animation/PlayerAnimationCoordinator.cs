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
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Animation;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Player.State;

namespace Nomad.Game.Gameplay.Player.Animation
{
	internal sealed class PlayerAnimationCoordinator : IPlayerAnimationController
	{
		public PlayerAnimationState Current => _playback.Current;

		public IGameEvent<PlayerAnimationStateChangedEventArgs> AnimationStateChanged => _animationStateChanged;
		private readonly IGameEvent<PlayerAnimationStateChangedEventArgs> _animationStateChanged;

		private readonly PlayerAnimationPlaybackRouter _playback;
		private readonly PlayerLocomotionAnimationController _locomotion;
		private readonly PlayerIdleAnimationController _idle;

		private readonly IDisposable _stateChanged;
		private readonly IDisposable _locomotionCue;
		private readonly IDisposable _directionalLocomotion;

		private bool _isDisposed;

		public PlayerAnimationCoordinator(
			PlayerId playerId,
			PlayerPrefab prefab,
			IPlayerMovementController movementController,
			IPlayerStateReader stateReader,
			IGameEventRegistryService eventFactory
		)
		{
			ArgumentGuard.ThrowIfNull( prefab, nameof( prefab ) );
			ArgumentGuard.ThrowIfNull( movementController, nameof( movementController ) );
			ArgumentGuard.ThrowIfNull( stateReader, nameof( stateReader ) );
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			playerId.ThrowIfInvalid( nameof( PlayerAnimationCoordinator ) );

			_animationStateChanged = eventFactory
				.GetEvent<PlayerAnimationStateChangedEventArgs>(
					PlayerAnimationStateChangedEventArgs.Name,
					PlayerAnimationStateChangedEventArgs.NameSpace
				);

			var animationTree = prefab.GetNode<AnimationTree>( "AnimationTree" );
			var stateMachine = animationTree.TreeRoot as AnimationNodeStateMachine;
			var playback = (AnimationNodeStateMachinePlayback)(GodotObject)animationTree.Get( "parameters/playback" );

			_playback = new PlayerAnimationPlaybackRouter(
				playerId,
				stateMachine,
				playback,
				_animationStateChanged
			);
			_locomotion = new PlayerLocomotionAnimationController( _playback );
			_idle = new PlayerIdleAnimationController( prefab, _playback );

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

			ApplyState( stateReader.Current );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_stateChanged.Dispose();
			_locomotionCue.Dispose();
			_directionalLocomotion.Dispose();
			_idle.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnStateChanged( in PlayerStateChangedEventArgs args )
		{
			ApplyState( args.NewState );
		}

		private void ApplyState( PlayerStateId state )
		{
			switch ( state ) {
				case PlayerStateId.Idle:
					_locomotion.Reset();
					_idle.Begin();
					break;

				case PlayerStateId.Moving:
					_idle.End();
					if ( _playback.Current != PlayerAnimationState.Running ) {
						_playback.TravelState( PlayerAnimationState.Running );
					}
					break;

				case PlayerStateId.RestingAtCheckpoint:
					_idle.End();
					_locomotion.Reset();
					_playback.TravelState( PlayerAnimationState.RestingAtCheckpoint );
					break;

				case PlayerStateId.Dead:
					_idle.End();
					_locomotion.Reset();
					_playback.TravelState( PlayerAnimationState.Dying );
					break;
			}
		}

		private void OnLocomotionCue( in PlayerLocomotionCueEventArgs args )
		{
			switch ( args.Cue ) {
				case PlayerLocomotionCue.HardStop:
					_idle.End();
					_locomotion.PlayHardStop();
					break;

				case PlayerLocomotionCue.Reverse:
					_idle.End();
					_locomotion.PlayReverse();
					break;
			}
		}

		private void OnDirectionalLocomotion( in PlayerDirectionalLocomotionEventArgs args )
		{
			if ( !args.IsMoving ) {
				return;
			}

			_idle.End();
			_locomotion.PlayDirectional( args.Direction );
		}
	}
}
