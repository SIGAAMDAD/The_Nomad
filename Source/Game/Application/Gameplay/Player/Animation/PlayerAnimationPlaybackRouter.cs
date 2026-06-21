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

using System.Runtime.CompilerServices;
using Godot;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Animation;
using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Application.Gameplay.Player.Animation
{
	internal enum PlayerAnimationClip : byte
	{
		Idle,
		Run,
		Backpedal,
		StrafeLeftRun,
		StrafeRightRun,
		Running180,
		SuddenStop,
		CheckpointResting,
		CheckpointDrinking,
		Dying,
		IdleStart,
		IdleMiddle,
		IdleFull,

		Count
	};

	internal sealed class PlayerAnimationPlaybackRouter
	{
		private static readonly StringName[] ClipAnimationNames = CreateClipAnimationNames();
		private static readonly PlayerAnimationClip[] StateClips = CreateStateClips();
		private static readonly PlayerAnimationClip[] LocomotionClips = CreateLocomotionClips();

		private readonly uint _availableClipMask;
		private readonly AnimationNodeStateMachinePlayback _playback;
		private readonly PlayerId _playerId;
		private readonly IGameEvent<PlayerAnimationStateChangedEventArgs> _animationStateChanged;

		public PlayerAnimationState Current { get; private set; } = PlayerAnimationState.Idle;

		public PlayerAnimationPlaybackRouter(
			PlayerId playerId,
			AnimationNodeStateMachine stateMachine,
			AnimationNodeStateMachinePlayback playback,
			IGameEvent<PlayerAnimationStateChangedEventArgs> animationStateChanged
		)
		{
			playerId.ThrowIfInvalid( nameof( PlayerAnimationPlaybackRouter ) );

			_playerId = playerId;
			_playback = playback;
			_animationStateChanged = animationStateChanged;
			_availableClipMask = CreateClipAvailabilityMask( stateMachine );

			_playback.Travel( ClipAnimationNames[(int)PlayerAnimationClip.Idle] );
		}

		public void TravelState( PlayerAnimationState state )
		{
			if ( Current == state ) {
				return;
			}

			PlayerAnimationClip clip = StateClips[(int)state];
			if ( !CanTravelTo( clip ) ) {
				return;
			}

			TravelUnchecked( clip, state );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void TravelLocomotion( PlayerLocomotionDirection direction )
		{
			PlayerAnimationClip clip = LocomotionClips[(int)direction];
			TravelLocomotionClip( clip );
		}

		public void TravelLocomotionClip( PlayerAnimationClip clip )
		{
			if ( !CanTravelTo( clip ) ) {
				return;
			}

			TravelUnchecked( clip, PlayerAnimationState.Running );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool IsCurrentClip( PlayerAnimationClip clip )
		{
			return _playback.GetCurrentNode() == ClipAnimationNames[(int)clip];
		}

		private void TravelUnchecked( PlayerAnimationClip clip, PlayerAnimationState state )
		{
			PlayerAnimationState oldState = Current;
			_playback.Travel( ClipAnimationNames[(int)clip] );
			Current = state;

			if ( oldState != state ) {
				_animationStateChanged.Publish(
					new PlayerAnimationStateChangedEventArgs(
						_playerId,
						oldState,
						state
					)
				);
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private bool CanTravelTo( PlayerAnimationClip clip )
		{
			return (_availableClipMask & (1u << (int)clip)) != 0u;
		}

		private static StringName[] CreateClipAnimationNames()
		{
			var names = new StringName[(int)PlayerAnimationClip.Count];
			names[(int)PlayerAnimationClip.Idle] = "Idle";
			names[(int)PlayerAnimationClip.Run] = "Run";
			names[(int)PlayerAnimationClip.Backpedal] = "Backpedal";
			names[(int)PlayerAnimationClip.StrafeLeftRun] = "StrafeLeftRun";
			names[(int)PlayerAnimationClip.StrafeRightRun] = "StrafeRightRun";
			names[(int)PlayerAnimationClip.Running180] = "Running180";
			names[(int)PlayerAnimationClip.SuddenStop] = "SuddenStop";
			names[(int)PlayerAnimationClip.CheckpointResting] = "CheckpointResting";
			names[(int)PlayerAnimationClip.CheckpointDrinking] = "CheckpointDrinking";
			names[(int)PlayerAnimationClip.Dying] = "Dying";
			names[(int)PlayerAnimationClip.IdleStart] = "IdleStart";
			names[(int)PlayerAnimationClip.IdleMiddle] = "IdleMiddle";
			names[(int)PlayerAnimationClip.IdleFull] = "IdleFull";
			return names;
		}

		private static PlayerAnimationClip[] CreateStateClips()
		{
			var clips = new PlayerAnimationClip[(int)PlayerAnimationState.Count];
			clips[(int)PlayerAnimationState.Idle] = PlayerAnimationClip.Idle;
			clips[(int)PlayerAnimationState.Running] = PlayerAnimationClip.Run;
			clips[(int)PlayerAnimationState.Sliding] = PlayerAnimationClip.Run;
			clips[(int)PlayerAnimationState.RestingAtCheckpoint] = PlayerAnimationClip.CheckpointResting;
			clips[(int)PlayerAnimationState.DrinkingCheckpoint] = PlayerAnimationClip.CheckpointDrinking;
			clips[(int)PlayerAnimationState.Dying] = PlayerAnimationClip.Dying;
			clips[(int)PlayerAnimationState.IdleStart] = PlayerAnimationClip.IdleStart;
			clips[(int)PlayerAnimationState.IdleMiddle] = PlayerAnimationClip.IdleMiddle;
			clips[(int)PlayerAnimationState.IdleFull] = PlayerAnimationClip.IdleFull;
			return clips;
		}

		private static PlayerAnimationClip[] CreateLocomotionClips()
		{
			var clips = new PlayerAnimationClip[(int)PlayerLocomotionDirection.Count];
			clips[(int)PlayerLocomotionDirection.Idle] = PlayerAnimationClip.Idle;
			clips[(int)PlayerLocomotionDirection.Forward] = PlayerAnimationClip.Run;
			clips[(int)PlayerLocomotionDirection.Backward] = PlayerAnimationClip.Backpedal;
			clips[(int)PlayerLocomotionDirection.StrafeLeft] = PlayerAnimationClip.StrafeLeftRun;
			clips[(int)PlayerLocomotionDirection.StrafeRight] = PlayerAnimationClip.StrafeRightRun;
			clips[(int)PlayerLocomotionDirection.Running180] = PlayerAnimationClip.Running180;
			return clips;
		}

		private static uint CreateClipAvailabilityMask( AnimationNodeStateMachine stateMachine )
		{
			if ( stateMachine == null ) {
				return (1u << (int)PlayerAnimationClip.Count) - 1u;
			}

			uint mask = 0u;
			for ( int i = 0; i < (int)PlayerAnimationClip.Count; i++ ) {
				if ( stateMachine.HasNode( ClipAnimationNames[i] ) ) {
					mask |= 1u << i;
				}
			}

			return mask;
		}
	};
};
