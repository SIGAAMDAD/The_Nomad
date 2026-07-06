using System;
using Godot;
using Nomad.Core.Input;
using Nomad.Core.Numerics;
using Nomad.Events.Globals;

namespace Nomad.Game.Prefabs
{
	internal sealed partial class PlayerCamera3D : Node3D
	{
		[ExportCategory( "Target" )]
		[Export] public Node3D Target { get; private set; }
		[Export] public Node3D OrbitTarget { get; private set; }
		[Export] public Vector3 TargetOffset { get; private set; } = new Vector3( 0f, 1.6f, 0f );

		[ExportCategory( "Camera" )]
		[Export] public float Distance { get; private set; } = 4.0f;
		[Export] public float ShoulderOffset { get; private set; } = 0.65f;
		[Export] public float VerticalOffset { get; private set; } = 0.15f;

		[ExportCategory( "Mouse Look" )]
		[Export] public float MouseSensitivity { get; private set; } = 0.01f;

		[Export] public float MinPitchDegrees { get; private set; } = -60.0f;
		[Export] public float MaxPitchDegrees { get; private set; } = 45.0f;
		[Export] public bool CaptureMouseOnReady { get; private set; } = true;
		[Export] public string MouseToggleAction { get; private set; } = "ui_cancel";

		[ExportCategory( "Smoothing" )]
		[Export] public float FollowSharpness { get; private set; } = 18.0f;

		[ExportCategory( "Node Paths" )]
		[Export] public NodePath YawPivotPath { get; private set; } = "YawPivot";
		[Export] public NodePath SpringArmPath { get; private set; } = "YawPivot/SpringArm3D";
		[Export] public NodePath CameraPath { get; private set; } = "YawPivot/SpringArm3D/Camera3D";

		private Node3D _yawPivot;
		private SpringArm3D _springArm;
		private Camera3D _camera;
		private IDisposable _mouseMotion;

		private float _yaw;
		private float _pitch;

		public override void _Ready()
		{
			_yawPivot = GetNodeOrNull<Node3D>( YawPivotPath );
			_springArm = GetNodeOrNull<SpringArm3D>( SpringArmPath );
			_camera = GetNodeOrNull<Camera3D>( CameraPath );

			if ( _yawPivot == null || _springArm == null || _camera == null ) {
				GD.PushError( $"{Name}: Camera rig paths are invalid." );
				SetProcess( false );
				SetProcessUnhandledInput( false );
				return;
			}

			_springArm.SpringLength = Distance;
			_springArm.Position = new Vector3( ShoulderOffset, VerticalOffset, 0f );
			_camera.Position = Vector3.Zero;

			TopLevel = true;
			GlobalPosition = Target.GlobalPosition + TargetOffset;
			UpdateOrbitAnchor();
			_camera.Current = true;

			_yaw = _yawPivot.Rotation.Y;
			_pitch = _springArm.Rotation.X;
			ApplyLookRotation();

			if ( CaptureMouseOnReady ) {
				Godot.Input.MouseMode = Godot.Input.MouseModeEnum.Captured;
			}

			_mouseMotion = GameEventRegistry
				.GetEvent<MouseMotionEventArgs>(
					MouseMotionEventArgs.Name,
					MouseMotionEventArgs.NameSpace
				)
				.Subscribe( OnMouseMove );
		}

		public override void _ExitTree()
		{
			_mouseMotion?.Dispose();
		}

		public override void _Process( double delta )
		{
			Vector3 desiredPosition = Target.GlobalPosition + TargetOffset;
			if ( FollowSharpness <= 0.0f ) {
				GlobalPosition = desiredPosition;
				UpdateOrbitAnchor();
				return;
			}

			float t = 1.0f - MathF.Exp( -FollowSharpness * (float)delta );
			GlobalPosition = GlobalPosition.Lerp( desiredPosition, t );
			UpdateOrbitAnchor();
		}

		public override void _UnhandledInput( InputEvent @event )
		{
			if ( !string.IsNullOrEmpty( MouseToggleAction ) && @event.IsActionPressed( MouseToggleAction ) ) {
				Godot.Input.MouseMode = Godot.Input.MouseMode == Godot.Input.MouseModeEnum.Captured
					? Godot.Input.MouseModeEnum.Visible
					: Godot.Input.MouseModeEnum.Captured;
			}
		}

		private void OnMouseMove( in MouseMotionEventArgs args )
		{
			if ( Godot.Input.MouseMode != Godot.Input.MouseModeEnum.Captured ) {
				return;
			}

			_yaw -= args.RelativeX * MouseSensitivity;
			_pitch -= args.RelativeY * MouseSensitivity;

			float minPitchRadians = AngleMath.ToRadians( MinPitchDegrees );
			float maxPitchRadians = AngleMath.ToRadians( MaxPitchDegrees );
			_pitch = Mathf.Clamp( _pitch, minPitchRadians, maxPitchRadians );

			ApplyLookRotation();
		}

		private void ApplyLookRotation()
		{
			_yawPivot.Rotation = new Vector3( 0.0f, _yaw, 0.0f );
			_springArm.Rotation = new Vector3( _pitch, 0.0f, 0.0f );

			_springArm.SpringLength = Distance;
			_springArm.Position = new Vector3( ShoulderOffset, VerticalOffset, 0.0f );
		}

		private void UpdateOrbitAnchor()
		{
			if ( OrbitTarget == null || !IsInstanceValid( OrbitTarget ) ) {
				return;
			}

			_yawPivot.GlobalPosition = OrbitTarget.GlobalPosition;
		}
	};
};
