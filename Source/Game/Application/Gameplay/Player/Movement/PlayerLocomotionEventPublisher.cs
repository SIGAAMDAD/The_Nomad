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
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;
using NumericsVector2 = System.Numerics.Vector2;

namespace Nomad.Game.Application.Gameplay.Player.Movement
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
		private NumericsVector2 _lastMoveDirection = NumericsVector2.Zero;
		private NumericsVector2 _lastFacingDirection = NumericsVector2.Zero;
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
			_lastMoveDirection = NumericsVector2.Zero;
			_lastFacingDirection = NumericsVector2.Zero;
			_lastDirectionalMoving = false;
		}

		public void PublishLockedStopIfNeeded( Vector3 previousHorizontalVelocity, uint tick )
		{
			if ( previousHorizontalVelocity.LengthSquared() <= PlayerMovementSettings.MOVING_THRESHOLD ) {
				return;
			}

			_locomotionCue.Publish(
				new PlayerLocomotionCueEventArgs(
					_id,
					PlayerLocomotionCue.HardStop,
					ToPlanar2D( previousHorizontalVelocity ),
					NumericsVector2.Zero,
					false,
					NumericsVector2.Zero,
					_prefab.GlobalPosition.ToSystem(),
					tick
				)
			);
		}

		public void PublishFrame(
			Vector3 previousHorizontalVelocity,
			Vector3 moveDirection,
			Vector3 facingDirection,
			NumericsVector2 moveInput,
			uint tick
		)
		{
			PublishDirectionalLocomotion( moveDirection, facingDirection, tick );
			PublishLocomotionCue( previousHorizontalVelocity, moveInput, tick );
		}

		private void PublishLocomotionCue( Vector3 previousHorizontalVelocity, NumericsVector2 moveInput, uint tick )
		{
			PlayerLocomotionCue cue = DetectLocomotionCue( previousHorizontalVelocity, moveInput );
			if ( cue == PlayerLocomotionCue.None ) {
				return;
			}

			_locomotionCue.Publish(
				new PlayerLocomotionCueEventArgs(
					_id,
					cue,
					ToPlanar2D( previousHorizontalVelocity ),
					ToPlanar2D( _runtime.HorizontalVelocity ),
					_runtime.HorizontalVelocity.LengthSquared() > PlayerMovementSettings.MOVING_THRESHOLD,
					moveInput,
					_prefab.GlobalPosition.ToSystem(),
					tick
				)
			);
		}

		private void PublishDirectionalLocomotion( Vector3 moveDirection, Vector3 facingDirection, uint tick )
		{
			bool isMoving = moveDirection.LengthSquared() > PlayerMovementSettings.EPSILON;
			Vector3 normalizedMove = isMoving ? moveDirection.Normalized() : Vector3.Zero;
			Vector3 normalizedFacing = PlayerMovementMath.NormalizePlanarOrDefault(
				facingDirection,
				new Vector3( 0.0f, 0.0f, -1.0f )
			);
			Vector3 facingRight = PlayerMovementMath.GetPlanarRight( normalizedFacing );

			float forwardAmount = isMoving ? normalizedFacing.Dot( normalizedMove ) : 0.0f;
			float rightAmount = isMoving ? facingRight.Dot( normalizedMove ) : 0.0f;
			PlayerLocomotionDirection direction = ClassifyDirectionalLocomotion( forwardAmount, rightAmount, isMoving );

			NumericsVector2 planarMove = ToPlanarDirection2D( normalizedMove );
			NumericsVector2 planarFacing = ToPlanarDirection2D( normalizedFacing );

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
			NumericsVector2 moveDirection,
			NumericsVector2 facingDirection,
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

			return NumericsVector2.DistanceSquared( moveDirection, _lastMoveDirection ) > DIRECTION_DELTA_THRESHOLD
				|| NumericsVector2.DistanceSquared( facingDirection, _lastFacingDirection ) > DIRECTION_DELTA_THRESHOLD;
		}

		private PlayerLocomotionCue DetectLocomotionCue( Vector3 previousHorizontalVelocity, NumericsVector2 moveInput )
		{
			bool wasMoving = previousHorizontalVelocity.LengthSquared() > PlayerMovementSettings.MOVING_THRESHOLD;
			bool isMoving = _runtime.HorizontalVelocity.LengthSquared() > PlayerMovementSettings.MOVING_THRESHOLD;
			bool hasInput = moveInput.LengthSquared() > PlayerMovementSettings.MOVING_THRESHOLD;

			if ( !wasMoving && isMoving && _runtime.HorizontalVelocity.Length() >= PlayerMovementSettings.HARD_START_THRESHOLD ) {
				return PlayerLocomotionCue.HardStart;
			}

			if ( wasMoving && !isMoving && previousHorizontalVelocity.Length() >= PlayerMovementSettings.HARD_STOP_THRESHOLD ) {
				return PlayerLocomotionCue.HardStop;
			}

			if ( wasMoving && hasInput ) {
				Vector3 previousDir = previousHorizontalVelocity.Normalized();
				Vector3 wishDir = _camera.GetWishDirection( moveInput );
				if ( wishDir.LengthSquared() <= PlayerMovementSettings.EPSILON ) {
					return PlayerLocomotionCue.None;
				}

				float dot = previousDir.Dot( wishDir.Normalized() );
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

		private static NumericsVector2 ToPlanar2D( Vector3 value )
		{
			return new NumericsVector2(
				PlayerMovementSettings.WorldUnitsToGameplayUnits( value.X ),
				PlayerMovementSettings.WorldUnitsToGameplayUnits( value.Z )
			);
		}

		private static NumericsVector2 ToPlanarDirection2D( Vector3 value )
		{
			return new NumericsVector2( value.X, value.Z );
		}
	}
}
