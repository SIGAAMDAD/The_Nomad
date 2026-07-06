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
using Nomad.Game.Gameplay.Player.Movement.WallRunning;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Player.State;

namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerWallrunningController

	===================================================================================
	*/
	/// <summary>
	/// Dynamic wall-mounted locomotion controller. It now participates in the movement
	/// frame pipeline by requesting body velocity commits instead of calling MoveAndSlide.
	/// </summary>

	internal sealed class PlayerWallrunningController : IDisposable
	{
		public bool IsActive => _isActive;
		public WallMountedMoveMode Mode => _mode;
		public WallSurfaceCandidate CurrentSurface => _surface;
		public Vector3 CurrentNormal => _surface.Normal;
		public Vector3 CurrentRunDirection => _runDirection;
		public WallRunSettings Settings => _settings;

		public uint SurfaceMask { get => _settings.SurfaceMask; set => _settings.SurfaceMask = value; }
		public float ChestProbeHeight { get => _settings.ChestProbeHeight; set => _settings.ChestProbeHeight = value; }
		public float HipProbeHeight { get => _settings.HipProbeHeight; set => _settings.HipProbeHeight = value; }
		public float SideProbeDistance { get => _settings.SideProbeDistance; set => _settings.SideProbeDistance = value; }
		public float ForwardProbeDistance { get => _settings.ForwardProbeDistance; set => _settings.ForwardProbeDistance = value; }
		public float SurfaceRetainDistance { get => _settings.SurfaceRetainDistance; set => _settings.SurfaceRetainDistance = value; }
		public float MinimumSurfaceVerticality { get => _settings.MinimumSurfaceVerticality; set => _settings.MinimumSurfaceVerticality = value; }
		public float MinimumEntrySpeed { get => _settings.MinimumEntrySpeed; set => _settings.MinimumEntrySpeed = value; }
		public float MinimumInputStrength { get => _settings.MinimumInputStrength; set => _settings.MinimumInputStrength = value; }
		public float MinimumIntoWallForVerticalRun { get => _settings.MinimumIntoWallForVerticalRun; set => _settings.MinimumIntoWallForVerticalRun = value; }
		public float UpIntentThreshold { get => _settings.UpIntentThreshold; set => _settings.UpIntentThreshold = value; }
		public float DownIntentThreshold { get => _settings.DownIntentThreshold; set => _settings.DownIntentThreshold = value; }
		public float MaximumRunDuration { get => _settings.MaximumRunDuration; set => _settings.MaximumRunDuration = value; }
		public float ReattachCooldown { get => _settings.ReattachCooldown; set => _settings.ReattachCooldown = value; }
		public float HorizontalSpeedMultiplier { get => _settings.HorizontalSpeedMultiplier; set => _settings.HorizontalSpeedMultiplier = value; }
		public float MinimumHorizontalSpeed { get => _settings.MinimumHorizontalSpeed; set => _settings.MinimumHorizontalSpeed = value; }
		public float UpRunSpeed { get => _settings.UpRunSpeed; set => _settings.UpRunSpeed = value; }
		public float DownRunSpeed { get => _settings.DownRunSpeed; set => _settings.DownRunSpeed = value; }
		public float TangentialAcceleration { get => _settings.TangentialAcceleration; set => _settings.TangentialAcceleration = value; }
		public float VerticalAcceleration { get => _settings.VerticalAcceleration; set => _settings.VerticalAcceleration = value; }
		public float WallStickSpeed { get => _settings.WallStickSpeed; set => _settings.WallStickSpeed = value; }
		public float HorizontalGravityScale { get => _settings.HorizontalGravityScale; set => _settings.HorizontalGravityScale = value; }
		public float VerticalGravityScale { get => _settings.VerticalGravityScale; set => _settings.VerticalGravityScale = value; }
		public float DownGravityScale { get => _settings.DownGravityScale; set => _settings.DownGravityScale = value; }
		public float Gravity { get => _settings.Gravity; set => _settings.Gravity = value; }
		public float WallJumpAwaySpeed { get => _settings.WallJumpAwaySpeed; set => _settings.WallJumpAwaySpeed = value; }
		public float WallJumpUpSpeed { get => _settings.WallJumpUpSpeed; set => _settings.WallJumpUpSpeed = value; }
		public float DropExitDownSpeed { get => _settings.DropExitDownSpeed; set => _settings.DropExitDownSpeed = value; }
		public float TraversalTransferVelocityScale { get => _settings.TraversalTransferVelocityScale; set => _settings.TraversalTransferVelocityScale = value; }
		public float TraversalTransferUpBias { get => _settings.TraversalTransferUpBias; set => _settings.TraversalTransferUpBias = value; }
		public float MinimumParkourTransferDuration { get => _settings.MinimumParkourTransferDuration; set => _settings.MinimumParkourTransferDuration = value; }

		private readonly PlayerPrefab _prefab;
		private readonly PlayerMovementRuntime _runtime;
		private readonly IPlayerFlagService _flags;
		private readonly WallRunSettings _settings = new();
		private readonly WallRunSurfaceProbe _surfaceProbe;
		private readonly WallRunModeResolver _modeResolver;
		private readonly WallRunMotionSolver _motionSolver;
		private readonly WallRunTraversalBridge _traversalBridge;

		private WallSurfaceCandidate _surface;
		private WallMountedMoveMode _mode = WallMountedMoveMode.None;
		private Vector3 _runDirection = Vector3.Zero;
		private bool _isActive;
		private float _elapsed;
		private float _cooldownRemaining;
		private bool _isDisposed;

		public PlayerWallrunningController(
			PlayerPrefab prefab,
			PlayerMovementRuntime runtime,
			IPlayerFlagService flags,
			IPlayerParkourController parkour,
			IGameEventRegistryService eventFactory
		)
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_runtime = runtime ?? throw new ArgumentNullException( nameof( runtime ) );
			_flags = flags ?? throw new ArgumentNullException( nameof( flags ) );

			WallRunSurfaceClassifier classifier = new( _settings );
			_modeResolver = new WallRunModeResolver( _settings );
			_motionSolver = new WallRunMotionSolver( _settings, _modeResolver );
			_traversalBridge = new WallRunTraversalBridge( _settings, parkour );
			_surfaceProbe = new WallRunSurfaceProbe( _prefab, _settings, classifier );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			Exit( preserveVelocity: true );
			_surfaceProbe.Dispose();
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public void Exit( bool preserveVelocity = true )
		{
			if ( !preserveVelocity ) {
				_prefab.Velocity = Godot.Vector3.Zero;
				_runtime.HorizontalVelocity = Vector3.Zero;
			}

			_isActive = false;
			_mode = WallMountedMoveMode.None;
			_elapsed = 0.0f;
			_cooldownRemaining = _settings.ReattachCooldown;
			_surface = default;
			_runDirection = Vector3.Zero;
			_runtime.ClearWallRun();
			_flags?.RemoveFlags( PlayerFlags.WallRunning );
		}

		public bool TryUpdate( ref PlayerMovementFrame frame, float movementSpeed )
		{
			UpdateCooldown( frame.DeltaTime );

			if ( _isActive ) {
				return UpdateActiveWallRun( ref frame, movementSpeed );
			}

			if ( !CanStartWallRun( ref frame ) ) {
				return false;
			}

			WallRunProbeRequest probeRequest = CreateProbeRequest( ref frame );
			if ( !_surfaceProbe.TryFindBestSurface( probeRequest, out WallSurfaceCandidate candidate ) ) {
				return false;
			}

			WallMountedMoveMode mode = _modeResolver.ResolveMode( frame.Input, frame.WishDirection, candidate );
			if ( mode == WallMountedMoveMode.None || mode == WallMountedMoveMode.Exit ) {
				return false;
			}

			Start( ref frame, candidate, mode );
			return UpdateActiveWallRun( ref frame, movementSpeed );
		}

		private bool UpdateActiveWallRun( ref PlayerMovementFrame frame, float movementSpeed )
		{
			_elapsed += frame.DeltaTime;

			if ( ShouldExitForFloorContact() ) {
				Exit( preserveVelocity: true );
				return false;
			}

			if ( frame.Input.DropPressed || frame.Input.SlidePressed ) {
				ExitWithDropVelocity( ref frame );
				return true;
			}

			if ( _traversalBridge.ShouldRequestTransfer( frame.Input, _mode, _elapsed ) && TryTransferToTraversal( ref frame ) ) {
				return true;
			}

			if ( frame.Input.JumpPressed ) {
				ExitWithWallJumpVelocity( ref frame );
				return true;
			}

			WallRunProbeRequest probeRequest = CreateProbeRequest( ref frame );
			if ( !_surfaceProbe.TryRefreshSurface( _surface, probeRequest, out WallSurfaceCandidate refreshedSurface ) ) {
				if ( _mode == WallMountedMoveMode.WallRunUp && TryTransferToTraversal( ref frame ) ) {
					return true;
				}

				Exit( preserveVelocity: true );
				return false;
			}

			_surface = refreshedSurface;
			frame.SetWallRunState( _surface.Normal, _runDirection, _mode );

			WallMountedMoveMode nextMode = _modeResolver.ResolveMode( frame.Input, frame.WishDirection, _surface );
			if ( nextMode == WallMountedMoveMode.None || nextMode == WallMountedMoveMode.Exit ) {
				Exit( preserveVelocity: true );
				return false;
			}

			_mode = nextMode;
			frame.WallMountedMode = nextMode;

			if ( _elapsed >= _settings.MaximumRunDuration ) {
				if ( TryTransferToTraversal( ref frame ) ) {
					return true;
				}

				Exit( preserveVelocity: true );
				return false;
			}

			ApplyWallRunVelocity( ref frame, movementSpeed );
			return true;
		}

		private void Start( ref PlayerMovementFrame frame, in WallSurfaceCandidate candidate, WallMountedMoveMode mode )
		{
			_surface = candidate;
			_mode = mode;
			_elapsed = 0.0f;
			_isActive = true;
			_cooldownRemaining = 0.0f;
			_runDirection = _modeResolver.ResolveRunDirection( mode, frame.WishDirection, candidate, _runDirection, frame.HorizontalVelocity );

			Vector3 velocity = frame.BodyVelocity;
			if ( velocity.Y < -2.0f && mode != WallMountedMoveMode.WallRunDown ) {
				velocity.Y = -2.0f;
				frame.SetBodyVelocity( velocity );
			}

			frame.SetWallRunState( candidate.Normal, _runDirection, mode );
			_flags?.AddFlags( PlayerFlags.WallRunning );
		}

		private void ApplyWallRunVelocity( ref PlayerMovementFrame frame, float movementSpeed )
		{
			WallRunMotionResult result = _motionSolver.Solve(
				frame.DeltaTime,
				_mode,
				_surface,
				frame.BodyVelocity,
				_runDirection,
				frame.WishDirection,
				frame.HorizontalVelocity,
				movementSpeed
			);

			_runDirection = result.RunDirection;
			frame.SetWallRunState( _surface.Normal, _runDirection, _mode );
			frame.SetFacingDirection( result.FacingDirection, force: true );
			frame.RequestBodyVelocity( result.Velocity, PlayerMovementMode.WallRun );
		}

		private bool CanStartWallRun( ref PlayerMovementFrame frame )
		{
			if ( _cooldownRemaining > 0.0f || frame.IsGrounded ) {
				return false;
			}

			if ( frame.MoveInputLengthSquared < _settings.MinimumInputStrength * _settings.MinimumInputStrength ) {
				return false;
			}

			float requiredSpeedSquared = _settings.MinimumEntrySpeed * _settings.MinimumEntrySpeed;
			if ( frame.HorizontalSpeedSquared < requiredSpeedSquared && frame.WishDirectionLengthSquared < WallRunMath.EPSILON ) {
				return false;
			}

			return true;
		}

		private bool TryTransferToTraversal( ref PlayerMovementFrame frame )
		{
			if ( !_traversalBridge.TryRequestTransfer( frame.Input, frame.BodyVelocity, out Vector3 transferVelocity ) ) {
				return false;
			}

			Vector3 normal = _surface.Normal;
			Vector3 runDirection = _runDirection;
			frame.SetFacingDirection( -normal, force: true );
			frame.RequestBodyVelocity( transferVelocity, PlayerMovementMode.WallRun );

			Exit( preserveVelocity: true );
			_mode = WallMountedMoveMode.ParkourTransfer;
			frame.SetWallRunState( normal, runDirection, WallMountedMoveMode.ParkourTransfer );
			return true;
		}

		private void ExitWithWallJumpVelocity( ref PlayerMovementFrame frame )
		{
			Vector3 tangent = _runDirection.LengthSquared() > WallRunMath.EPSILON ? _runDirection : Vector3.Zero;
			float speed = MathF.Max( MathF.Sqrt( frame.HorizontalSpeedSquared ), _settings.MinimumHorizontalSpeed );
			Vector3 velocity = tangent * speed;

			velocity += _surface.Normal * _settings.WallJumpAwaySpeed;
			velocity.Y = MathF.Max( velocity.Y, _settings.WallJumpUpSpeed );

			frame.SetFacingDirection( WallRunMath.SafePlanarNormalized( velocity, -_surface.Normal ), force: true );
			frame.RequestBodyVelocity( velocity, PlayerMovementMode.WallRun );

			Exit( preserveVelocity: true );
		}

		private void ExitWithDropVelocity( ref PlayerMovementFrame frame )
		{
			Vector3 velocity = WallRunMath.RemovePositiveNormalComponent( frame.BodyVelocity, _surface.Normal );
			velocity += _surface.Normal * 1.25f;
			velocity.Y = MathF.Min( velocity.Y, -_settings.DropExitDownSpeed );

			frame.SetFacingDirection( WallRunMath.SafePlanarNormalized( velocity, -_surface.Normal ), force: true );
			frame.RequestBodyVelocity( velocity, PlayerMovementMode.WallRun );
			Exit( preserveVelocity: true );
		}

		private WallRunProbeRequest CreateProbeRequest( ref PlayerMovementFrame frame )
		{
			return new WallRunProbeRequest(
				frame.WishDirection,
				frame.HorizontalVelocity,
				WallRunMath.SafePlanarNormalized( -_prefab.GlobalTransform.Basis.Z.ToSystem(), -Vector3.UnitZ )
			);
		}

		private bool ShouldExitForFloorContact()
		{
			return _prefab.IsOnFloor() && _elapsed > 0.05f;
		}

		private void UpdateCooldown( float delta )
		{
			if ( _cooldownRemaining <= 0.0f ) {
				return;
			}

			_cooldownRemaining = MathF.Max( 0.0f, _cooldownRemaining - delta );
		}
	};
};
