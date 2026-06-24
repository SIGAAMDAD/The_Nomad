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
using Nomad.Game.Sdk.Events.Player;
using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerMovementController

	===================================================================================
	*/
	/// <summary>
	/// Thin orchestration layer for the player movement stack. Input sampling, body
	/// movement, dash/slide bridging, locomotion publishing, wall-running, and traversal
	/// remain separate collaborators so this component only arbitrates the active mode.
	/// </summary>

	internal sealed class PlayerMovementController : NomadBehaviour, IPlayerMovementController
	{
		public const float GAMEPLAY_UNITS_PER_WORLD_UNIT = PlayerMovementSettings.GAMEPLAY_UNITS_PER_WORLD_UNIT;

		public PlayerId Id { get; set; }
		public IPlayerDerivedStatService Stats { get; set; }
		public IPlayerFlagService Flags { get; set; }
		public IPlayerInputSource InputSource { get; set; }
		public IPlayerStateReader StateReader { get; set; }
		public IPlayerStateWriter StateWriter { get; set; }
		public IPlayerParkourController ParkourController { get; set; }
		public string CameraNodePath { get; set; } = "CameraRig/PlayerCamera3D";

		public IGameEvent<PlayerLocomotionCueEventArgs> LocomotionCue => _locomotionEvents.LocomotionCue;
		public IGameEvent<PlayerDirectionalLocomotionEventArgs> DirectionalLocomotion => _locomotionEvents.DirectionalLocomotion;

		private readonly PlayerMovementSpeeds _speeds = new();

		private PlayerPrefab _prefab;
		private PlayerMovementRuntime _runtime;
		private PlayerCameraController _camera;
		private PlayerMovementBodyMotor _bodyMotor;
		private PlayerGroundMotionSolver _groundMotion;
		private PlayerSlideController _slide;
		private PlayerDashMovementBridge _dash;
		private PlayerWallrunningController _wallrunning;
		private PlayerLocomotionEventPublisher _locomotionEvents;

		private IDisposable _statChangedSubscription;
		private uint _inputTick;
		private bool _isInitialized;

		/*
		===============
		OnInit
		===============
		*/
		public override void OnInit()
		{
			base.OnInit();

			_prefab = Object.CastAs<PlayerPrefab>();
			_runtime = new PlayerMovementRuntime();
			_speeds.Initialize( Stats );

			var eventFactory = GameEventRegistry.Instance;

			_camera = new PlayerCameraController(
				_prefab,
				PlayerCameraResolver.Resolve( _prefab, CameraNodePath ),
				_runtime,
				eventFactory
			);

			_bodyMotor = new PlayerMovementBodyMotor( _prefab, _runtime );
			_bodyMotor.ConfigureBody();

			_groundMotion = new PlayerGroundMotionSolver(
				_prefab,
				_runtime,
				Flags,
				_speeds
			);

			_slide = new PlayerSlideController(
				_prefab,
				_runtime,
				_camera,
				Flags
			);

			_dash = new PlayerDashMovementBridge(
				Id,
				_runtime,
				_camera,
				Flags,
				eventFactory
			);

			_wallrunning = new PlayerWallrunningController(
				_prefab,
				_runtime,
				Flags,
				ParkourController,
				eventFactory
			);

			_locomotionEvents = new PlayerLocomotionEventPublisher(
				Id,
				_prefab,
				_runtime,
				_camera,
				eventFactory
			);

			_statChangedSubscription = eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>(
					PlayerDerivedStatChangedEventArgs.Name,
					PlayerDerivedStatChangedEventArgs.NameSpace
				)
				.Subscribe( OnStatChanged );

			_isInitialized = true;
		}

		/*
		===============
		OnShutdown
		===============
		*/
		public override void OnShutdown()
		{
			base.OnShutdown();

			_statChangedSubscription?.Dispose();
			_dash?.Dispose();
			_slide?.Dispose();
			_wallrunning?.Dispose();

			_statChangedSubscription = null;
			_dash = null;
			_slide = null;
			_wallrunning = null;
			_locomotionEvents = null;
			_groundMotion = null;
			_bodyMotor = null;
			_camera = null;
			_runtime = null;
			_prefab = null;
			_isInitialized = false;
		}

		/*
		===============
		OnPhysicsUpdate
		===============
		*/
		public override void OnPhysicsUpdate( float delta )
		{
			base.OnPhysicsUpdate( delta );

			if ( !_isInitialized ) {
				return;
			}

			Vector3 previousHorizontalVelocity = _runtime.HorizontalVelocity;
			PlayerInputFrame input = ReadInputFrame();
			PlayerMovementInputState inputState = PlayerMovementInputState.FromFrame( input );
			_dash.UpdateInput( inputState.Move );

			if ( !CanDriveMovement() ) {
				ApplyLockedMovement( previousHorizontalVelocity, input.Tick );
				return;
			}

			if ( TryUpdateParkour( input, delta ) ) {
				return;
			}

			Vector3 wishDirection = ResolveWishDirection( inputState.Move );
			if ( TryUpdateWallrunning( input, inputState, delta, wishDirection, previousHorizontalVelocity ) ) {
				return;
			}

			UpdateGroundMovement( input, inputState, delta, wishDirection, previousHorizontalVelocity );
		}

		/*
		===============
		SetParkourController
		===============
		*/
		public void SetParkourController( IPlayerParkourController parkour )
		{
			ParkourController = parkour;
			_wallrunning?.SetParkourController( parkour );
		}

		private PlayerInputFrame ReadInputFrame()
		{
			return InputSource != null
				? InputSource.ReadFrame( _inputTick++ )
				: PlayerInputFrame.Empty;
		}

		private bool CanDriveMovement()
		{
			return StateReader.CanMove && StateReader.CanTakeInput;
		}

		private Vector3 ResolveWishDirection( in System.Numerics.Vector2 moveInput )
		{
			Vector3 wishDirection = _camera.GetWishDirection( moveInput );
			if ( wishDirection.LengthSquared() > PlayerMovementSettings.EPSILON ) {
				_runtime.LastPlanarWishDirection = wishDirection;
			}

			return wishDirection;
		}

		private bool TryUpdateParkour( in PlayerInputFrame input, float delta )
		{
			if ( ParkourController == null ) {
				return false;
			}

			ParkourController.ApplyInput( input );
			ParkourController.PhysicsUpdate( delta );

			if ( !ParkourController.IsActive ) {
				return false;
			}

			_wallrunning?.Exit();
			_runtime.HorizontalVelocity = Vector3.Zero;
			_locomotionEvents.ResetDirectionalSnapshot();
			return true;
		}

		private bool TryUpdateWallrunning(
			in PlayerInputFrame input,
			in PlayerMovementInputState inputState,
			float delta,
			Vector3 wishDirection,
			Vector3 previousHorizontalVelocity
		)
		{
			if ( _wallrunning == null ) {
				return false;
			}

			if ( !_wallrunning.TryUpdate(
				delta,
				input,
				wishDirection,
				_speeds.GetMovementSpeed(),
				out Vector3 wallFacingDirection
			) ) {
				return false;
			}

			Vector3 facingDirection = wallFacingDirection.LengthSquared() > PlayerMovementSettings.EPSILON
				? wallFacingDirection
				: _camera.GetLookFacingDirection( wishDirection );

			UpdateFacing( delta, facingDirection, force: true );
			UpdateLocomotionState();
			_locomotionEvents.PublishFrame(
				previousHorizontalVelocity,
				wishDirection,
				facingDirection,
				inputState.Move,
				input.Tick
			);
			return true;
		}

		private void UpdateGroundMovement(
			in PlayerInputFrame input,
			in PlayerMovementInputState inputState,
			float delta,
			Vector3 wishDirection,
			Vector3 previousHorizontalVelocity
		)
		{
			if ( input.SlidePressed ) {
				_slide.TryStart( wishDirection );
			}

			Vector3 facingDirection = _camera.GetLookFacingDirection( wishDirection );
			_runtime.HorizontalVelocity = _groundMotion.Solve( delta, wishDirection );
			_bodyMotor.ApplyGroundVelocity( delta, _runtime.HorizontalVelocity );

			UpdateFacing(
				delta,
				facingDirection,
				inputState.HasMoveInput || _runtime.HorizontalVelocity.LengthSquared() > PlayerCameraController.FACING_VELOCITY_THRESHOLD_SQUARED
			);

			UpdateLocomotionState();
			_locomotionEvents.PublishFrame(
				previousHorizontalVelocity,
				wishDirection,
				facingDirection,
				inputState.Move,
				input.Tick
			);
		}

		private void UpdateFacing( float delta, Vector3 facingDirection, bool force )
		{
			if ( !force || facingDirection.LengthSquared() <= PlayerMovementSettings.EPSILON ) {
				return;
			}

			_camera.UpdateFacing( delta, facingDirection );
		}

		private void ApplyLockedMovement( Vector3 previousHorizontalVelocity, uint tick )
		{
			ParkourController?.ForceDetach( preserveVelocity: false );
			_wallrunning?.Exit( preserveVelocity: false );
			_slide?.Cancel();
			_dash?.Cancel();

			_bodyMotor.ApplyLockedVelocity();
			StateWriter.SetIdle( PlayerStateChangeReason.Movement );
			_locomotionEvents.ResetDirectionalSnapshot();
			_locomotionEvents.PublishLockedStopIfNeeded( previousHorizontalVelocity, tick );
		}

		private void UpdateLocomotionState()
		{
			bool moving = _runtime.HorizontalVelocity.LengthSquared() > PlayerMovementSettings.MOVING_THRESHOLD;
			if ( moving ) {
				StateWriter.SetMoving( PlayerStateChangeReason.Movement );
			} else {
				StateWriter.SetIdle( PlayerStateChangeReason.Movement );
			}
		}

		private void OnStatChanged( in PlayerDerivedStatChangedEventArgs args )
		{
			_speeds.ApplyStatChange( args );
		}
	}
}
