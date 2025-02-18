using Godot;
using System;

public partial class Player : CharacterBody2D {
	private enum Hands {
		Left,
		Right,
		Both
	};

	private enum PlayerFlags {
		Sliding			= 0x0001,
		Crouching		= 0x0002,
		BulletTime		= 0x0004,
		Dashing			= 0x0008,
		DemonRage		= 0x0010,
		UsedMana		= 0x0020,
		DemonSight		= 0x0040,
		OnHorse			= 0x0080,
		IdleAnimation	= 0x1000
	};


	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	private AnimatedSprite2D LegAnimation = null;
	private AnimatedSprite2D ArmsLeftAnimation = null;
	private AnimatedSprite2D ArmsRightAnimation = null;
	private AnimatedSprite2D TorsoAnimation = null;
	private Node2D Animations = null;
	private string MoveLeftBind = "move_left_0";
	private string MoveRightBind = "move_right_0";
	private string MoveUpBind = "move_up_0";
	private string MoveDownBind = "move_down_0";
	private string DashBind = "dash_0";
	private string SlideBind = "slide_0";

	// persistant data
	private float Health = 100.0f;
	private float Rage = 0.0f;
	private bool SplitScreen = false;
	private int InputDevice = 0;
	private PlayerFlags Flags = 0;

	private Hands HandsUsed = Hands.Left;
	private int LeftArm = 0;
	private int RightArm = 0;

	public void SetupSplitScreen( int nInputIndex ) {
		GD.Print( "Setting up split-screen input for " + nInputIndex );

		SplitScreen = true;
		InputDevice = nInputIndex;
		MoveLeftBind = "move_left_" + Convert.ToString( InputDevice );
		MoveRightBind = "move_right_" + Convert.ToString( InputDevice );
		MoveUpBind = "move_up_" + Convert.ToString( InputDevice );
		MoveDownBind = "move_down_" + Convert.ToString( InputDevice );
	}
	public void Save( FileAccess file ) {
		file.StoreFloat( Health );
		file.StoreFloat( Rage );
	}
	public void Load( FileAccess file ) {
		
	}

	private void FlipSpriteLeft() {
		LegAnimation.FlipH = true;
		TorsoAnimation.FlipH = true;
		ArmsLeftAnimation.FlipH = true;
		ArmsRightAnimation.FlipH = true;
		Animations.MoveChild( ArmsRightAnimation, 1 );
		Animations.MoveChild( ArmsLeftAnimation, 3 );
	}
	private void FlipSpriteRight() {
		LegAnimation.FlipH = false;
		TorsoAnimation.FlipH = false;
		ArmsLeftAnimation.FlipH = false;
		ArmsRightAnimation.FlipH = false;
		Animations.MoveChild( ArmsLeftAnimation, 1 );
		Animations.MoveChild( ArmsRightAnimation, 3 );
	}

    public override void _Ready() {
		base._Ready();

		Animations = GetNode<Node2D>( "Animations" );
		TorsoAnimation = Animations.GetNode<AnimatedSprite2D>( "Torso" );
		LegAnimation = Animations.GetNode<AnimatedSprite2D>( "Legs" );
		ArmsLeftAnimation = Animations.GetNode<AnimatedSprite2D>( "ArmsLeft" );
		ArmsRightAnimation = Animations.GetNode<AnimatedSprite2D>( "ArmsRight" );
    }

    public override void _Process( double delta ) {
        base._Process( delta );

		if ( SplitScreen ) {
			float direction = Input.GetAxis( MoveLeftBind, MoveRightBind );

			if ( direction > 0.0f ) {
				FlipSpriteRight();
			} else if ( direction < 0.0f ) {
				FlipSpriteLeft();
			}
		} else {
			Godot.Vector2 mousePosition = GetViewport().GetMousePosition();
			Godot.Vector2 screenSize = DisplayServer.ScreenGetSize();

			if ( mousePosition.X > screenSize.X / 2 ) {
				FlipSpriteLeft();
			} else if ( mousePosition.Y < screenSize.X / 2 ) {
				FlipSpriteRight();
			}
		}
    }

    public override void _PhysicsProcess( double delta ) {
		Godot.Vector2 inputDir = Input.GetVector( MoveLeftBind, MoveRightBind, MoveUpBind, MoveDownBind );
		Velocity = inputDir * Speed;

		if ( Velocity != Godot.Vector2.Zero ) {
			LegAnimation.Play( "run" );
			ArmsLeftAnimation.Play( "run" );
			ArmsRightAnimation.Play( "run" );
		} else {
			LegAnimation.Play( "idle" );
			ArmsLeftAnimation.Play( "idle" );
			ArmsRightAnimation.Play( "idle" );
		}

/*
		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
*/

		MoveAndSlide();
	}
}
