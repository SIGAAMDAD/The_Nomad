/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.
===========================================================================
*/

using System;
using Godot;
using Nomad.Core.Numerics;
using Nomad.Game.Application.Gameplay;

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================

	PlayerCamera3D

	===================================================================================
	*/
	/// <summary>
	/// Detached third-person chase camera for the full-3D player controller.
	///
	/// The camera is still authored as a child of Player.tscn, but TopLevel is enabled
	/// at runtime so it can smooth independently from the CharacterBody3D transform.
	/// </summary>

	public partial class PlayerCamera3D : Camera3D
	{
		[Export]
		public NodePath TargetPath { get; set; }

		[Export]
		public float Distance { get; set; } = 92.0f;

		[Export]
		public float Height { get; set; } = 38.0f;

		[Export]
		public float LookAtHeight { get; set; } = 18.0f;

		[Export]
		public float ShoulderOffset { get; set; } = 10.0f;

		[Export]
		public float PositionSharpness { get; set; } = 12.0f;

		[Export]
		public float RotationSharpness { get; set; } = 18.0f;

		[Export]
		public bool EnableMouseLook { get; set; } = true;

		[Export]
		public bool CaptureMouse { get; set; } = true;

		[Export]
		public float MouseSensitivity { get; set; } = 0.0035f;

		[Export]
		public float MinPitchDegrees { get; set; } = -20.0f;

		[Export]
		public float MaxPitchDegrees { get; set; } = 55.0f;

		private float _yaw = 0.0f;
		private float _pitch = 0.0f;

		private Vector2 _joltDirection = Vector2.Zero;
		private float _shakeStrength = 0.0f;
		private Node3D _target;

		private const float SHAKE_FADE = 0.5f;
		private const float DIRECTIONAL_INFLUENCE = 0.7f;
		private const float DEFAULT_FOV = 60.0f;

		public override void _Ready()
		{
			base._Ready();

			Projection = ProjectionType.Perspective;
			Fov = Fov <= 0.0f ? DEFAULT_FOV : Fov;
			Near = Near <= 0.0f ? 0.05f : Near;
			Far = Far <= 0.0f ? 1200.0f : Far;
			Current = true;
			TopLevel = true;

			_target = ResolveTarget();
			if ( _target != null ) {
				_yaw = GetPlanarYaw( -_target.GlobalTransform.Basis.Z );
				SnapToTarget();
			}

			if ( EnableMouseLook && CaptureMouse ) {
				global::Godot.Input.MouseMode = global::Godot.Input.MouseModeEnum.Captured;
			}
		}

		public override void _Input( InputEvent @event )
		{
			base._Input( @event );

			if ( !EnableMouseLook || @event is not InputEventMouseMotion mouseMotion ) {
				return;
			}

			if ( CaptureMouse && global::Godot.Input.MouseMode != global::Godot.Input.MouseModeEnum.Captured ) {
				return;
			}

			_yaw -= mouseMotion.Relative.X * MouseSensitivity;
			_pitch = Mathf.Clamp(
				_pitch - (mouseMotion.Relative.Y * MouseSensitivity),
				Mathf.DegToRad( MinPitchDegrees ),
				Mathf.DegToRad( MaxPitchDegrees )
			);
		}

		public override void _PhysicsProcess( double delta )
		{
			base._PhysicsProcess( delta );

			_target ??= ResolveTarget();
			if ( _target == null ) {
				return;
			}

			UpdateCameraTransform( (float)delta );
		}

		public override void _Process( double delta )
		{
			base._Process( delta );

			if ( _shakeStrength > 0.0f ) {
				_shakeStrength = Interpolation.Lerp( _shakeStrength, 0.0f, SHAKE_FADE * (float)delta );

				Vector2 offset = _joltDirection != Vector2.Zero
					? _joltDirection.Normalized() * _shakeStrength * DIRECTIONAL_INFLUENCE
					: new Vector2(
						RNJesus.FloatRange( -_shakeStrength, _shakeStrength ),
						RNJesus.FloatRange( -_shakeStrength, _shakeStrength )
					);

				HOffset = offset.X;
				VOffset = offset.Y;
			} else {
				HOffset = 0.0f;
				VOffset = 0.0f;
			}
		}

		private Node3D ResolveTarget()
		{
			if ( TargetPath != null && !TargetPath.IsEmpty ) {
				return GetNodeOrNull<Node3D>( TargetPath );
			}

			return GetParentOrNull<Node3D>();
		}

		private void SnapToTarget()
		{
			Vector3 desiredPosition = CalculateDesiredPosition();
			Vector3 lookAtPosition = CalculateLookAtPosition();

			GlobalPosition = desiredPosition;
			LookAt( lookAtPosition, Vector3.Up );
		}

		private void UpdateCameraTransform( float delta )
		{
			Vector3 desiredPosition = CalculateDesiredPosition();
			Vector3 lookAtPosition = CalculateLookAtPosition();

			float positionWeight = 1.0f - Mathf.Exp( -PositionSharpness * delta );
			GlobalPosition = GlobalPosition.Lerp( desiredPosition, positionWeight );

			Transform3D desiredTransform = GlobalTransform.LookingAt( lookAtPosition, Vector3.Up );
			float rotationWeight = 1.0f - Mathf.Exp( -RotationSharpness * delta );
			GlobalTransform = new Transform3D(
				GlobalTransform.Basis.Slerp( desiredTransform.Basis, rotationWeight ),
				GlobalTransform.Origin
			);
		}

		private Vector3 CalculateDesiredPosition()
		{
			Vector3 targetPosition = _target.GlobalPosition;
			Vector3 forward = CalculateOrbitForward();
			Vector3 right = new Vector3( forward.Z, 0.0f, -forward.X );

			float planarDistance = Distance * Mathf.Cos( _pitch );
			float pitchHeight = Distance * Mathf.Sin( _pitch );

			return targetPosition
				+ (Vector3.Up * (Height + pitchHeight))
				- (forward * planarDistance)
				+ (right * ShoulderOffset);
		}

		private Vector3 CalculateLookAtPosition()
		{
			return _target.GlobalPosition + (Vector3.Up * LookAtHeight);
		}

		private Vector3 CalculateOrbitForward()
		{
			return new Vector3( Mathf.Sin( _yaw ), 0.0f, -Mathf.Cos( _yaw ) ).Normalized();
		}

		private static float GetPlanarYaw( Vector3 forward )
		{
			forward.Y = 0.0f;
			if ( forward.LengthSquared() <= 0.0001f ) {
				return 0.0f;
			}

			forward = forward.Normalized();
			return MathF.Atan2( forward.X, -forward.Z );
		}
	}
}
