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
using Nomad.EngineUtils;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;

namespace Nomad.Game.Gameplay.Player.Movement
{
	internal sealed class PlayerLocomotionEventPublisher
	{
		private const float DIRECTION_DELTA_THRESHOLD = 0.035f;
		private const uint DIRECTIONAL_REFRESH_TICKS = 8;

		public IGameEvent<PlayerLocomotionCueEventArgs> LocomotionCue => _locomotionCue;
		public IGameEvent<PlayerDirectionalLocomotionEventArgs> DirectionalLocomotion => _directionalLocomotion;

		private readonly PlayerId _id;
		private readonly PlayerPrefab _prefab;
		private readonly PlayerMovementRuntime _runtime;
		private readonly PlayerCameraController _camera;
		private readonly IGameEvent<PlayerLocomotionCueEventArgs> _locomotionCue;
		private readonly IGameEvent<PlayerDirectionalLocomotionEventArgs> _directionalLocomotion;

		private PlayerLocomotionDirection _lastDirection = PlayerLocomotionDirection.Count;
		private Vector2 _lastMoveDirection = Vector2.Zero;
		private Vector2 _lastFacingDirection = Vector2.Zero;
		private bool _lastDirectionalMoving;
		private bool _hasDirectionalSnapshot;
		private uint _lastDirectionalTick;

		public PlayerLocomotionEventPublisher(
			PlayerId id,
			PlayerPrefab prefab,
			PlayerMovementRuntime runtime,
			PlayerCameraController camera,
			IGameEventRegistryService eventFactory
		)
		{
			_id = id;
			_prefab = prefab;
			_runtime = runtime;
			_camera = camera;

			_locomotionCue = eventFactory
				.GetEvent<PlayerLocomotionCueEventArgs>(
					PlayerLocomotionCueEventArgs.Name,
					PlayerLocomotionCueEventArgs.NameSpace,
					EventFlags.NoLock
				);

			_directionalLocomotion = eventFactory
				.GetEvent<PlayerDirectionalLocomotionEventArgs>(
					PlayerDirectionalLocomotionEventArgs.Name,
					PlayerDirectionalLocomotionEventArgs.NameSpace,
					EventFlags.NoLock
				);
		}

		public void ResetDirectionalSnapshot()
		{
			_hasDirectionalSnapshot = false;
			_lastDirection = PlayerLocomotionDirection.Count;
			_lastMoveDirection = Vector2.Zero;
			_lastFacingDirection = Vector2.Zero;
			_lastDirectionalMoving = false;
		}

		public void PublishLockedStopIfNeeded( in PlayerMovementFrame frame )
		{
			if ( frame.PreviousHorizontalSpeedSquared <= PlayerMovementSettings.MOVING_THRESHOLD ) {
				return;
			}

			_locomotionCue.Publish(
				new PlayerLocomotionCueEventArgs(
					_id,
					PlayerLocomotionCue.HardStop,
					ToPlanar2D( frame.PreviousHorizontalVelocity ),
					Vector2.Zero,
					false,
					Vector2.Zero,
					_prefab.GlobalPosition.ToSystem(),
					frame.Tick
				)
			);
		}

		public void PublishFrame( in PlayerMovementFrame frame )
		{
			PublishDirectionalLocomotion( frame.WishDirection, frame.WishDirectionLengthSquared, frame.FacingDirection, frame.Tick );
			PublishLocomotionCue( in frame );
		}

		private void PublishLocomotionCue( in PlayerMovementFrame frame )
		{
			PlayerLocomotionCue cue = DetectLocomotionCue( in frame );
			if ( cue == PlayerLocomotionCue.None ) {
				return;
			}

			_locomotionCue.Publish(
				new PlayerLocomotionCueEventArgs(
					_id,
					cue,
					ToPlanar2D( frame.PreviousHorizontalVelocity ),
					ToPlanar2D( frame.HorizontalVelocity ),
					frame.HorizontalSpeedSquared > PlayerMovementSettings.MOVING_THRESHOLD,
					frame.MoveInput,
					_prefab.GlobalPosition.ToSystem(),
					frame.Tick
				)
			);
		}

		private void PublishDirectionalLocomotion( Vector3 moveDirection, float moveDirectionLengthSquared, Vector3 facingDirection, uint tick )
		{
			bool isMoving = moveDirectionLengthSquared > PlayerMovementSettings.EPSILON;
			Vector3 normalizedMove = isMoving ? moveDirection : Vector3.Zero;
			Vector3 normalizedFacing = PlayerMovementMath.NormalizePlanarOrDefault(
				facingDirection,
				new Vector3( 0.0f, 0.0f, -1.0f )
			);
			Vector3 facingRight = PlayerMovementMath.GetPlanarRight( normalizedFacing );

			float forwardAmount = isMoving ? Vector3.Dot( normalizedFacing, normalizedMove ) : 0.0f;
			float rightAmount = isMoving ? Vector3.Dot( facingRight, normalizedMove ) : 0.0f;
			PlayerLocomotionDirection direction = ClassifyDirectionalLocomotion( forwardAmount, rightAmount, isMoving );

			Vector2 planarMove = ToPlanarDirection2D( normalizedMove );
			Vector2 planarFacing = ToPlanarDirection2D( normalizedFacing );

			if ( !ShouldPublishDirectional( direction, planarMove, planarFacing, isMoving, tick ) ) {
				return;
			}

			_directionalLocomotion.Publish(
				new PlayerDirectionalLocomotionEventArgs(
					_id,
					direction,
					planarMove,
					planarFacing,
					forwardAmount,
					rightAmount,
					isMoving,
					tick
				)
			);

			_lastDirection = direction;
			_lastMoveDirection = planarMove;
			_lastFacingDirection = planarFacing;
			_lastDirectionalMoving = isMoving;
			_lastDirectionalTick = tick;
			_hasDirectionalSnapshot = true;
		}

		private bool ShouldPublishDirectional(
			PlayerLocomotionDirection direction,
			Vector2 moveDirection,
			Vector2 facingDirection,
			bool isMoving,
			uint tick
		)
		{
			if ( !_hasDirectionalSnapshot ) {
				return true;
			}

			if ( direction != _lastDirection || isMoving != _lastDirectionalMoving ) {
				return true;
			}

			if ( tick - _lastDirectionalTick >= DIRECTIONAL_REFRESH_TICKS ) {
				return true;
			}

			return Vector2.DistanceSquared( moveDirection, _lastMoveDirection ) > DIRECTION_DELTA_THRESHOLD
				|| Vector2.DistanceSquared( facingDirection, _lastFacingDirection ) > DIRECTION_DELTA_THRESHOLD;
		}

		private PlayerLocomotionCue DetectLocomotionCue( in PlayerMovementFrame frame )
		{
			bool wasMoving = frame.PreviousHorizontalSpeedSquared > PlayerMovementSettings.MOVING_THRESHOLD;
			bool isMoving = frame.HorizontalSpeedSquared > PlayerMovementSettings.MOVING_THRESHOLD;
			bool hasInput = frame.MoveInputLengthSquared > PlayerMovementSettings.MOVING_THRESHOLD;
			float hardStartThresholdSquared = PlayerMovementSettings.HARD_START_THRESHOLD * PlayerMovementSettings.HARD_START_THRESHOLD;
			float hardStopThresholdSquared = PlayerMovementSettings.HARD_STOP_THRESHOLD * PlayerMovementSettings.HARD_STOP_THRESHOLD;

			if ( !wasMoving && isMoving && frame.HorizontalSpeedSquared >= hardStartThresholdSquared ) {
				return PlayerLocomotionCue.HardStart;
			}

			if ( wasMoving && !isMoving && frame.PreviousHorizontalSpeedSquared >= hardStopThresholdSquared ) {
				return PlayerLocomotionCue.HardStop;
			}

			if ( wasMoving && hasInput ) {
				Vector3 previousDir = Vector3.Normalize( frame.PreviousHorizontalVelocity );
				Vector3 wishDir = frame.WishDirection;
				if ( frame.WishDirectionLengthSquared <= PlayerMovementSettings.EPSILON ) {
					return PlayerLocomotionCue.None;
				}

				float dot = Vector3.Dot( previousDir, wishDir );
				if ( dot < -0.15f ) {
					return PlayerLocomotionCue.Reverse;
				}

				if ( dot < 0.35f ) {
					return PlayerLocomotionCue.SharpTurn;
				}
			}

			return PlayerLocomotionCue.None;
		}

		private static PlayerLocomotionDirection ClassifyDirectionalLocomotion(
			float forwardAmount,
			float rightAmount,
			bool isMoving
		)
		{
			if ( !isMoving ) {
				return PlayerLocomotionDirection.Idle;
			}

			if ( MathF.Abs( rightAmount ) > MathF.Abs( forwardAmount ) ) {
				return rightAmount < 0.0f
					? PlayerLocomotionDirection.StrafeLeft
					: PlayerLocomotionDirection.StrafeRight;
			}

			return forwardAmount < 0.0f
				? PlayerLocomotionDirection.Backward
				: PlayerLocomotionDirection.Forward;
		}

		private static Vector2 ToPlanar2D( Vector3 value )
		{
			return new Vector2(
				PlayerMovementSettings.WorldUnitsToGameplayUnits( value.X ),
				PlayerMovementSettings.WorldUnitsToGameplayUnits( value.Z )
			);
		}

		private static Vector2 ToPlanarDirection2D( Vector3 value )
		{
			return new Vector2( value.X, value.Z );
		}
	};
};
