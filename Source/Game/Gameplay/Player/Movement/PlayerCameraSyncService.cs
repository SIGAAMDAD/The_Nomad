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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Numerics;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerCameraSyncService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerCameraSyncService
	{
		private const float RETURN_DURATION = 0.8f;

		private enum SyncState : byte
		{
			Idle,
			Orbiting,
			Returning
		};

		private SyncState _state = SyncState.Idle;

		private float _elapsed;

		private Transform3D _startTransform;
		private Vector3 _startPosition;
		private Quaternion _startRotation;

		private Vector3 _returnStartPosition;
		private Quaternion _returnStartRotation;

		private float _startYaw;
		private float _startRadius;
		private float _startHeight;

		private readonly PlayerCamera3D _camera;

		private SyncPoint? _activeSync = null;

		public PlayerCameraSyncService( PlayerCamera3D camera )
		{
			_camera = camera ?? throw new ArgumentNullException( nameof( camera ) );
		}

		public void BeginSync( SyncPoint syncPoint )
		{
			if ( _state != SyncState.Idle ) {
				return;
			}

			ArgumentGuard.ThrowIfNull( syncPoint, nameof( syncPoint ) );

			_activeSync = syncPoint;

			_startTransform = syncPoint.GlobalTransform;
			_startPosition = syncPoint.GlobalPosition;
			_startRotation = syncPoint.GlobalTransform.Basis.GetRotationQuaternion();

			Vector3 focus = GetFocusPoint();
			Vector3 flatOffset = _startPosition - focus;
			flatOffset.Y = 0.0f;

			_startRadius = flatOffset.Length();

			if ( _startRadius < 0.05f ) {
				_startRadius = syncPoint.OrbitRadius;
				_startYaw = syncPoint.GlobalRotation.Y;
			} else {
				_startYaw = MathF.Atan2( flatOffset.X, flatOffset.Z );
			}

			_startHeight = _startPosition.Y - focus.Y;

			_elapsed = 0.0f;
			_state = SyncState.Orbiting;

			_camera.GetTree().ProcessFrame += ProcessFrame;
		}

		public void EndSyncEarly()
		{
			if ( _state == SyncState.Idle ) {
				return;
			}

			BeginReturnOrFinish();
		}

		private void ProcessFrame()
		{
			if ( _state == SyncState.Idle ) {
				return;
			}

			float dt = (float)_camera.GetProcessDeltaTime();

			switch ( _state ) {
				case SyncState.Orbiting:
					ProcessOrbit( dt );
					break;
				case SyncState.Returning:
					ProcessReturn( dt );
					break;
			}
		}

		private void ProcessOrbit( float delta )
		{
			_elapsed += delta;

			float duration = MathF.Max( _activeSync.SyncDuration, 0.001f );
			float t = Math.Clamp( _elapsed / duration, 0f, 1f );
			float eased = EaseInOutCubic( t );

			Vector3 focus = GetFocusPoint();

			float orbitRadians = AngleMath.ToRadians( _activeSync.OrbitDegrees );
			float angle = _startYaw + orbitRadians * eased;

			float midArc = MathF.Sin( t * MathF.PI );

			float radius = AngleMath.LerpRadians( _startRadius, _activeSync.OrbitRadius, eased );
			radius += _activeSync.MidOrbitExtraDistance * midArc;

			float height = AngleMath.LerpRadians( _startHeight, _activeSync.OrbitHeight, eased );
			height += _activeSync.MidOrbitExtraHeight * midArc;

			Vector3 orbitOffset = new Vector3(
				MathF.Sin( angle ) * radius,
				height,
				MathF.Cos( angle ) * radius
			);

			_camera.GlobalPosition = focus + orbitOffset;
			_camera.LookAt( focus, Vector3.Up );

			if ( t >= 1.0f ) {
				BeginReturnOrFinish();
			}
		}

		private void BeginReturnOrFinish()
		{
			_returnStartPosition = _camera.GlobalPosition;
			_returnStartRotation = _camera.GlobalTransform.Basis.GetRotationQuaternion();

			_elapsed = 0.0f;
			_state = SyncState.Returning;
		}

		private void ProcessReturn( float delta )
		{
			_elapsed += delta;

			float duration = MathF.Max( RETURN_DURATION, 0.001f );
			float t = Math.Clamp( _elapsed / duration, 0f, 1f );
			float eased = EaseInOutCubic( t );

			Vector3 position = _returnStartPosition.Lerp( _startPosition, eased );
			Quaternion rotation = _returnStartRotation.Slerp( _startRotation, eased );

			_camera.GlobalTransform = new Transform3D( new Basis( rotation ), position );

			if ( t >= 1.0f ) {
				_camera.GlobalTransform = _startTransform;
				FinishSync();
			}
		}

		private void FinishSync()
		{
			_elapsed = 0.0f;
			_state = SyncState.Idle;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private Vector3 GetFocusPoint()
		{
			return _camera.Target.GlobalPosition + _activeSync.FocusOffset;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static float EaseInOutCubic( float t )
		{
			return t < 0.5f
				? 4.0f * t * t * t
				: 1.0f - MathF.Pow( -2.0f * t + 2.0f, 3.0f ) / 2.0f;
		}
	};
};
