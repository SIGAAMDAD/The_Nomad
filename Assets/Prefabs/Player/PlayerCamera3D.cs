using System;
using Godot;
using Nomad.Core.Input;
using Nomad.Core.Numerics;
using Nomad.Core.Events;
using Nomad.Events.Globals;

public partial class PlayerCamera3D : Node3D
{
	[ExportCategory( "Target" )]
	[Export] public NodePath TargetPath { get; set; }

	[Export] public Vector3 TargetOffset { get; set; } = new Vector3( 0f, 1.6f, 0f );

	[ExportCategory( "Camera" )]
	[Export] public float Distance { get; set; } = 4.0f;
	[Export] public float ShoulderOffset { get; set; } = 0.65f;
	[Export] public float VerticalOffset { get; set; } = 0.15f;

	[ExportCategory( "Mouse Look" )]
	[Export] public float MouseSensitivity { get; set; } = 0.01f;

	[Export] public float MinPitchDegrees { get; set; } = -60.0f;
	[Export] public float MaxPitchDegrees { get; set; } = 45.0f;
	[Export] public bool CaptureMouseOnReady { get; set; } = true;
	[Export] public string MouseToggleAction { get; set; } = "ui_cancel";

	[ExportCategory( "Smoothing" )]
	[Export] public float FollowSharpness { get; set; } = 18f;

	[ExportCategory( "Node Paths" )]
	[Export] public NodePath YawPivotPath { get; set; } = "YawPivot";
	[Export] public NodePath SpringArmPath { get; set; } = "YawPivot/SpringArm3D";
	[Export] public NodePath CameraPath { get; set; } = "YawPivot/SpringArm3D/Camera3D";

	private Node3D _target;
	private Node3D _yawPivot;
	private SpringArm3D _springArm;
	private Camera3D _camera;
	private IGameEvent<MouseMotionEventArgs> _mouseMotionEvent;

	private float _yaw;
	private float _pitch;

	public override void _Ready()
	{
		_target = GetNodeOrNull<Node3D>( TargetPath );

		if ( _target == null ) {
			GD.PushError( $"{Name}: TargetPath is not assigned or does not point to a Node3D." );
			SetProcess( false );
			SetProcessUnhandledInput( false );
			return;
		}

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
		GlobalPosition = _target.GlobalPosition + TargetOffset;
		_camera.Current = true;

		_yaw = _yawPivot.Rotation.Y;
		_pitch = _springArm.Rotation.X;
		ApplyLookRotation();

		if ( CaptureMouseOnReady ) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		_mouseMotionEvent = GameEventRegistry.GetEvent<MouseMotionEventArgs>(
			MouseMotionEventArgs.Name,
			MouseMotionEventArgs.NameSpace
		);
		_mouseMotionEvent.Subscribe( OnMouseMove );
	}

	public override void _Process( double delta )
	{
		Vector3 desiredPosition = _target.GlobalPosition + TargetOffset;
		if ( FollowSharpness <= 0.0f ) {
			GlobalPosition = desiredPosition;
			return;
		}

		float t = 1.0f - MathF.Exp( -FollowSharpness * (float)delta );
		GlobalPosition = GlobalPosition.Lerp( desiredPosition, t );
	}

	public override void _UnhandledInput( InputEvent @event )
	{
		if ( !string.IsNullOrEmpty( MouseToggleAction ) && @event.IsActionPressed( MouseToggleAction ) ) {
			Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
				? Input.MouseModeEnum.Visible
				: Input.MouseModeEnum.Captured;
		}
	}

	public override void _ExitTree()
	{
		_mouseMotionEvent?.Unsubscribe( OnMouseMove );
	}

	private void OnMouseMove( in MouseMotionEventArgs args )
	{
		if ( Input.MouseMode != Input.MouseModeEnum.Captured ) {
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
}
