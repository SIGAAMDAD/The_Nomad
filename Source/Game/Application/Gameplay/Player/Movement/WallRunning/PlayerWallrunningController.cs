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
using Nomad.Game.Application.Gameplay.Player.Movement.WallRunning;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Player.State;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerWallrunningController

	===================================================================================
	*/
	/// <summary>
	/// State-machine/orchestration layer for dynamic wall-mounted locomotion.
	/// Surface probing, group classification, input-mode selection, velocity solving, and
	/// traversal handoff live in focused collaborators under Movement/WallRunning.
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
			_flags = flags;

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

		public void SetParkourController( IPlayerParkourController parkour )
		{
			_traversalBridge.SetParkourController( parkour );
		}

		public void Exit( bool preserveVelocity = true )
		{
			if ( !preserveVelocity ) {
				_prefab.Velocity = Vector3.Zero;
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

		public bool TryUpdate(
			float delta,
			in PlayerInputFrame input,
			Vector3 planarWishDirection,
			float movementSpeed,
			out Vector3 facingDirection
		)
		{
			facingDirection = Vector3.Zero;
			UpdateCooldown( delta );

			if ( _isActive ) {
				return UpdateActiveWallRun( delta, input, planarWishDirection, movementSpeed, out facingDirection );
			}

			if ( !CanStartWallRun( input, planarWishDirection ) ) {
				return false;
			}

			WallRunProbeRequest probeRequest = CreateProbeRequest( planarWishDirection );
			if ( !_surfaceProbe.TryFindBestSurface( probeRequest, out WallSurfaceCandidate candidate ) ) {
				return false;
			}

			WallMountedMoveMode mode = _modeResolver.ResolveMode( input, planarWishDirection, candidate );
			if ( mode == WallMountedMoveMode.None || mode == WallMountedMoveMode.Exit ) {
				return false;
			}

			Start( candidate, mode, planarWishDirection );
			return UpdateActiveWallRun( delta, input, planarWishDirection, movementSpeed, out facingDirection );
		}

		private bool UpdateActiveWallRun(
			float delta,
			in PlayerInputFrame input,
			Vector3 planarWishDirection,
			float movementSpeed,
			out Vector3 facingDirection
		)
		{
			facingDirection = Vector3.Zero;
			_elapsed += delta;

			if ( ShouldExitForFloorContact() ) {
				Exit( preserveVelocity: true );
				return false;
			}

			if ( input.DropPressed || input.SlidePressed ) {
				ExitWithDropVelocity();
				return true;
			}

			if ( _traversalBridge.ShouldRequestTransfer( input, _mode, _elapsed ) && TryTransferToTraversal( input, out facingDirection ) ) {
				return true;
			}

			if ( input.JumpPressed ) {
				ExitWithWallJumpVelocity();
				facingDirection = WallRunMath.SafePlanarNormalized( _prefab.Velocity, -_surface.Normal );
				return true;
			}

			WallRunProbeRequest probeRequest = CreateProbeRequest( planarWishDirection );
			if ( !_surfaceProbe.TryRefreshSurface( _surface, probeRequest, out WallSurfaceCandidate refreshedSurface ) ) {
				if ( _mode == WallMountedMoveMode.WallRunUp && TryTransferToTraversal( input, out facingDirection ) ) {
					return true;
				}

				Exit( preserveVelocity: true );
				return false;
			}

			_surface = refreshedSurface;
			_runtime.WallNormal = _surface.Normal;

			WallMountedMoveMode nextMode = _modeResolver.ResolveMode( input, planarWishDirection, _surface );
			if ( nextMode == WallMountedMoveMode.None || nextMode == WallMountedMoveMode.Exit ) {
				Exit( preserveVelocity: true );
				return false;
			}

			_mode = nextMode;
			_runtime.WallMountedMode = nextMode;

			if ( _elapsed >= _settings.MaximumRunDuration ) {
				if ( TryTransferToTraversal( input, out facingDirection ) ) {
					return true;
				}

				Exit( preserveVelocity: true );
				return false;
			}

			ApplyWallRunVelocity( delta, planarWishDirection, movementSpeed, out facingDirection );
			return true;
		}

		private void Start( in WallSurfaceCandidate candidate, WallMountedMoveMode mode, Vector3 planarWishDirection )
		{
			_surface = candidate;
			_mode = mode;
			_elapsed = 0.0f;
			_isActive = true;
			_cooldownRemaining = 0.0f;
			_runDirection = _modeResolver.ResolveRunDirection( mode, planarWishDirection, candidate, _runDirection, _runtime.HorizontalVelocity );

			Vector3 velocity = _prefab.Velocity;
			if ( velocity.Y < -2.0f && mode != WallMountedMoveMode.WallRunDown ) {
				velocity.Y = -2.0f;
				_prefab.Velocity = velocity;
			}

			_runtime.WallNormal = candidate.Normal;
			_runtime.WallRunDirection = _runDirection;
			_runtime.WallMountedMode = mode;
			_flags?.AddFlags( PlayerFlags.WallRunning );
		}

		private void ApplyWallRunVelocity(
			float delta,
			Vector3 planarWishDirection,
			float movementSpeed,
			out Vector3 facingDirection
		)
		{
			WallRunMotionResult result = _motionSolver.Solve(
				delta,
				_mode,
				_surface,
				_prefab.Velocity,
				_runDirection,
				planarWishDirection,
				_runtime.HorizontalVelocity,
				movementSpeed
			);

			_runDirection = result.RunDirection;
			_runtime.WallRunDirection = _runDirection;

			_prefab.Velocity = result.Velocity;
			_prefab.MoveAndSlide();

			Vector3 corrected = WallRunMath.RemovePositiveNormalComponent( _prefab.Velocity, _surface.Normal );
			_runtime.HorizontalVelocity = new Vector3( corrected.X, 0.0f, corrected.Z );
			facingDirection = result.FacingDirection;
		}

		private bool CanStartWallRun( in PlayerInputFrame input, Vector3 planarWishDirection )
		{
			if ( _cooldownRemaining > 0.0f || _prefab.IsOnFloor() ) {
				return false;
			}

			if ( input.Move.LengthSquared() < _settings.MinimumInputStrength * _settings.MinimumInputStrength ) {
				return false;
			}

			float currentHorizontalSpeedSquared = _prefab.Velocity.X * _prefab.Velocity.X + _prefab.Velocity.Z * _prefab.Velocity.Z;
			float runtimeSpeedSquared = _runtime.HorizontalVelocity.LengthSquared();
			float requiredSpeedSquared = _settings.MinimumEntrySpeed * _settings.MinimumEntrySpeed;
			if ( currentHorizontalSpeedSquared < requiredSpeedSquared && runtimeSpeedSquared < requiredSpeedSquared && planarWishDirection.LengthSquared() < WallRunMath.Epsilon ) {
				return false;
			}

			return true;
		}

		private bool TryTransferToTraversal( in PlayerInputFrame input, out Vector3 facingDirection )
		{
			facingDirection = Vector3.Zero;

			if ( !_traversalBridge.TryRequestTransfer( input, _prefab.Velocity, out Vector3 transferVelocity ) ) {
				return false;
			}

			_prefab.Velocity = transferVelocity;
			_runtime.HorizontalVelocity = new Vector3( transferVelocity.X, 0.0f, transferVelocity.Z );
			facingDirection = -_surface.Normal;

			Exit( preserveVelocity: true );
			_mode = WallMountedMoveMode.ParkourTransfer;
			_runtime.WallMountedMode = WallMountedMoveMode.ParkourTransfer;
			return true;
		}

		private void ExitWithWallJumpVelocity()
		{
			Vector3 tangent = _runDirection.LengthSquared() > WallRunMath.Epsilon ? _runDirection : Vector3.Zero;
			float speed = MathF.Max( _runtime.HorizontalVelocity.Length(), _settings.MinimumHorizontalSpeed );
			Vector3 velocity = tangent * speed;
			velocity += _surface.Normal * _settings.WallJumpAwaySpeed;
			velocity.Y = MathF.Max( velocity.Y, _settings.WallJumpUpSpeed );

			_prefab.Velocity = velocity;
			_runtime.HorizontalVelocity = new Vector3( velocity.X, 0.0f, velocity.Z );
			Exit( preserveVelocity: true );
		}

		private void ExitWithDropVelocity()
		{
			Vector3 velocity = WallRunMath.RemovePositiveNormalComponent( _prefab.Velocity, _surface.Normal );
			velocity += _surface.Normal * 1.25f;
			velocity.Y = MathF.Min( velocity.Y, -_settings.DropExitDownSpeed );

			_prefab.Velocity = velocity;
			_runtime.HorizontalVelocity = new Vector3( velocity.X, 0.0f, velocity.Z );
			Exit( preserveVelocity: true );
		}

		private WallRunProbeRequest CreateProbeRequest( Vector3 planarWishDirection )
		{
			return new WallRunProbeRequest(
				planarWishDirection,
				_runtime.HorizontalVelocity,
				WallRunMath.SafePlanarNormalized( -_prefab.GlobalTransform.Basis.Z, Vector3.Forward )
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
	}
}
