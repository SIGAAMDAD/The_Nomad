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
using System.Runtime.CompilerServices;
using Godot;
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Player.Stats;
using NumericsVector2 = System.Numerics.Vector2;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerMovementController

	===================================================================================
	*/
	/// <summary>
	/// Third-person 3D movement controller.
	///
	/// The public input contract remains intentionally planar: PlayerInputFrame.Move is
	/// a Vector2. This controller projects that input onto the 3D world's XZ ground
	/// plane, relative to the active Camera3D, then drives CharacterBody3D directly.
	///
	/// This makes the player ready for a full model/skeleton pipeline while preserving
	/// the existing gameplay systems that still reason about movement in two axes.
	/// </summary>

	internal sealed class PlayerMovementController : NomadBehaviour, IMovementController
	{
		private const float EPSILON = 0.0001f;
		private const float MOVING_THRESHOLD = 0.001f;
		public const float GAMEPLAY_UNITS_PER_WORLD_UNIT = 32.0f;
		private const float HARD_START_THRESHOLD = 64.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		private const float HARD_STOP_THRESHOLD = 64.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;

		// Pixel-era project units are still used here. When the world is rescaled to
		// meters, divide these values by the same pixels-per-meter constant.
		private const float GRAVITY = 1800.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		private const float GROUND_STICK_VELOCITY = -32.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		private const float AIR_CONTROL_MULTIPLIER = 0.35f;
		private const float DASH_SPEED_FALLBACK_MULTIPLIER = 2.60f;
		private const float SLIDE_SPEED_MULTIPLIER = 1.65f;
		private const float SLIDE_FRICTION_MULTIPLIER = 0.45f;
		private const float FLOOR_SNAP_LENGTH = 12.0f / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		private const float FLOOR_MAX_ANGLE_DEGREES = 46.0f;

		public PlayerId Id { get; set; }
		public IPlayerDerivedStatService Stats { get; set; }
		public IPlayerFlagService Flags { get; set; }
		public IPlayerInputSource InputSource { get; set; }
		public IPlayerStateReader StateReader { get; set; }
		public IPlayerStateWriter StateWriter { get; set; }
		public IAimWriter AimWriter { get; set; }

		private float _effectiveMovementSpeed = 0.0f;
		private float _effectiveDashSpeed = 0.0f;

		private bool _isMoving = false;
		private uint _inputTick;

		private NumericsVector2 _moveInput = NumericsVector2.Zero;
		private readonly PlayerMovementRuntime _runtime;
		private readonly PlayerCameraController _camera;

		private PlayerPrefab _prefab;

		private readonly Godot.Timer _slideTimer;

		public IGameEvent<PlayerLocomotionCueEventArgs> LocomotionCue => _locomotionCue;
		private IGameEvent<PlayerLocomotionCueEventArgs> _locomotionCue = null;
		public IGameEvent<PlayerDirectionalLocomotionEventArgs> DirectionalLocomotion => _directionalLocomotion;
		private IGameEvent<PlayerDirectionalLocomotionEventArgs> _directionalLocomotion = null;

		/*
		===============
		PlayerMovementController
		===============
		*/
		public PlayerMovementController()
		{
			_slideTimer = new Godot.Timer() {
				WaitTime = Sdk.Player.Constants.SLIDE_DURATION,
				OneShot = true
			};
			_slideTimer.Timeout += OnSlideTimerTimeout;
		}

		/*
		===============
		OnInit
		===============
		*/
		public override void OnInit()
		{
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();
			_prefab.AddChild( _slideTimer );

			_prefab.UpDirection = Vector3.Up;
			_prefab.FloorSnapLength = FLOOR_SNAP_LENGTH;
			_prefab.FloorMaxAngle = Mathf.DegToRad( FLOOR_MAX_ANGLE_DEGREES );
			_prefab.SlideOnCeiling = true;

			_effectiveMovementSpeed = Stats.GetValue( DerivedStatType.EffectiveMovementSpeed );
			_effectiveDashSpeed = Stats.GetValue( DerivedStatType.EffectiveDashSpeed );

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerDashStartEventArgs>(
					PlayerDashStartEventArgs.Name,
					PlayerDashStartEventArgs.NameSpace
				)
				.Subscribe( OnDashStarted );

			eventFactory
				.GetEvent<PlayerDashEndedEventArgs>(
					PlayerDashEndedEventArgs.Name,
					PlayerDashEndedEventArgs.NameSpace
				)
				.Subscribe( OnDashEnded );

			eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>(
					PlayerDerivedStatChangedEventArgs.Name,
					PlayerDerivedStatChangedEventArgs.NameSpace
				)
				.Subscribe( OnStatChanged );

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

		/*
		===============
		OnShutdown
		===============
		*/
		public override void OnShutdown()
		{
			base.OnShutdown();

			var eventFactory = GameEventRegistry.Instance;

			eventFactory
				.GetEvent<PlayerDashStartEventArgs>(
					PlayerDashStartEventArgs.Name,
					PlayerDashStartEventArgs.NameSpace
				)
				.Unsubscribe( OnDashStarted );

			eventFactory
				.GetEvent<PlayerDashEndedEventArgs>(
					PlayerDashEndedEventArgs.Name,
					PlayerDashEndedEventArgs.NameSpace
				)
				.Unsubscribe( OnDashEnded );

			eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>(
					PlayerDerivedStatChangedEventArgs.Name,
					PlayerDerivedStatChangedEventArgs.NameSpace
				)
				.Unsubscribe( OnStatChanged );

			_slideTimer.Timeout -= OnSlideTimerTimeout;
			_slideTimer.Dispose();

			_locomotionCue.Dispose();
			_directionalLocomotion.Dispose();
		}

		/*
		===============
		OnPhysicsUpdate
		===============
		*/
		public override void OnPhysicsUpdate( float delta )
		{
			base.OnPhysicsUpdate( delta );

			Vector3 previousHorizontalVelocity = _runtime.HorizontalVelocity;
			PlayerInputFrame input = InputSource != null ? InputSource.ReadFrame( _inputTick++ ) : PlayerInputFrame.Empty;

			AimWriter.SetAimDirection( input.AimDirection, input.Tick );
			_moveInput = ClampInput( input.Move );
			_isMoving = _moveInput.LengthSquared() > MOVING_THRESHOLD;

			if ( !StateReader.CanMove || !StateReader.CanTakeInput ) {
				ApplyLockedMovement( previousHorizontalVelocity, input.Tick );
				return;
			}

			Vector3 wishDirection = _camera.GetWishDirection( _moveInput );
			if ( wishDirection.LengthSquared() > EPSILON ) {
				_runtime.LastPlanarWishDirection = wishDirection;
			}

			if ( input.SlidePressed ) {
				StartSlide( wishDirection );
			}

			Vector3 facingDirection = _camera.GetLookFacingDirection( wishDirection );
			_runtime.HorizontalVelocity = CalculateHorizontalVelocity( delta, wishDirection );
			ApplyVelocityToBody( delta );
			if ( _isMoving || _runtime.HorizontalVelocity.LengthSquared() > PlayerCameraController.FACING_VELOCITY_THRESHOLD_SQUARED ) {
				_camera.UpdateFacing( delta, facingDirection );
			}
			UpdateLocomotionState();
			PublishDirectionalLocomotion( wishDirection, facingDirection, input.Tick );
			PublishLocomotionCue( previousHorizontalVelocity, input.Tick );
		}

		/*
		===============
		ApplyLockedMovement
		===============
		*/
		private void ApplyLockedMovement( Vector3 previousHorizontalVelocity, uint tick )
		{
			_moveInput = NumericsVector2.Zero;
			_isMoving = false;
			_runtime.HorizontalVelocity = Vector3.Zero;

			Vector3 velocity = _prefab.Velocity;
			velocity.X = 0.0f;
			velocity.Z = 0.0f;
			velocity.Y = _prefab.IsOnFloor() ? GROUND_STICK_VELOCITY : velocity.Y;
			_prefab.Velocity = velocity;
			_prefab.MoveAndSlide();

			StateWriter.SetIdle( PlayerStateChangeReason.Movement );

			if ( previousHorizontalVelocity.LengthSquared() > MOVING_THRESHOLD ) {
				_locomotionCue.Publish(
					new PlayerLocomotionCueEventArgs(
						PlayerLocomotionCue.HardStop,
						ToPlanar2D( previousHorizontalVelocity ),
						NumericsVector2.Zero,
						false,
						NumericsVector2.Zero,
						tick
					)
				);
			}
		}

		/*
		===============
		CalculateHorizontalVelocity
		===============
		*/
		private Vector3 CalculateHorizontalVelocity( float delta, Vector3 wishDirection )
		{
			Vector3 targetVelocity;
			float acceleration = GameplayUnitsToWorldUnits( Sdk.Player.Constants.MOVEMENT_ACCELERATION );

			if ( Flags.GetFlags( PlayerFlags.Dashing ) ) {
				targetVelocity = _runtime.DashDirection * GetDashSpeed();
				acceleration *= 3.0f;
			} else if ( Flags.GetFlags( PlayerFlags.Sliding ) ) {
				targetVelocity = _runtime.SlideDirection * GetSlideSpeed();
				acceleration *= SLIDE_FRICTION_MULTIPLIER;
			} else if ( wishDirection.LengthSquared() > EPSILON ) {
				targetVelocity = wishDirection * GetMovementSpeed();
			} else {
				targetVelocity = Vector3.Zero;
				acceleration = GameplayUnitsToWorldUnits( Sdk.Player.Constants.MOVEMENT_FRICTION );
			}

			if ( !_prefab.IsOnFloor() ) {
				acceleration *= AIR_CONTROL_MULTIPLIER;
			}

			return MoveToward( _runtime.HorizontalVelocity, targetVelocity, acceleration * delta );
		}

		/*
		===============
		ApplyVelocityToBody
		===============
		*/
		private void ApplyVelocityToBody( float delta )
		{
			Vector3 velocity = _prefab.Velocity;
			velocity.X = _runtime.HorizontalVelocity.X;
			velocity.Z = _runtime.HorizontalVelocity.Z;

			if ( _prefab.IsOnFloor() ) {
				if ( velocity.Y < 0.0f ) {
					velocity.Y = GROUND_STICK_VELOCITY;
				}
			} else {
				velocity.Y -= GRAVITY * delta;
			}

			_prefab.Velocity = velocity;
			_prefab.MoveAndSlide();

			// CharacterBody3D may alter velocity during collision response. Feed the result
			// back into the planar controller so acceleration/deceleration stays stable.
			_runtime.HorizontalVelocity = new Vector3( _prefab.Velocity.X, 0.0f, _prefab.Velocity.Z );
		}

		/*
		===============
		UpdateLocomotionState
		===============
		*/
		private void UpdateLocomotionState()
		{
			bool moving = _runtime.HorizontalVelocity.LengthSquared() > MOVING_THRESHOLD;
			if ( moving ) {
				StateWriter.SetMoving( PlayerStateChangeReason.Movement );
			} else {
				StateWriter.SetIdle( PlayerStateChangeReason.Movement );
			}
		}

		/*
		===============
		PublishLocomotionCue
		===============
		*/
		private void PublishLocomotionCue( Vector3 previousHorizontalVelocity, uint tick )
		{
			PlayerLocomotionCue cue = DetectLocomotionCue( previousHorizontalVelocity );
			if ( cue == PlayerLocomotionCue.None ) {
				return;
			}

			_locomotionCue.Publish(
				new PlayerLocomotionCueEventArgs(
					cue,
					ToPlanar2D( previousHorizontalVelocity ),
					ToPlanar2D( _runtime.HorizontalVelocity ),
					_runtime.HorizontalVelocity.LengthSquared() > MOVING_THRESHOLD,
					_moveInput,
					tick
				)
			);
		}

		/*
		===============
		PublishDirectionalLocomotion
		===============
		*/
		private void PublishDirectionalLocomotion( Vector3 moveDirection, Vector3 facingDirection, uint tick )
		{
			bool isMoving = moveDirection.LengthSquared() > EPSILON;
			Vector3 normalizedMove = isMoving ? moveDirection.Normalized() : Vector3.Zero;
			Vector3 normalizedFacing = NormalizePlanarOrDefault( facingDirection, new Vector3( 0.0f, 0.0f, -1.0f ) );
			Vector3 facingRight = GetPlanarRight( normalizedFacing );

			float forwardAmount = isMoving ? normalizedFacing.Dot( normalizedMove ) : 0.0f;
			float rightAmount = isMoving ? facingRight.Dot( normalizedMove ) : 0.0f;
			PlayerLocomotionDirection direction = ClassifyDirectionalLocomotion(
				forwardAmount,
				rightAmount,
				isMoving
			);

			_directionalLocomotion.Publish(
				new PlayerDirectionalLocomotionEventArgs(
					Id,
					direction,
					ToPlanarDirection2D( normalizedMove ),
					ToPlanarDirection2D( normalizedFacing ),
					forwardAmount,
					rightAmount,
					isMoving,
					tick
				)
			);
		}

		/*
		===============
		DetectLocomotionCue
		===============
		*/
		private PlayerLocomotionCue DetectLocomotionCue( Vector3 previousHorizontalVelocity )
		{
			bool wasMoving = previousHorizontalVelocity.LengthSquared() > MOVING_THRESHOLD;
			bool isMoving = _runtime.HorizontalVelocity.LengthSquared() > MOVING_THRESHOLD;
			bool hasInput = _moveInput.LengthSquared() > MOVING_THRESHOLD;

			if ( !wasMoving && isMoving && _runtime.HorizontalVelocity.Length() >= HARD_START_THRESHOLD ) {
				return PlayerLocomotionCue.HardStart;
			}

			if ( wasMoving && !isMoving && previousHorizontalVelocity.Length() >= HARD_STOP_THRESHOLD ) {
				return PlayerLocomotionCue.HardStop;
			}

			if ( wasMoving && hasInput ) {
				Vector3 previousDir = previousHorizontalVelocity.Normalized();
				Vector3 wishDir = _camera.GetWishDirection( _moveInput );
				if ( wishDir.LengthSquared() <= EPSILON ) {
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

		/*
		===============
		StartSlide
		===============
		*/
		private void StartSlide( Vector3 wishDirection )
		{
			if ( Flags.GetFlags( PlayerFlags.Sliding ) ) {
				return;
			}

			_runtime.SlideDirection = _camera.ResolveActionDirection( wishDirection );
			Flags.AddFlags( PlayerFlags.Sliding );
			_slideTimer.Start();
		}

		/*
		===============
		OnSlideTimerTimeout
		===============
		*/
		private void OnSlideTimerTimeout()
		{
			Flags.RemoveFlags( PlayerFlags.Sliding );
		}

		/*
		===============
		OnDashStarted
		===============
		*/
		private void OnDashStarted( in PlayerDashStartEventArgs args )
		{
			_runtime.DashDirection = _camera.ResolveActionDirection( _camera.GetWishDirection( _moveInput ) );
			Flags.AddFlags( PlayerFlags.Dashing );
		}

		/*
		===============
		OnDashEnded
		===============
		*/
		private void OnDashEnded( in PlayerDashEndedEventArgs args )
		{
			Flags.RemoveFlags( PlayerFlags.Dashing );
		}

		/*
		===============
		OnStatChanged
		===============
		*/
		private void OnStatChanged( in PlayerDerivedStatChangedEventArgs args )
		{
			if ( args.StatId == DerivedStatType.EffectiveMovementSpeed ) {
				_effectiveMovementSpeed = args.NewValue;
				return;
			}

			if ( args.StatId == DerivedStatType.EffectiveDashSpeed ) {
				_effectiveDashSpeed = args.NewValue;
			}
		}

		/*
		===============
		ClassifyDirectionalLocomotion
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="forwardAmount"></param>
		/// <param name="rightAmount"></param>
		/// <param name="isMoving"></param>
		/// <returns></returns>
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

		/*
		===============
		NormalizePlanarOrDefault
		===============
		*/
		private static Vector3 NormalizePlanarOrDefault( Vector3 value, Vector3 fallback )
		{
			value.Y = 0.0f;
			if ( value.LengthSquared() > EPSILON ) {
				return value.Normalized();
			}

			fallback.Y = 0.0f;
			return fallback.LengthSquared() > EPSILON
				? fallback.Normalized()
				: new Vector3( 0.0f, 0.0f, -1.0f );
		}

		/*
		===============
		GetPlanarRight
		===============
		*/
		private static Vector3 GetPlanarRight( Vector3 forward )
		{
			return new Vector3( -forward.Z, 0.0f, forward.X ).Normalized();
		}

		/*
		===============
		GetDashSpeed
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float GetDashSpeed()
		{
			float fallback = GetMovementSpeed() * DASH_SPEED_FALLBACK_MULTIPLIER;
			return MathF.Max( fallback, GameplayUnitsToWorldUnits( _effectiveDashSpeed ) );
		}

		/*
		===============
		GetSlideSpeed
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float GetSlideSpeed()
		{
			return GetMovementSpeed() * SLIDE_SPEED_MULTIPLIER;
		}

		/*
		===============
		GetMovementSpeed
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float GetMovementSpeed()
		{
			return GameplayUnitsToWorldUnits( _effectiveMovementSpeed );
		}

		/*
		===============
		GameplayUnitsToWorldUnits
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static float GameplayUnitsToWorldUnits( float value )
		{
			return value / GAMEPLAY_UNITS_PER_WORLD_UNIT;
		}

		/*
		===============
		WorldUnitsToGameplayUnits
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static float WorldUnitsToGameplayUnits( float value )
		{
			return value * GAMEPLAY_UNITS_PER_WORLD_UNIT;
		}

		/*
		===============
		MoveToward
		===============
		*/
		private static Vector3 MoveToward( Vector3 from, Vector3 to, float delta )
		{
			Vector3 vector = to - from;
			float length = vector.Length();
			if ( length <= delta || length < 1E-06f ) {
				return to;
			}
			return from + (vector / length * delta);
		}

		/*
		===============
		ClampInput
		===============
		*/
		private static NumericsVector2 ClampInput( NumericsVector2 input )
		{
			float lengthSquared = input.LengthSquared();
			if ( lengthSquared <= 1.0f ) {
				return input;
			}
			return NumericsVector2.Normalize( input );
		}

		/*
		===============
		ToPlanar2D
		===============
		*/
		private static NumericsVector2 ToPlanar2D( Vector3 value )
		{
			return new NumericsVector2(
				WorldUnitsToGameplayUnits( value.X ),
				WorldUnitsToGameplayUnits( value.Z )
			);
		}

		/*
		===============
		ToPlanarDirection2D
		===============
		*/
		private static NumericsVector2 ToPlanarDirection2D( Vector3 value )
		{
			return new NumericsVector2( value.X, value.Z );
		}

		/*
		===============
		LerpAngle
		===============
		*/
		private static float LerpAngle( float from, float to, float weight )
		{
			float delta = WrapRadians( to - from );
			return from + (delta * weight);
		}

		/*
		===============
		WrapRadians
		===============
		*/
		private static float WrapRadians( float radians )
		{
			while ( radians > MathF.PI ) {
				radians -= 6.28318530717958647692f;
			}

			while ( radians < -MathF.PI ) {
				radians += 6.28318530717958647692f;
			}

			return radians;
		}
	};
};
