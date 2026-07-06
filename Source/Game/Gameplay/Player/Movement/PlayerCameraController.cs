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

using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Prefabs;
using System;
using System.Numerics;
using Nomad.Core.Numerics;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.EngineUtils;

namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerCameraController

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerCameraController : IPlayerCameraController, IDisposable
	{
		public const float FACING_VELOCITY_THRESHOLD_SQUARED = (16.0f / PlayerMovementController.GAMEPLAY_UNITS_PER_WORLD_UNIT) * (16.0f / PlayerMovementController.GAMEPLAY_UNITS_PER_WORLD_UNIT);

		private const string VISUAL_ROOT_PATH = "VisualRoot";
		private const float EPSILON = 0.0001f;
		private const float TURN_SPEED = 14.0f;

		private readonly Godot.Node3D _visualRoot;
		private readonly float _visualYawOffset;

		private readonly PlayerCamera3D _camera;
		private readonly PlayerPrefab _prefab;
		private readonly PlayerMovementRuntime _runtime;

		private readonly PlayerCameraSyncService _syncService;

		public IGameEvent<PlayerCameraStatusChangedEventArgs> CameraStatusChanged => _cameraStatusChanged;
		private readonly IGameEvent<PlayerCameraStatusChangedEventArgs> _cameraStatusChanged = null;

		private bool _isDisposed = false;

		/*
		===============
		PlayerCameraController
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="prefab"></param>
		/// <param name="camera"></param>
		/// <param name="runtime"></param>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public PlayerCameraController(
			PlayerPrefab prefab,
			PlayerCamera3D camera,
			PlayerMovementRuntime runtime,
			IGameEventRegistryService eventFactory
		)
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_camera = camera ?? throw new ArgumentNullException( nameof( camera ) );
			_runtime = runtime ?? throw new ArgumentNullException( nameof( runtime ) );

			_syncService = new PlayerCameraSyncService( camera );

			_visualRoot = _prefab.GetNodeOrNull<Godot.Node3D>( VISUAL_ROOT_PATH ) ?? _prefab;
			_visualYawOffset = _visualRoot.Rotation.Y;

			_cameraStatusChanged = eventFactory
				.GetEvent<PlayerCameraStatusChangedEventArgs>(
					PlayerCameraStatusChangedEventArgs.Name,
					PlayerCameraStatusChangedEventArgs.NameSpace
				);
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_cameraStatusChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		GetLookFacingDirection
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="fallbackDirection"></param>
		/// <returns></returns>
		public Vector3 GetLookFacingDirection( Vector3 fallbackDirection )
		{
			Vector3 forward = GetCameraPlanarForward();
			if ( forward.LengthSquared() > EPSILON ) {
				return forward;
			}

			if ( fallbackDirection.LengthSquared() > EPSILON ) {
				return Vector3.Normalize( fallbackDirection );
			}

			return ResolveActionDirection( Vector3.Zero );
		}

		/*
		===============
		GetCameraPlanarForward
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private Vector3 GetCameraPlanarForward()
		{
			var basisZ = -_camera.GlobalTransform.Basis.Z.ToSystem();

			Vector3 forward = basisZ;
			forward.Y = 0.0f;

			if ( forward.LengthSquared() <= EPSILON ) {
				forward = basisZ;
				forward.Y = 0.0f;
			}

			return forward.LengthSquared() > EPSILON ? Vector3.Normalize( forward ) : new Vector3( 0.0f, 0.0f, -1.0f );
		}

		/*
		===============
		GetWishDirection
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="moveInput"></param>
		/// <returns></returns>
		public Vector3 GetWishDirection( System.Numerics.Vector2 moveInput )
		{
			if ( moveInput.LengthSquared() <= EPSILON ) {
				return Vector3.Zero;
			}

			Vector3 forward = GetCameraPlanarForward();
			Vector3 right = GetCameraPlanarRight();
			Vector3 direction = (right * moveInput.X) + (forward * moveInput.Y);
			direction.Y = 0.0f;

			return direction.LengthSquared() > EPSILON ? Vector3.Normalize( direction ) : Vector3.Zero;
		}

		/*
		===============
		GetCameraPlanarRight
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private Vector3 GetCameraPlanarRight()
		{
			Vector3 right = _camera.GlobalTransform.Basis.X.ToSystem();
			right.Y = 0.0f;
			return right.LengthSquared() > EPSILON ? Vector3.Normalize( right ) : new Vector3( 1.0f, 0.0f, 0.0f );
		}

		/*
		===============
		ResolveActionDirection
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="wishDirection"></param>
		/// <returns></returns>
		public Vector3 ResolveActionDirection( Vector3 wishDirection )
		{
			if ( wishDirection.LengthSquared() > EPSILON ) {
				return Vector3.Normalize( wishDirection );
			}

			if ( _runtime.HorizontalVelocity.LengthSquared() > EPSILON ) {
				return Vector3.Normalize( _runtime.HorizontalVelocity );
			}

			if ( _runtime.LastPlanarWishDirection.LengthSquared() > EPSILON ) {
				return Vector3.Normalize( _runtime.LastPlanarWishDirection );
			}

			Vector3 forward = -_visualRoot.GlobalTransform.Basis.Z.ToSystem();
			forward.Y = 0.0f;
			return forward.LengthSquared() > EPSILON ? Vector3.Normalize( forward ) : new Vector3( 0.0f, 0.0f, -1.0f );
		}

		/*
		===============
		UpdateFacing
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="wishDirection"></param>
		public void UpdateFacing( float delta, Vector3 wishDirection )
		{
			Vector3 facingDirection = Vector3.Zero;

			if ( wishDirection.LengthSquared() > EPSILON ) {
				facingDirection = wishDirection;
			} else if ( _runtime.HorizontalVelocity.LengthSquared() > FACING_VELOCITY_THRESHOLD_SQUARED ) {
				facingDirection = Vector3.Normalize( _runtime.HorizontalVelocity );
			}

			if ( facingDirection.LengthSquared() <= EPSILON ) {
				return;
			}

			float desiredYaw = MathF.Atan2( -facingDirection.X, -facingDirection.Z );
			desiredYaw += _visualYawOffset;

			Godot.Vector3 rotation = _visualRoot.Rotation;
			rotation.Y = AngleMath.LerpRadians( rotation.Y, desiredYaw, MathF.Min( 1.0f, TURN_SPEED * delta ) );
			_visualRoot.Rotation = rotation;

			_cameraStatusChanged.Publish(
				new PlayerCameraStatusChangedEventArgs(
					playerId: _prefab.PeerId,
					origin: _camera.GlobalPosition.ToSystem(),
					forward: -_camera.GlobalBasis.Z.ToSystem(),
					right: _camera.GlobalBasis.X.ToSystem(),
					pitch: rotation.Y,
					yaw: rotation.X,
					roll: rotation.Z
				)
			);
		}

		/*
		===============
		BeginSynchronization
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void BeginSynchronization()
		{
		}
	};
};
