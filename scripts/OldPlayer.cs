using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using PlayerSystem;

public partial class OldPlayer : CharacterBody2D {
	public enum Hands : byte {
		Left,
		Right,
		Both
	};

	private enum PlayerFlags : uint {
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

	public enum AnimationState : byte {
		Idle,
		Move,
		Melee
	};

	private const byte MAX_WEAPON_SLOTS = 8;

	private const float ACCEL = 1500;
	private const float FRICTION = 1400.0f;
	private const float MAX_SPEED = 400.0f;
	private const float JUMP_VELOCITY = -400.0f;

	private Random RandomFactory = new Random();

	private GpuParticles2D WalkEffect;
	private GpuParticles2D SlideEffect;
	private GpuParticles2D DashEffect;
	private AnimatedSprite2D JumpkitSparks;

	private PackedScene BloodSplatter;

	private Resource SwitchToKeyboard;
	private Resource SwitchToGamepad;
	private Resource KeyboardInputMapping;
	private Resource GamepadInputMapping;

	private Resource MoveActionGamepad;
	private Resource DashActionGamepad;
	private Resource SlideActionGamepad;
	private Resource UseWeaponActionGamepad;
	private Resource NextWeaponActionGamepad;
	private Resource PrevWeaponActionGamepad;
	private Resource SwitchWeaponModeActionGamepad;
	private Resource BulletTimeActionGamepad;
	private Resource OpenInventoryActionGamepad;
	private Resource ArmAngleActionGamepad;

	private Resource MoveActionKeyboard;
	private Resource DashActionKeyboard;
	private Resource SlideActionKeyboard;
	private Resource UseWeaponActionKeyboard;
	private Resource NextWeaponActionKeyboard;
	private Resource PrevWeaponActionKeyboard;
	private Resource SwitchWeaponModeActionKeyboard;
	private Resource BulletTimeActionKeyboard;
	private Resource OpenInventoryActionKeyboard;

	private Resource MoveAction;
	private Resource DashAction;
	private Resource SlideAction;
	private Resource UseWeaponAction;
	private Resource NextWeaponAction;
	private Resource PrevWeaponAction;
	private Resource SwitchWeaponModeAction;
	private Resource BulletTimeAction;
	private Resource OpenInventoryAction;
	private Resource DemonEyeAction;

	private Timer IdleTimer;
	private AnimatedSprite2D IdleAnimation;

	private AnimatedSprite2D LegAnimation;
	public AnimatedSprite2D TorsoAnimation;
	private Node2D Animations;
	private Camera2D Camera;
	private PlayerSystem.Arm ArmLeft;
	private PlayerSystem.Arm ArmRight;
	private Node2D HUD;
	private CollisionShape2D Collision; // only used with horsy
	private Vector2 DashDirection = Godot.Vector2.Zero;
	private Timer DashCooldown;
	private Timer DashTime;
	private Timer SlideTime;

	private List<Resource> ConsumableStacks;
	private List<Resource> WeaponStacks;
	private List<Resource> AmmoPelletStacks;
	private List<Resource> AmmoHeavyStacks;
	private List<Resource> AmmoLightStacks;
	private Node Inventory;

	private AudioStreamPlayer2D ChangeWeaponSfx;
	private List<AudioStreamPlayer2D> PainSfx;
	private List<AudioStreamPlayer2D> DieSfx;
	private List<AudioStreamPlayer2D> DeathSfx;
	private List<AudioStreamPlayer2D> MoveGravelSfx;
	private List<AudioStreamPlayer2D> DashSfx;
	private List<AudioStreamPlayer2D> SlideSfx;
	private AudioStreamPlayer2D SlowMoBeginSfx;
	private AudioStreamPlayer2D SlowMoEndSfx;

	// persistent data
	private bool SplitScreen = false;
	private int InputDevice = 0;
	private float Health = 100.0f;
	private float Rage = 0.0f;
	private PlayerFlags Flags = 0;
	private int Hellbreaks = 0;
	private byte CurrentWeapon = 0;
	private List<PlayerSystem.WeaponSlot> WeaponSlots;

	// multiplayer data
	public ulong MultiplayerId = 0; // literally just a steamID
	public string MultiplayerUsername = ""; // steam username
	public uint MultiplayerKills = 0;
	public uint MultiplayerDeaths = 0;
	public double MultiplayerHilltime = 0.0f;

	private Vector2 LastMousePosition = Godot.Vector2.Zero;
	private Vector2 InputVelocity = Godot.Vector2.Zero;
	private Hands HandsUsed = Hands.Left;
	private PlayerSystem.Arm LastUsedArm = null;

	private float FrameDamage = 0.0f;
	private float ArmAngle = 0.0f;

	[Signal]
	public delegate void DieEventHandler( CharacterBody2D Attacker, CharacterBody2D Target );
	[Signal]
	public delegate void DamagedEventHandler( CharacterBody2D Attacker, CharacterBody2D Target, float nAmount );

	public void SetupSplitScreen( int nInputIndex ) {
		GetNode( "/root/Console" ).Call( "print_line", "Setting up split-screen input for " + nInputIndex );

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			SplitScreen = true;
		}
		InputDevice = nInputIndex;
	}

	public PlayerSystem.Arm GetWeaponHand( WeaponEntity weapon ) {
		if ( WeaponSlots[ ArmLeft.GetSlot() ].GetWeapon() == weapon ) {
			return ArmLeft;
		} else if ( WeaponSlots[ ArmRight.GetSlot() ].GetWeapon() == weapon ) {
			return ArmRight;
		}
		// not equipped
		return null;
	}

	private void IdleReset() {
		IdleTimer.Start();
		IdleAnimation.Hide();
		IdleAnimation.Stop();

		LegAnimation.Show();
		TorsoAnimation.Show();
		ArmRight.Animations.Show();
		ArmLeft.Animations.Show();
	}

	private void PlaySfx( AudioStreamPlayer2D sfx ) {
		sfx.Play();
	}

    public override void _ExitTree() {
        base._ExitTree();

		TorsoAnimation.QueueFree();
		LegAnimation.QueueFree();
		ArmLeft.Animations.QueueFree();
		ArmRight.Animations.QueueFree();
		ArmLeft.QueueFree();
		ArmRight.QueueFree();

		IdleTimer.QueueFree();
		IdleAnimation.QueueFree();
		Inventory.QueueFree();
		HUD.QueueFree();

		QueueFree();
    }

	private void OnDeath( CharacterBody2D Attacker ) {
		if ( (bool)( GetNode( "/root/SettingsData" ).Get( "_hellbreaker" ) ) ) {
			GetTree().CurrentScene.Call( "ToggleHellbreaker" );
		}

		EmitSignal( "Die", Attacker, this );
		LegAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();

		TorsoAnimation.Play( "death" );
	}

	public void Damage( CharacterBody2D Attacker, float nAmount ) {
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			return; // iframes
		}
		EmitSignal( "Damaged", Attacker, this, nAmount );

		Health -= nAmount;
		Rage += nAmount;

		Node2D blood = BloodSplatter.Instantiate<Node2D>();
		blood.GlobalPosition = GlobalPosition;
		AddChild( blood );

		if ( Health <= 0.0f ) {
			OnDeath( Attacker );
		} else {
			PlaySfx( PainSfx[ RandomFactory.Next( 0, PainSfx.Count - 1 ) ] );
		}
	}

	public void Save( FileAccess file ) {
		file.StoreFloat( GlobalPosition.X );
		file.StoreFloat( GlobalPosition.Y );

	}
	public void Load( FileAccess file ) {
	}

    public Godot.Vector2 GetInputVelocity() {
		return InputVelocity;
	}

	public PlayerSystem.WeaponSlot GetSlot( int nSlot ) {
		return WeaponSlots[ nSlot ];
	}
	public AnimatedSprite2D GetTorsoAnimation() {
		return TorsoAnimation;
	}
	public AnimatedSprite2D GetLegAnimation() {
		return LegAnimation;
	}
	public Node GetInventory() {
		return Inventory;
	}

	public float GetArmAngle() {
		return ArmAngle;
	}

	public PlayerSystem.Arm GetLastUsedArm() {
		return LastUsedArm;
	}
	public void SetLastUsedArm( PlayerSystem.Arm arm ) {
		LastUsedArm = arm;
	}

	public Hands GetHandsUsed() {
		return HandsUsed;
	}
	public void SetHandsUsed( Hands hands ) {
		HandsUsed = hands;
	}

	public PlayerSystem.Arm GetLeftArm() {
		return ArmLeft;
	}
	public PlayerSystem.Arm GetRightArm() {
		return ArmRight;
	}


	private void FlipSpriteLeft() {
		LegAnimation.FlipH = true;
		TorsoAnimation.FlipH = true;
		ArmLeft.Flip = true;
		ArmRight.Flip = true;
	}
	private void FlipSpriteRight() {
		LegAnimation.FlipH = false;
		TorsoAnimation.FlipH = false;
		ArmLeft.Flip = false;
		ArmRight.Flip = false;
	}

	private void LoadGamepadBinds() {
		MoveActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/move_player0.tres" );
		DashActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/dash_player0.tres" );
		SlideActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/slide_player0.tres" );
		UseWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/use_weapon_player0.tres" );
		NextWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/next_weapon_player0.tres" );
		PrevWeaponActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/prev_weapon_player0.tres" );
		SwitchWeaponModeActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_weapon_mode_player0.tres" );
		BulletTimeActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/bullet_time_player0.tres" );
		OpenInventoryActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/open_inventory_player0.tres" );
		ArmAngleActionGamepad = ResourceLoader.Load( "res://resources/binds/actions/gamepad/arm_angle.tres" );
	}
	private void LoadKeyboardBinds() {
		MoveActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/move.tres" );
		DashActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/dash.tres" );
		SlideActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/slide.tres" );
		UseWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/use_weapon.tres" );
		NextWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/next_weapon.tres" );
		PrevWeaponActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/prev_weapon.tres" );
		SwitchWeaponModeActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_weapon_mode.tres" );
		BulletTimeActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/bullet_time.tres" );
		OpenInventoryActionKeyboard = ResourceLoader.Load( "res://resources/binds/actions/keyboard/open_inventory.tres" );
	}
	private void SwitchInputMode( Resource InputContext ) {
		( (Node)Get( "/root/GUIDE" ) ).Call( "enable_mapping_context", InputContext );

		if ( InputContext == KeyboardInputMapping ) {
			MoveAction = MoveActionKeyboard;
			DashAction = DashActionKeyboard;
			SlideAction = SlideActionKeyboard;
			BulletTimeAction = BulletTimeActionKeyboard;
			PrevWeaponAction = PrevWeaponActionKeyboard;
			NextWeaponAction = NextWeaponActionKeyboard;
			SwitchWeaponModeAction = SwitchWeaponModeActionKeyboard;
			UseWeaponAction = UseWeaponActionKeyboard;
		} else {
			MoveAction = MoveActionGamepad;
			DashAction = DashActionGamepad;
			SlideAction = SlideActionGamepad;
			BulletTimeAction = BulletTimeActionGamepad;
			PrevWeaponAction = PrevWeaponActionGamepad;
			NextWeaponAction = NextWeaponActionGamepad;
			SwitchWeaponModeAction = SwitchWeaponModeActionGamepad;
			UseWeaponAction = UseWeaponActionGamepad;
		}

		DashAction.Connect( "triggered", Callable.From( OnDash ) );
		SlideAction.Connect( "triggered", Callable.From( OnSlide ) );
		UseWeaponAction.Connect( "triggered", Callable.From( OnUseWeapon ) );
	}
	private void LoadSfx() {
		ChangeWeaponSfx = GetNode<AudioStreamPlayer2D>( "SoundEffects/ChangeWeapon" );
		
		PainSfx = new List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Pain0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Pain1" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Pain2" )
		};
		DieSfx = new List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Die0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Die1" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Die2" )
		};
		DeathSfx = new List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/DieSound0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/DieSound1" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/DieSound2" )
		};
		MoveGravelSfx = new List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/MoveGravel0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/MoveGravel1" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/MoveGravel2" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/MoveGravel3" )
		};
		DashSfx = new List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Dash0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Dash1" )
		};
		SlideSfx = new List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Slide0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Slide1" )
		};
		SlowMoBeginSfx = GetNode<AudioStreamPlayer2D>( "SoundEffects/SlowMoBegin" );
		SlowMoBeginSfx = GetNode<AudioStreamPlayer2D>( "SoundEffects/SlowMoEnd" );
	}

	private void OnLegsAnimationLooped() {
		if ( Velocity != Godot.Vector2.Zero ) {
			PlaySfx( MoveGravelSfx[ RandomFactory.Next( 0, MoveGravelSfx.Count - 1 ) ] );
		}
	}

	private void OnDashTimeTimeout() {
		( (TextureRect)HUD.Get( "_dash_overlay" ) ).Hide();
		DashEffect.Emitting = false;
		DashEffect.Hide();
		Flags &= ~PlayerFlags.Dashing;
		if ( LegAnimation.FlipH ) {
			JumpkitSparks.FlipH = false;
			JumpkitSparks.Offset = new Godot.Vector2( 255.0f, 0.0f );
		} else {
			JumpkitSparks.FlipH = true;
			JumpkitSparks.Offset = new Godot.Vector2( 0.0f, 0.0f );
		}
	}

	private void OnSlideTimeout() {
		SlideEffect.Emitting = false;
		Flags &= ~PlayerFlags.Sliding;
	}
	
	private void OnJumpkitSparksAnimationFinished() {
		JumpkitSparks.Hide();
	}

	private void OnIdleAnimationTimerTimeout() {
		if ( IdleAnimation.IsPlaying() ) {
			return;
		}
		TorsoAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();
		LegAnimation.Hide();
		IdleAnimation.Show();
		IdleAnimation.Play( "start" );
	}
	
	private void OnIdleAnimationFinished() {
		IdleAnimation.Play( "loop" );
	}

	public void SwitchWeaponWielding() {
	}
	private void SwitchWeaponMode() {
	}
	private void SwitchWeaponHand() {
	}

	private void OnDash() {
		if ( ( (Control)GetNode( "/root/Console" ) ).Visible ) {
			return;
		}

		IdleReset();
		Flags |= PlayerFlags.Dashing;
		DashTime.Start();
		PlaySfx( DashSfx[ RandomFactory.Next( 0, DashSfx.Count - 1 ) ] );
		DashEffect.Show();
		DashEffect.Emitting = true;
		DashDirection = Velocity;
		( (TextureRect)HUD.Get( "_dash_overlay" ) ).Show();
	}
	private void OnSlide() {
		if ( ( (Control)GetNode( "/root/Console" ) ).Visible ) {
			return;
		} else if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
			return;
		}
		
		IdleReset();
		Flags |= PlayerFlags.Sliding;
		SlideTime.Start();
		PlaySfx( SlideSfx[ RandomFactory.Next( 0, SlideSfx.Count - 1 ) ] );
		SlideEffect.Emitting = true;
		LegAnimation.Play( "slide" );
	}
	private void OnUseWeapon() {
		if ( ( (Control)GetNode( "/root/Console" ) ).Visible ) {
			return;
		}
		IdleReset();
		if ( WeaponSlots[ CurrentWeapon ].IsUsed() ) {
			FrameDamage += WeaponSlots[ CurrentWeapon ].GetWeapon().Use( WeaponSlots[ CurrentWeapon ].GetMode() );
		}
	}

	private void OnArmAngleChanged() {
		ArmAngle = (float)ArmAngleActionGamepad.Get( "value_axis_1d" );
	}

    public override void _Ready() {
		base._Ready();

		//
		// initialize input context
		//
		SwitchToGamepad = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_to_gamepad.tres" );
		SwitchToKeyboard = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_to_keyboard.tres" );

		Animations = GetNode<Node2D>( "Animations" );
		TorsoAnimation = GetNode<AnimatedSprite2D>( "Animations/Torso" );
		LegAnimation = GetNode<AnimatedSprite2D>( "Animations/Legs" );
		if ( LegAnimation == null ) {
			GD.PushError( "ERROR" );
		}
		IdleAnimation = GetNode<AnimatedSprite2D>( "Animations/Idle" );
		JumpkitSparks = GetNode<AnimatedSprite2D>( "Animations/JumpkitSparks" );
		WalkEffect = GetNode<GpuParticles2D>( "Animations/DustPuff" );
		DashEffect = GetNode<GpuParticles2D>( "Animations/DashEffect" );

		IdleTimer = GetNode<Timer>( "IdleAnimationTimer" );
		ArmLeft = GetNode<Arm>( "ArmLeft" );
		ArmRight = GetNode<Arm>( "ArmRight" );

		DashTime = GetNode<Timer>( "Timers/DashTime" );
		SlideTime = GetNode<Timer>( "Timers/SlideTime" );
		DashCooldown = GetNode<Timer>( "Timers/DashCooldownTime" );

		Camera = GetNode<Camera2D>( "Camera2D" );

		HUD = GetNode<Node2D>( "HeadsUpDisplay" );

		// connect signals
		SwitchToKeyboard.Connect( "triggered", Callable.From<Resource>( SwitchInputMode ) );
		SwitchToGamepad.Connect( "triggererd", Callable.From<Resource>( SwitchInputMode ) );

		LegAnimation.Connect( "animation_looped", Callable.From( OnLegsAnimationLooped ) );
		DashTime.Connect( "timeout", Callable.From( OnDashTimeTimeout ) );
		IdleAnimation.Connect( "animation_finished", Callable.From( OnIdleAnimationFinished ) );
		IdleTimer.Connect( "timeout", Callable.From( OnIdleAnimationTimerTimeout ) );
		JumpkitSparks.Connect( "animation_finished", Callable.From( OnJumpkitSparksAnimationFinished ) );

		AmmoHeavyStacks = new List<Resource>();
		AmmoLightStacks = new List<Resource>();
		AmmoPelletStacks = new List<Resource>();
		ConsumableStacks = new List<Resource>();
		WeaponStacks = new List<Resource>();

		if ( SplitScreen ) {
			ArmAngleActionGamepad.Connect( "triggered", Callable.From( OnArmAngleChanged ) );
			SwitchInputMode( GamepadInputMapping );
		} else {
			SwitchInputMode( KeyboardInputMapping );
		}

		LoadKeyboardBinds();
		LoadGamepadBinds();
		LoadSfx();

		IdleTimer.Start();
		HUD.Call( "init", Health, Rage );
		LastUsedArm = ArmRight;

		WeaponSlots = new List<WeaponSlot>();
		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlots.Add( new WeaponSlot() );
			WeaponSlots[i].SetIndex( i );
		}
	}

    public override void _PhysicsProcess( double delta ) {
		if ( ( (Control)GetNode( "/root/Console" ) ).Visible ) {
			return;
		}

		Godot.Vector2 velocity = Velocity;
		if ( velocity != Godot.Vector2.Zero ) {
			IdleReset();
		}

		float speed = MAX_SPEED;
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			speed += 1800;
		}
		if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
			speed += 400;
		}

		InputVelocity = (Godot.Vector2)MoveAction.Get( "value_axis_2d" );
		if ( InputVelocity != Godot.Vector2.Zero ) {
			velocity = velocity.MoveToward( InputVelocity * speed, (float)delta * ACCEL );
		} else {
			Velocity = Velocity.MoveToward( Godot.Vector2.Zero, (float)delta * FRICTION );
		}

		if ( InputVelocity != Godot.Vector2.Zero ) {
			if ( ( Flags & PlayerFlags.Sliding ) == 0 && ( Flags & PlayerFlags.OnHorse ) == 0 ) {
				LegAnimation.Play( "run" );
				WalkEffect.Emitting = true;
			}
		}
		else {
			LegAnimation.Play( "idle" );
			WalkEffect.Emitting = false;
			SlideEffect.Emitting = false;
		}

		MoveAndSlide();
	}

	private bool CanDash() {
		return ( Flags & PlayerFlags.Dashing ) == 0 && DashCooldown.TimeLeft == 0.0f;
	}
	private void ExitBulletTime() {
		Flags &= ~PlayerFlags.BulletTime;
		PlaySfx( SlowMoEndSfx );
		( (TextureRect)HUD.Get( "_reflex_overlay" ) ).Hide();
		AudioServer.PlaybackSpeedScale = 1.0f;
		Engine.TimeScale = 1.0f;
	}

	private void CheckStatus( float delta ) {
		if ( FrameDamage > 0.0f ) {
			Rage += FrameDamage * delta;
			FrameDamage = 0.0f;
			Flags |= PlayerFlags.UsedMana;
		}

		FrameDamage = 0.0f;
		if ( Health < 100.0f && Rage > 0.0f ) {
			Health += 0.075f * delta;
			Rage -= 0.5f * delta;

			Flags |= PlayerFlags.UsedMana;
		}

		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			if ( Rage <= 0.0f ) {
				ExitBulletTime();
			}
			Rage -= 0.5f * delta;
		}
		
		Rage = Math.Clamp( Rage, 0.0f, 100.0f );

		HUD.Set( "_health_bar.health", Health );
		HUD.Set( "_rage_bar.rage", Rage );
	}

	private void CalcArmAnimation( Arm arm ) {
		arm.Animations.GlobalRotation = ArmAngle;
		arm.Animations.SpriteFrames = arm.GetAnimationSet();
		arm.Animations.FlipV = TorsoAnimation.FlipH;

		if ( arm.GetSlot() == WeaponSlot.INVALID ) {
			arm.Animations.Play( InputVelocity != Godot.Vector2.Zero ? "run" : "idle" );
		} else {
			WeaponEntity weapon = WeaponSlots[ arm.GetSlot() ].GetWeapon();
			string animationName = "";
			switch ( weapon.GetCurrentState() ) {
			case WeaponEntity.WeaponState.Idle:
				animationName = "idle";
				break;
			case WeaponEntity.WeaponState.Reload:
				animationName = "reload";
				break;
			case WeaponEntity.WeaponState.Use:
				animationName = "use";
				break;
			case WeaponEntity.WeaponState.Empty:
				animationName = "empty";
				break;
			};
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
				if ( ( arm == ArmLeft && !arm.Animations.FlipH )
					|| ( arm == ArmRight && arm.Animations.FlipH ) ) {
					animationName += "_flip";
				}
			}
			arm.Animations.Play( animationName );
		}
	}

    public override void _Process( double delta ) {
//		if ( ( (Control)GetNode( "/root/Console" ) ).Visible ) {
//			return;
//		}

        base._Process( delta );

		// the voices!
		// TODO:

		if ( SplitScreen ) {
			if ( ArmAngle > 0.0f ) {
				FlipSpriteRight();
			} else if ( ArmAngle < 0.0f ) {
				FlipSpriteLeft();
			}
		} else {
			Godot.Vector2I screenSize = DisplayServer.WindowGetSize();
			Godot.Vector2 mousePosition = Godot.Vector2.Zero;

			// TODO: implement settings manager in C#?
			if ( (int)( (Node)GetNode( "/root/SettingsData" ) ).Get( "_window_mode" ) >= 2 ) {
				mousePosition = DisplayServer.MouseGetPosition();
			} else {
				mousePosition = GetViewport().GetMousePosition();
			}
			
//			mousePosition = GetGlobalMousePosition();
			float y = mousePosition.Y;
			float x = mousePosition.X;
			int width = screenSize.X;
			int height = screenSize.Y;

			if ( LastMousePosition != mousePosition ) {
				LastMousePosition = mousePosition;
				IdleReset();
			}

			ArmAngle = (float)Math.Atan2( y - ( height / 2.0f ), x - ( width / 2.0f ) );
			if ( x > width * 0.5f ) {
//				FlipSpriteRight();
			} else if ( x < width * 0.5f ) {
//				FlipSpriteLeft();
			}
		}

		CheckStatus( (float)delta );

		PlayerSystem.Arm back = null;
		PlayerSystem.Arm front = null;
		if ( TorsoAnimation.FlipH ) {
			back = ArmRight;
			front = ArmLeft;
		} else {
			back = ArmLeft;
			front = ArmRight;
		}

		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();

		if ( HandsUsed == Hands.Both ) {
			front = LastUsedArm;
			back.Animations.Hide();
		} else {
			back.Animations.Show();
		}

		Animations.MoveChild( back.Animations, 0 );
		Animations.MoveChild( front.Animations, 3 );

		front.Show();

		CalcArmAnimation( front );
		CalcArmAnimation( back );
	}

    public void PickupWeapon( WeaponEntity weapon ) {
		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			if ( !WeaponSlots[i].IsUsed() ) {
				WeaponSlots[i].SetWeapon( weapon );
				CurrentWeapon = (byte)i;
				break;
			}
		}

		Resource itemStack = new Resource();
		WeaponStacks.Add( itemStack );
		Inventory.Call( "add_to_stack", itemStack, weapon.Data.Get( "id" ), 1, weapon.Data.Get( "properties" ) );

		TorsoAnimation.FlipH = false;
		LegAnimation.FlipH = false;

		Resource stack = null;
		switch ( weapon.GetAmmoType() ) {
		case WeaponEntity.AmmoType.Light: {
			if ( AmmoLightStacks.Count > 0 ) {
				stack = AmmoLightStacks.ElementAt( AmmoLightStacks.Count - 1 );
			}
			break; }
		case WeaponEntity.AmmoType.Heavy: {
			if ( AmmoHeavyStacks.Count > 0 ) {
				stack = AmmoHeavyStacks.ElementAt( AmmoHeavyStacks.Count - 1 );
			}
			break; }
		case WeaponEntity.AmmoType.Pellets: {
			if ( AmmoPelletStacks.Count > 0 ) {
				stack = AmmoPelletStacks.ElementAt( AmmoPelletStacks.Count - 1 );
			}
			break; }
		};
		if ( stack != null ) {
//			weapon.SetReserve( stack );
//			weapon.SetAmmo( (Resource)stack.Get( "ammo_type" ) );
		}
		if ( (bool)( (Node)GetNode( "/root/SettingsData" ) ).Get( "_equip_weapon_on_pickup" ) ) {
			HUD.Call( "set_weapon", weapon );

			// apply rules of various weapon properties
			if ( ( weapon.GetDefaultMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				ArmLeft.SetWeapon( CurrentWeapon );
				ArmRight.SetWeapon( CurrentWeapon );

				HandsUsed = Hands.Both;
				LastUsedArm = ArmRight;

				// this will automatically overwrite any other modes
				WeaponSlots[ ArmLeft.GetSlot() ].SetMode( weapon.GetDefaultMode() );
				WeaponSlots[ ArmRight.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}
			else if ( ( weapon.GetDefaultMode() & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
				if ( LastUsedArm == null ) {
					LastUsedArm = ArmRight;
				}
				LastUsedArm.SetWeapon( CurrentWeapon );

				if ( LastUsedArm == ArmRight ) {
					HandsUsed = Hands.Right;
				} else if ( LastUsedArm == ArmLeft ) {
					HandsUsed = Hands.Left;
				}

				WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}

			// update the hand data
			LastUsedArm.SetWeapon( CurrentWeapon );
			WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetProperties() );
			weapon.SetUseMode( weapon.GetDefaultMode() );
		}
	}
};