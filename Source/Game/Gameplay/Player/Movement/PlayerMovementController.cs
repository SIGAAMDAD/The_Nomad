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
using Nomad.Game.Gameplay.Player.State;
using Nomad.Game.Gameplay.Player.Stats;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerMovementController

	===================================================================================
	*/
	/// <summary>
	/// Frame-pipeline orchestrator for the player movement stack. Each physics tick builds
	/// a PlayerMovementFrame, runs the interested controllers over it, commits body motion
	/// once, then publishes post-commit locomotion state.
	/// </summary>

	internal sealed class PlayerMovementController : IPlayerMovementController, IDisposable
	{
		public const float GAMEPLAY_UNITS_PER_WORLD_UNIT = PlayerMovementSettings.GAMEPLAY_UNITS_PER_WORLD_UNIT;

		public IGameEvent<PlayerLocomotionCueEventArgs> LocomotionCue => _locomotionEvents.LocomotionCue;
		public IGameEvent<PlayerDirectionalLocomotionEventArgs> DirectionalLocomotion => _locomotionEvents.DirectionalLocomotion;

		private readonly PlayerMovementSpeeds _speeds;

		private readonly PlayerStateCoordinator _stateCoordinator;
		private readonly PlayerFlagService _flagService;
		private readonly PlayerDerivedStatService _derivedStatService;
		private readonly PlayerParkourController _parkourController;

		private readonly IPlayerInputSource _inputSource;
		private readonly PlayerPrefab _prefab;
		private readonly PlayerMovementRuntime _runtime;
		private readonly PlayerCameraController _camera;
		private readonly PlayerMovementBodyMotor _bodyMotor;
		private readonly PlayerGroundMotionSolver _groundMotion;
		private readonly PlayerMovementActionController _actions;
		private readonly PlayerWallrunningController _wallrunning;
		private readonly PlayerLocomotionEventPublisher _locomotionEvents;
		private readonly IDisposable _statChangedSubscription;

		private bool _isDisposed;
		private uint _inputTick;

		public PlayerMovementController(
			PlayerPrefab prefab,
			PlayerFlagService flagService,
			PlayerStateCoordinator stateCoordinator,
			PlayerDerivedStatService derivedStatService,
			PlayerParkourController parkourController,
			IPlayerInputSource inputSource,
			IGameEventRegistryService eventFactory
		)
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_inputSource = inputSource ?? throw new ArgumentNullException( nameof( inputSource ) );
			_stateCoordinator = stateCoordinator ?? throw new ArgumentNullException( nameof( stateCoordinator ) );
			_flagService = flagService ?? throw new ArgumentNullException( nameof( flagService ) );
			_derivedStatService = derivedStatService ?? throw new ArgumentNullException( nameof( derivedStatService ) );
			_parkourController = parkourController ?? throw new ArgumentNullException( nameof( parkourController ) );

			_runtime = new PlayerMovementRuntime();
			_speeds = new PlayerMovementSpeeds( _derivedStatService );

			_camera = new PlayerCameraController(
				_prefab,
				PlayerCameraResolver.Resolve( _prefab, "CameraRig/Camera3D" ),
				_runtime,
				eventFactory
			);

			_bodyMotor = new PlayerMovementBodyMotor( _prefab, _runtime );
			_bodyMotor.ConfigureBody();

			_groundMotion = new PlayerGroundMotionSolver(
				_runtime,
				_speeds
			);

			_actions = new PlayerMovementActionController(
				_prefab.PeerId,
				_runtime,
				_camera,
				_flagService,
				eventFactory
			);

			_wallrunning = new PlayerWallrunningController(
				_prefab,
				_runtime,
				_flagService,
				_parkourController,
				eventFactory
			);

			_locomotionEvents = new PlayerLocomotionEventPublisher(
				_prefab.PeerId,
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
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_statChangedSubscription.Dispose();
			_actions.Dispose();
			_wallrunning.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		OnPhysicsUpdate
		===============
		*/
		/// <summary>
		/// Runs one movement transaction. Controllers are not allowed to commit body motion
		/// directly; they request a commit by mutating the current frame.
		/// </summary>
		/// <param name="delta"></param>
		public void OnPhysicsUpdate( float delta )
		{
			PlayerInputFrame input = _inputSource.ReadFrame( _inputTick++ );
			_runtime.BeginFrame(
				input,
				delta,
				_prefab.Velocity.ToSystem(),
				_prefab.IsOnFloor(),
				CanDriveMovement()
			);

			ref PlayerMovementFrame frame = ref _runtime.Current;
			_actions.BeginFrame( ref frame );

			if ( !frame.CanDriveMovement ) {
				ApplyLockedMovement( ref frame );
				CompleteCommittedFrame( ref frame );
				return;
			}

			if ( TryUpdateParkour( ref frame ) ) {
				CompleteParkourFrame( ref frame );
				return;
			}

			ResolveWishDirection( ref frame );

			if ( !TryUpdateWallrunning( ref frame ) ) {
				_actions.ProcessGroundActions( ref frame );
				UpdateGroundMovement( ref frame );
			}

			CompleteCommittedFrame( ref frame );
		}

		private bool CanDriveMovement()
		{
			return _stateCoordinator.CanMove && _stateCoordinator.CanTakeInput;
		}

		private void ResolveWishDirection( ref PlayerMovementFrame frame )
		{
			frame.SetWishDirection( _camera.GetWishDirection( frame.MoveInput ) );
		}

		private bool TryUpdateParkour( ref PlayerMovementFrame frame )
		{
			_parkourController.ApplyInput( frame.Input );
			_parkourController.PhysicsUpdate( frame.DeltaTime );

			if ( !_parkourController.IsActive ) {
				return false;
			}

			_wallrunning?.Exit();
			frame.MarkParkourHandled( _prefab.Velocity.ToSystem() );
			return true;
		}

		private bool TryUpdateWallrunning( ref PlayerMovementFrame frame )
		{
			if ( _wallrunning == null ) {
				return false;
			}

			return _wallrunning.TryUpdate( ref frame, _speeds.GetMovementSpeed() );
		}

		private void UpdateGroundMovement( ref PlayerMovementFrame frame )
		{
			Vector3 facingDirection = _camera.GetLookFacingDirection( frame.WishDirection );
			frame.SetFacingDirection( facingDirection, force: frame.HasMoveInput );

			Vector3 horizontalVelocity = _groundMotion.Solve( ref frame );
			frame.RequestGroundVelocity( horizontalVelocity, frame.Mode );
		}

		private void ApplyLockedMovement( ref PlayerMovementFrame frame )
		{
			_parkourController.ForceDetach( preserveVelocity: false );
			_wallrunning?.Exit( preserveVelocity: false );
			_actions.Cancel();

			frame.RequestLockedVelocity();
		}

		private void CompleteParkourFrame( ref PlayerMovementFrame frame )
		{
			_runtime.SynchronizeCommittedBodyVelocity( _prefab.Velocity.ToSystem() );

			if ( frame.ShouldResetDirectionalSnapshot ) {
				_locomotionEvents.ResetDirectionalSnapshot();
			}

			_runtime.EndFrame();
		}

		private void CompleteCommittedFrame( ref PlayerMovementFrame frame )
		{
			_bodyMotor.CommitFrame( ref frame );

			if ( frame.ShouldResetDirectionalSnapshot ) {
				_locomotionEvents.ResetDirectionalSnapshot();
			}

			UpdateFacing( ref frame );
			UpdateLocomotionState( ref frame );

			if ( frame.ShouldPublishLocomotion ) {
				_locomotionEvents.PublishFrame( in frame );
			}

			_runtime.EndFrame();
		}

		private void UpdateFacing( ref PlayerMovementFrame frame )
		{
			bool shouldFace = frame.ForceFacingUpdate
				|| frame.HasMoveInput
				|| frame.HorizontalSpeedSquared > PlayerCameraController.FACING_VELOCITY_THRESHOLD_SQUARED;

			if ( !shouldFace || frame.FacingDirectionLengthSquared <= PlayerMovementSettings.EPSILON ) {
				return;
			}

			_camera.UpdateFacing( frame.DeltaTime, frame.FacingDirection );
		}

		private void UpdateLocomotionState( ref PlayerMovementFrame frame )
		{
			bool moving = frame.HorizontalSpeedSquared > PlayerMovementSettings.MOVING_THRESHOLD;
			if ( moving ) {
				_stateCoordinator.SetMoving( PlayerStateChangeReason.Movement );
			} else {
				_stateCoordinator.SetIdle( PlayerStateChangeReason.Movement );
			}
		}

		private void OnStatChanged( in PlayerDerivedStatChangedEventArgs args )
		{
			_speeds.ApplyStatChange( args );
		}
	};
};
