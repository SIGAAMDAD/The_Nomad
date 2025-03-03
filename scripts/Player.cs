using Godot;
using System;
using PlayerSystem;
using GDExtension.Wrappers;
using System.Linq;

public partial class Player : CharacterBody2D {
	public enum GameMode {
		Singleplayer,
		Coop2,
		Coop3,
		Coop4,
		Multiplayer
	};

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

	private System.Collections.Generic.List<WeaponEntity.Properties> WeaponModeList = new System.Collections.Generic.List<WeaponEntity.Properties>{
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsFirearm,

		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBladed,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBlunt,
		WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsFirearm
	};

	private const int MAX_WEAPON_SLOTS = 4;

	private const float ACCEL = 1500;
	private const float FRICTION = 1400.0f;
	private const float MAX_SPEED = 400.0f;
	private const float JUMP_VELOCITY = -400.0f;

	private Random RandomFactory = new Random();

	private Resource KeyboardInputMappings;
	private Resource GamepadInputMappings;

	private Resource SwitchToKeyboard;
	private Resource SwitchToGamepad;

	private Resource MoveAction;
	private Resource DashAction;
	private Resource SlideAction;
	private Resource MeleeAction;
	private Resource UseWeaponAction;
	private Resource SwitchWeaponModeAction;
	private Resource NextWeaponAction;
	private Resource PrevWeaponAction;
	private Resource OpenInventoryAction;
	private Resource BulletTimeAction;

	private Resource MoveActionGamepad;
	private Resource DashActionGamepad;
	private Resource SlideActionGamepad;
	private Resource MeleeActionGamepad;
	private Resource UseWeaponActionGamepad;
	private Resource SwitchWeaponModeActionGamepad;
	private Resource NextWeaponActionGamepad;
	private Resource PrevWeaponActionGamepad;
	private Resource OpenInventoryActionGamepad;
	private Resource BulletTimeActionGamepad;
	private Resource ArmAngleAction;

	private Resource MoveActionKeyboard;
	private Resource DashActionKeyboard;
	private Resource SlideActionKeyboard;
	private Resource MeleeActionKeyboard;
	private Resource UseWeaponActionKeyboard;
	private Resource SwitchWeaponModeActionKeyboard;
	private Resource NextWeaponActionKeyboard;
	private Resource PrevWeaponActionKeyboard;
	private Resource OpenInventoryActionKeyboard;
	private Resource BulletTimeActionKeyboard;

	private GpuParticles2D WalkEffect;
	private GpuParticles2D SlideEffect;
	private GpuParticles2D DashEffect;
	private AnimatedSprite2D JumpkitSparks;

	private PackedScene BloodSplatter;

	private Node2D Animations;
	private AnimatedSprite2D TorsoAnimation;
	private AnimatedSprite2D LegAnimation;
	private AnimatedSprite2D IdleAnimation;

	private Timer IdleTimer;

	private Timer DashTime;
	private Timer SlideTime;
	private Timer DashCooldownTime;

	[Export]
	private Inventory Inventory;
	[Export]
	private Arm ArmLeft;
	[Export]
	private Arm ArmRight;
	[Export]
	private HeadsUpDisplay HUD;

	private System.Collections.Generic.List<WeaponSlot> WeaponSlots;

	private float Health = 100.0f;
	private float Rage = 0.0f;
	private PlayerFlags Flags = 0;
	private int CurrentWeapon = 0;

	// multiplayer data
	public ulong MultiplayerId = 0; // literally just a steamID
	public string MultiplayerUsername;
	public uint MultiplayerKills = 0;
	public uint MultiplayerDeaths = 0;
	public uint MultiplayerFlagCaptures = 0;
	public uint MultiplayerFlagReturns = 0;
	public float MultiplayerHilltime = 0.0f;

	private AudioStreamPlayer2D ChangeWeaponSfx;
	private System.Collections.Generic.List<AudioStreamPlayer2D> PainSfx;
	private System.Collections.Generic.List<AudioStreamPlayer2D> DieSfx;
	private System.Collections.Generic.List<AudioStreamPlayer2D> DeathSfx;
	private System.Collections.Generic.List<AudioStreamPlayer2D> MoveGravelSfx;
	private System.Collections.Generic.List<AudioStreamPlayer2D> DashSfx;
	private System.Collections.Generic.List<AudioStreamPlayer2D> SlideSfx;
	private AudioStreamPlayer2D SlowMoBeginSfx;
	private AudioStreamPlayer2D SlowMoEndSfx;

	private System.Collections.Generic.List<WeaponEntity> WeaponsStack = new System.Collections.Generic.List<WeaponEntity>();
	private System.Collections.Generic.List<Resource> ConsumableStacks = new System.Collections.Generic.List<Resource>();
	private System.Collections.Generic.List<AmmoStack> AmmoPelletStacks = new System.Collections.Generic.List<AmmoStack>();
	private System.Collections.Generic.List<AmmoStack> AmmoHeavyStacks = new System.Collections.Generic.List<AmmoStack>();
	private System.Collections.Generic.List<AmmoStack> AmmoLightStacks = new System.Collections.Generic.List<AmmoStack>();

	private Hands HandsUsed = Hands.Right;
	private Arm LastUsedArm;
	private float ArmAngle = 0.0f;
	private int InputDevice = 0;
	private float FrameDamage;
	private int Hellbreaks = 0;
	private bool SplitScreen = false;
	private Godot.Vector2 DashDirection = Godot.Vector2.Zero;
	private Godot.Vector2 InputVelocity = Godot.Vector2.Zero;
	private Godot.Vector2 LastMousePosition = Godot.Vector2.Zero;

	[Signal]
	public delegate void DieEventHandler( CharacterBody2D attacker, CharacterBody2D target );
	[Signal]
	public delegate void DamagedEventHandler( CharacterBody2D attacker, CharacterBody2D target, float nAmount );

	public System.Collections.Generic.List<WeaponEntity> GetWeaponStack() {
		return WeaponsStack;
	}
	public System.Collections.Generic.List<AmmoStack> GetLightAmmoStacks() {
		return AmmoLightStacks;
	}
	public System.Collections.Generic.List<AmmoStack> GetHeavyAmmoStacks() {
		return AmmoHeavyStacks;
	}
	public System.Collections.Generic.List<AmmoStack> GetPelletAmmoStacks() {
		return AmmoPelletStacks;
	}
	public Inventory GetInventory() {
		return Inventory;
	}
	public Arm GetWeaponHand( WeaponEntity weapon ) {
		if ( ArmLeft.GetSlot() != WeaponSlot.INVALID && WeaponSlots[ ArmLeft.GetSlot() ].GetWeapon() == weapon ) {
			return ArmLeft;
		} else if ( ArmRight.GetSlot() != WeaponSlot.INVALID && WeaponSlots[ ArmRight.GetSlot() ].GetWeapon() == weapon ) {
			return ArmRight;
		}
		return null;
	}
	public Godot.Vector2 GetInputVelocity() {
		return InputVelocity;
	}
	public WeaponSlot GetSlot( int nSlot ) {
		return WeaponSlots[ nSlot ];
	}
	public float GetArmAngle() {
		return ArmAngle;
	}
	public Arm GetLastUsedArm() {
		return LastUsedArm;
	}
	public void SetLastUsedArm( Arm arm ) {
		LastUsedArm = arm;
	}

	public Hands GetHandsUsed() {
		return HandsUsed;
	}
	public void SetHandsUsed( Hands hands ) {
		HandsUsed = hands;
	}

	public Arm GetLeftArm() {
		return ArmLeft;
	}
	public Arm GetRightArm() {
		return ArmRight;
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

	public void SetupSplitScreen( int nInputIndex ) {
		GetNode( "/root/Console" ).Call( "print_line", "Setting up split-screen input for " + nInputIndex );

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			SplitScreen = true;
		}
		InputDevice = nInputIndex;
	}

	public override void _ExitTree() {
		base._ExitTree();

		ChangeWeaponSfx.QueueFree();
		for ( int i = 0; i < PainSfx.Count; i++ ) {
			PainSfx[i].QueueFree();
		}
		for ( int i = 0; i < DieSfx.Count; i++ ) {
			DieSfx[i].QueueFree();
		}
		for ( int i = 0; i < DeathSfx.Count; i++ ) {
			DeathSfx[i].QueueFree();
		}
		for ( int i = 0; i < MoveGravelSfx.Count; i++ ) {
			MoveGravelSfx[i].QueueFree();
		}
		for ( int i = 0; i < DashSfx.Count; i++ ) {
			DashSfx[i].QueueFree();
		}
		for ( int i = 0; i < SlideSfx.Count; i++ ) {
			SlideSfx[i].QueueFree();
		}
		SlowMoBeginSfx.QueueFree();
		SlowMoEndSfx.QueueFree();

		PainSfx.Clear();
		DieSfx.Clear();
		DeathSfx.Clear();
		MoveGravelSfx.Clear();
		DashSfx.Clear();
		SlideSfx.Clear();

		DashTime.QueueFree();
		SlideTime.QueueFree();
		DashCooldownTime.QueueFree();

		WalkEffect.QueueFree();
		DashEffect.QueueFree();
		SlideEffect.QueueFree();
		JumpkitSparks.QueueFree();

		IdleTimer.QueueFree();
		IdleAnimation.QueueFree();

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

		WeaponSlots.Clear();

		ConsumableStacks.Clear();
		WeaponsStack.Clear();
		AmmoLightStacks.Clear();
		AmmoHeavyStacks.Clear();
		AmmoPelletStacks.Clear();

		QueueFree();
	}

	private void OnDeath( CharacterBody2D attacker ) {
		EmitSignal( "Die", attacker, this );
		LegAnimation.Hide();
		ArmLeft.Animations.Hide();
		ArmRight.Animations.Hide();

		TorsoAnimation.Play( "death" );

		DieSfx[ RandomFactory.Next( 0, DieSfx.Count - 1 ) ].Play();

		SetProcessUnhandledInput( true );
	}

	public void Damage( CharacterBody2D attacker, float nAmount ) {
		if ( ( Flags & PlayerFlags.Dashing ) != 0 ) {
			return; // iframes
		}

		EmitSignal( "Damaged", attacker, this, nAmount );

		Health -= nAmount;
		Rage += nAmount;

		Node2D blood = BloodSplatter.Instantiate<Node2D>();
		blood.GlobalPosition = GlobalPosition;
		AddChild( blood );

		if ( Health <= 0.0f ) {
			OnDeath( attacker );
		} else {
			PainSfx[ RandomFactory.Next( 0, PainSfx.Count - 1 ) ].Play();
		}
	}

	public void Save( FileAccess file ) {
		CommandConsole.PrintLine( "Saving player data..." );

		file.StoreFloat( GlobalPosition.X );
		file.StoreFloat( GlobalPosition.Y );

		file.StoreFloat( Health );
		file.StoreFloat( Rage );
		file.Store32( (uint)Flags );
		file.Store32( (uint)Hellbreaks );
		file.Store32( (uint)CurrentWeapon );
		file.Store32( (uint)HandsUsed );

		file.Store32( (uint)ArmLeft.GetSlot() );
		file.Store32( (uint)ArmRight.GetSlot() );

		file.Store32( MAX_WEAPON_SLOTS );
		for ( int i = 0; i < WeaponSlots.Count; i++ ) {
			file.Store8( WeaponSlots[i].IsUsed() ? (byte)1 : (byte)0 );
			if ( WeaponSlots[i].IsUsed() ) {
				file.StorePascalString( (string)WeaponSlots[i].GetWeapon().Data.Get( "id" ) );
			}
		}
	}
	public void Load( FileAccess file ) {
		CommandConsole.PrintLine( "Loading player data..." );

		Godot.Vector2 position = new Godot.Vector2();
		position.X = file.GetFloat();
		position.Y = file.GetFloat();
		GlobalPosition = position;

		Health = file.GetFloat();
		Rage = file.GetFloat();
		Flags = (PlayerFlags)file.Get32();
		Hellbreaks = (int)file.Get32();
		CurrentWeapon = (int)file.Get32();
		HandsUsed = (Hands)file.Get32();
		
		uint leftSlot = file.Get32();
		if ( leftSlot == (uint)WeaponSlot.INVALID ) {
			ArmLeft.SetWeapon( WeaponSlot.INVALID );
		} else {
			ArmLeft.SetWeapon( (int)leftSlot );
		}
		uint rightSlot = file.Get32();
		if ( rightSlot == (uint)WeaponSlot.INVALID ) {
			ArmRight.SetWeapon( WeaponSlot.INVALID );
		} else {
			ArmRight.SetWeapon( (int)rightSlot );
		}

		uint maxSlots = file.Get32();
		for ( int i = 0; i < maxSlots; i++ ) {
			bool used = file.Get8() == 1 ? true : false;
			if ( used ) {
				WeaponEntity weapon = new WeaponEntity();
				weapon.Data = Inventory.GetItemFromId( file.GetPascalString() );
				WeaponSlots[i].SetWeapon( weapon );
				AddChild( weapon );
			}
		}
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
		ArmAngleAction = ResourceLoader.Load( "res://resources/binds/actions/gamepad/arm_angle.tres" );
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
	private void LoadSfx() {
		ChangeWeaponSfx = GetNode<AudioStreamPlayer2D>( "SoundEffects/ChangeWeapon" );
		
		PainSfx = new System.Collections.Generic.List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Pain0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Pain1" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Pain2" )
		};
		DieSfx = new System.Collections.Generic.List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Die0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Die1" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Die2" )
		};
		DeathSfx = new System.Collections.Generic.List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/DieSound0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/DieSound1" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/DieSound2" )
		};
		MoveGravelSfx = new System.Collections.Generic.List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/MoveGravel0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/MoveGravel1" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/MoveGravel2" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/MoveGravel3" )
		};
		DashSfx = new System.Collections.Generic.List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Dash0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Dash1" )
		};
		SlideSfx = new System.Collections.Generic.List<AudioStreamPlayer2D>{
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Slide0" ),
			GetNode<AudioStreamPlayer2D>( "SoundEffects/Slide1" )
		};
		SlowMoBeginSfx = GetNode<AudioStreamPlayer2D>( "SoundEffects/SlowMoBegin" );
		SlowMoEndSfx = GetNode<AudioStreamPlayer2D>( "SoundEffects/SlowMoEnd" );
	}

	private void ExitBulletTime() {
		Flags &= ~PlayerFlags.BulletTime;
		SlowMoEndSfx.Play();
		HUD.GetReflexOverlay().Hide();
		Engine.TimeScale = 1.0f;
		AudioServer.PlaybackSpeedScale = 1.0f;
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
		SteamAchievements.ActivateAchievement( "ACH_SMOKE_BREAK" );
	}
	private void OnIdleAnimationAnimationFinished()	{
		IdleAnimation.Play( "loop" );
	}
	private void OnLegsAnimationLooped() {
		if ( Velocity != Godot.Vector2.Zero ) {
			MoveGravelSfx[ RandomFactory.Next( 0, MoveGravelSfx.Count - 1 ) ].Play();
		}
	}
	private void OnDashTimeTimeout() {
		HUD.GetDashOverlay().Hide();
		( (PointLight2D)DashEffect.GetChild( 0 ) ).Hide();
		DashEffect.Emitting = false;
		Flags &= ~PlayerFlags.Dashing;
		if ( LegAnimation.FlipH ) {
			JumpkitSparks.FlipH = false;
			JumpkitSparks.Offset = new Godot.Vector2( 255.0f, 0.0f );
		} else {
			JumpkitSparks.FlipH = false;
			JumpkitSparks.Offset = Godot.Vector2.Zero;
		}
	}
	private void OnSlideTimeout() {
		SlideEffect.Emitting = false;
		Flags &= ~PlayerFlags.Sliding;
	}

	private void OnDash() {
		IdleReset();
		Flags |= PlayerFlags.Dashing;
		DashTime.Start();
		DashSfx[ RandomFactory.Next( 0, DashSfx.Count - 1 ) ].Play();
		( (PointLight2D)DashEffect.GetChild( 0 ) ).Show();
		DashEffect.Emitting = true;
		DashDirection = Velocity;
		if ( HUD == null ) {
			GD.PushError( "HUD IS NULL" );
		}
		HUD.GetDashOverlay().Show();
	}
	private void OnSlide() {
		if ( ( Flags & PlayerFlags.Sliding ) != 0 ) {
			return;
		}
		IdleReset();
		Flags |= PlayerFlags.Sliding;
		SlideTime.Start();
		SlideEffect.Emitting = true;
		LegAnimation.Play( "slide" );
	}
	private void OnUseWeapon() {
		IdleReset();

		int slot = LastUsedArm.GetSlot();
		if ( slot == WeaponSlot.INVALID ) {
			return; // nothing equipped
		}
		if ( WeaponSlots[ slot ].IsUsed() ) {
			FrameDamage += WeaponSlots[ slot ].GetWeapon().Use( WeaponSlots[ slot ].GetWeapon().GetLastUsedMode() );
		}
	}
	private void OnArmAngleChanged() {
		ArmAngle = (float)ArmAngleAction.Get( "value_axis_1d" );
	}
	private void OnPrevWeapon() {
		int index = CurrentWeapon <= 0 ? MAX_WEAPON_SLOTS - 1 : CurrentWeapon - 1;
		while ( index != -1 ) {
			if ( WeaponSlots[ index ].IsUsed() ) {
				break;
			}
			index--;
		}

		if ( index == MAX_WEAPON_SLOTS ) {
			index = -1;
		}

		// adjust arm state
		if ( index != WeaponSlot.INVALID ) {
			WeaponEntity weapon;
			Arm otherArm;

			if ( LastUsedArm == ArmLeft ) {
				otherArm = ArmRight;
				HandsUsed = Hands.Left;
			} else if ( LastUsedArm == ArmRight ) {
				otherArm = ArmLeft;
				HandsUsed = Hands.Right;
			} else {
				GD.PushError( "OnNextWeapon: invalid LastUsedArm" );
				LastUsedArm = ArmRight;
				return;
			}

			weapon = WeaponSlots[ index ].GetWeapon();
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				otherArm.SetWeapon( WeaponSlot.INVALID );
				HandsUsed = Hands.Both;
			}

			HUD.SetWeapon( weapon );
		} else {
			HUD.SetWeapon( null );
		}

		ChangeWeaponSfx.Play();
		CurrentWeapon = index;
		LastUsedArm.SetWeapon( CurrentWeapon );
	}
	private void OnNextWeapon() {
		int index = CurrentWeapon == MAX_WEAPON_SLOTS - 1 ? 0 : CurrentWeapon + 1;
		while ( index < MAX_WEAPON_SLOTS ) {
			if ( WeaponSlots[ index ].IsUsed() ) {
				break;
			}
			index++;
		}

		if ( index == MAX_WEAPON_SLOTS ) {
			index = -1;
		}

		// adjust arm state
		if ( index != WeaponSlot.INVALID ) {
			WeaponEntity weapon;
			Arm otherArm;

			if ( LastUsedArm == ArmLeft ) {
				otherArm = ArmRight;
				HandsUsed = Hands.Left;
			} else if ( LastUsedArm == ArmRight ) {
				otherArm = ArmLeft;
				HandsUsed = Hands.Right;
			} else {
				GD.PushError( "OnNextWeapon: invalid LastUsedArm" );
				LastUsedArm = ArmRight;
				return;
			}

			weapon = WeaponSlots[ index ].GetWeapon();
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
				otherArm.SetWeapon( WeaponSlot.INVALID );
				HandsUsed = Hands.Both;
			}

			HUD.SetWeapon( weapon );
		} else {
			HUD.SetWeapon( null );
		}

		ChangeWeaponSfx.Play();
		CurrentWeapon = index;
		LastUsedArm.SetWeapon( CurrentWeapon );
	}
	private void OnBulletTime() {
		IdleReset();
		if ( ( Flags & PlayerFlags.BulletTime ) != 0 ) {
			ExitBulletTime();
		} else {
			Flags |= PlayerFlags.BulletTime;
			AudioServer.PlaybackSpeedScale = 0.50f;
			Engine.TimeScale = 0.010f;
		}
	}
	private void OnToggleInventory() {
		HUD.OnShowInventory();
	}
	private void SwitchWeaponWielding() {
		Arm src;
		Arm dst;

		switch ( HandsUsed ) {
		case Hands.Left:
			src = ArmLeft;
			dst = ArmRight;
			break;
		case Hands.Right:
			src = ArmRight;
			dst = ArmLeft;
			break;
		case Hands.Both:
		default:
			src = LastUsedArm;
			dst = src;
			break;
		};

		if ( src.GetSlot() == WeaponSlot.INVALID ) {
			// nothing in the source hand, deny
			return;
		}

		LastUsedArm = dst;

		WeaponEntity srcWeapon = WeaponSlots[ src.GetSlot() ].GetWeapon();
		if ( ( srcWeapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 && ( srcWeapon.GetLastUsedMode() & WeaponEntity.Properties.IsOneHanded ) == 0 ) {
			// cannot change hands, no one-handing allowed
			return;
		}
		
		// check if the destination hand has something in it, if true, then swap
		if ( dst.GetSlot() != WeaponSlot.INVALID ) {
			int tmp = dst.GetSlot();

			dst.SetWeapon( src.GetSlot() );
			src.SetWeapon( tmp );
		} else {
			// if we have nothing in the destination hand, then just clear the source hand
			int tmp = src.GetSlot();

			src.SetWeapon( WeaponSlot.INVALID );
			dst.SetWeapon( tmp );
		}
	}
	private void SwitchWeaponHand() {
		// TODO: implement use both hands
		
		switch ( HandsUsed ) {
		case Hands.Left:
			HandsUsed = Hands.Right; // set to right
			LastUsedArm = ArmRight;
			break;
		case Hands.Right:
			HandsUsed = Hands.Left; // set to right
			LastUsedArm = ArmLeft;
			break;
		// can't switch if we're using both hands for one weapon
		case Hands.Both:
		default:
			GD.PushWarning( "SwitchWeaponHand: invalid hand, setting to default of right" );
			HandsUsed = Hands.Right;
			break;
		};
		if ( LastUsedArm.GetSlot() != WeaponSlot.INVALID ) {
			EquipSlot( LastUsedArm.GetSlot() );
		}
	}
	private void SwitchWeaponMode() {
		// weapon mode order (default)
		// blade -> blunt -> firearm

		WeaponSlot slot;

		switch ( HandsUsed ) {
		case Hands.Left: {
			int index = ArmLeft.GetSlot();
			if ( index == WeaponSlot.INVALID ) {
				return;
			}
			slot = WeaponSlots[ index ];
			break; }
		case Hands.Right: {
			int index = ArmRight.GetSlot();
			if ( index == WeaponSlot.INVALID ) {
				return;
			}
			slot = WeaponSlots[ index ];
			break; }
		case Hands.Both:
		default:
			slot = WeaponSlots[ CurrentWeapon ];
			break;
		};

		WeaponEntity.Properties mode = slot.GetMode();
		WeaponEntity weapon = slot.GetWeapon();

		slot.SetMode( WeaponEntity.Properties.None ); // clear the modes

		// find the next most suitable mode
		WeaponEntity.Properties props = weapon.GetProperties();
		bool IsOneHanded = ( props & WeaponEntity.Properties.IsOneHanded ) != 0;
		bool IsTwoHanded = ( props & WeaponEntity.Properties.IsTwoHanded ) != 0;
		const WeaponEntity.Properties hands = ~(  WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsTwoHanded );

		for ( int i = 0; i < WeaponModeList.Count; i++ ) {
			WeaponEntity.Properties current = WeaponModeList[i];
			if ( mode == current ) {
				// same mode, don't switch
				continue;
			}
			if ( ( props & ( current & hands ) ) != 0 ) {
				// apply some filters
				if ( ( current & WeaponEntity.Properties.IsOneHanded ) != 0 && !IsOneHanded ) {
					// one handing not possible, deny
					continue;
				}
				if ( ( current & WeaponEntity.Properties.IsTwoHanded ) != 0 && IsTwoHanded ) {
					// two handing not possible, deny
					continue;
				}
				
				// we've got a match!
				slot.SetMode( current );
				slot.GetWeapon().SetUseMode( current );
			}
		}
	}
	private void SwitchInputMode( Resource InputContext ) {
		GetNode( "/root/GUIDE" ).Call( "enable_mapping_context", InputContext );

		if ( InputContext == KeyboardInputMappings ) {
			MoveAction = MoveActionKeyboard;
			DashAction = DashActionKeyboard;
			SlideAction = SlideActionKeyboard;
			BulletTimeAction = BulletTimeActionKeyboard;
			PrevWeaponAction = PrevWeaponActionKeyboard;
			NextWeaponAction = NextWeaponActionKeyboard;
			SwitchWeaponModeAction = SwitchWeaponModeActionKeyboard;
			OpenInventoryAction = OpenInventoryActionKeyboard;
			UseWeaponAction = UseWeaponActionKeyboard;
		} else {
			MoveAction = MoveActionGamepad;
			DashAction = DashActionGamepad;
			SlideAction = SlideActionGamepad;
			BulletTimeAction = BulletTimeActionGamepad;
			PrevWeaponAction = PrevWeaponActionGamepad;
			NextWeaponAction = NextWeaponActionGamepad;
			SwitchWeaponModeAction = SwitchWeaponModeActionGamepad;
			OpenInventoryAction = OpenInventoryActionGamepad;
			UseWeaponAction = UseWeaponActionGamepad;
		}


		SwitchWeaponModeAction.Connect( "triggered", Callable.From( SwitchWeaponMode ) );
		BulletTimeAction.Connect( "triggered", Callable.From( OnBulletTime ) );
		NextWeaponAction.Connect( "triggered", Callable.From( OnNextWeapon ) );
		PrevWeaponAction.Connect( "triggered", Callable.From( OnPrevWeapon ) );
		DashAction.Connect( "triggered", Callable.From( OnDash ) );
		SlideAction.Connect( "triggered", Callable.From( OnSlide ) );
		UseWeaponAction.Connect( "triggered", Callable.From( OnUseWeapon ) );
		OpenInventoryAction.Connect( "triggered", Callable.From( OnToggleInventory ) );
	}

	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );
		
		if ( Health <= 0.0f ) {
			// dead
			Health = 100.0f;

			SetProcess( true );
			SetPhysicsProcess( true );
			SetProcessUnhandledInput( false );

			TorsoAnimation.Play( "idle" );
			LegAnimation.Show();
			ArmLeft.Animations.Show();
			ArmRight.Animations.Show();

			EmitSignal( "Respawn" );
		}
	}

    public void EquipSlot( int nSlot ) {
		CurrentWeapon = nSlot;

		WeaponEntity weapon = WeaponSlots[ nSlot ].GetWeapon();
		if ( weapon != null ) {
			// apply rules of various weapon properties
			if ( ( weapon.GetLastUsedMode() & WeaponEntity.Properties.IsTwoHanded ) != 0 )	{
				ArmLeft.SetWeapon( CurrentWeapon );
				ArmRight.SetWeapon( CurrentWeapon );

				// this will automatically override any other modes
				WeaponSlots[ ArmLeft.GetSlot() ].SetMode( weapon.GetDefaultMode() );
			}

			WeaponSlots[ LastUsedArm.GetSlot() ].SetMode( weapon.GetProperties() );
		}
		else {
			ArmLeft.SetWeapon( -1 );
			ArmRight.SetWeapon( -1 );
		}

		// update hand data
		LastUsedArm.SetWeapon( CurrentWeapon );

		HUD.SetWeapon( weapon );
	}

	private void CmdSuicide() {
		Health = 0.0f;
		OnDeath( this );
	}

	private float LitValue = 0.0f;
	private Texture2D LastSpriteTexture = null;
	private Rect2I LastCropRect = new Rect2I();

	private void OnViewportFramePreDraw() {
		Texture2D spriteTexture = TorsoAnimation.SpriteFrames.GetFrameTexture( TorsoAnimation.Animation, TorsoAnimation.Frame );
		Image image = spriteTexture.GetImage();

		image.GetPixel( 0, 0 );

		LastSpriteTexture = TorsoAnimation.SpriteFrames.GetFrameTexture( TorsoAnimation.Animation, TorsoAnimation.Frame );
		Godot.Vector2 viewportScale = TorsoAnimation.GetViewportTransform().Scale;
		Godot.Vector2I screenPos = (Godot.Vector2I)( TorsoAnimation.GetScreenTransform().Origin * viewportScale );
		LastCropRect = new Godot.Rect2I( screenPos - ( ( (Godot.Vector2I)LastSpriteTexture.GetSize() / new Godot.Vector2I( 2, 2 ) ) * (Godot.Vector2I)viewportScale ), (Godot.Vector2I)LastSpriteTexture.GetSize() * (Godot.Vector2I)viewportScale );
	}
	private void OnViewportFramePostDraw() {
		if ( LastSpriteTexture == null ) {
			return;
		}
		Image viewImage = GetViewport().GetTexture().GetImage();
		Image spriteImage = LastSpriteTexture.GetImage();

		viewImage = viewImage.GetRegion( LastCropRect );
		viewImage.Resize( (int)LastSpriteTexture.GetSize().X, (int)LastSpriteTexture.GetSize().Y );
		viewImage.Convert( spriteImage.GetFormat() );

		Image finalImage = Image.CreateEmpty( (int)LastSpriteTexture.GetSize().X, (int)LastSpriteTexture.GetSize().Y, false, Image.Format.Rgba8 );
		finalImage.BlitRectMask( viewImage, spriteImage, new Rect2I( Vector2I.Zero, (Vector2I)LastSpriteTexture.GetSize() ), Vector2I.Zero );
		finalImage.FixAlphaEdges();
		finalImage.Resize( 1, 1, Image.Interpolation.Lanczos );
		spriteImage.Resize( 1, 1, Image.Interpolation.Lanczos );

		Color finalColor = finalImage.GetPixel( 0, 0 );
		Color baseColor = spriteImage.GetPixel( 0, 0 );
		if ( baseColor.A != 0.0f ) {
			finalColor = finalColor / finalColor.A;
			baseColor = baseColor / baseColor.A;
			LitValue = finalColor.Luminance - baseColor.Luminance;
		}
	}

	public override void _Ready() {
		base._Ready();

		Engine.MaxFps = 0;

		LoadKeyboardBinds();
		LoadGamepadBinds();
		LoadSfx();

		SetProcessUnhandledInput( false );

		KeyboardInputMappings = ResourceLoader.Load( "res://resources/binds/binds_keyboard.tres" );
		GamepadInputMappings = ResourceLoader.Load( "res://resources/binds/binds_gamepad.tres" );

		//
		// initialize input context
		//
		SwitchToGamepad = ResourceLoader.Load( "res://resources/binds/actions/keyboard/switch_to_gamepad.tres" );
		SwitchToKeyboard = ResourceLoader.Load( "res://resources/binds/actions/gamepad/switch_to_keyboard.tres" );

		SwitchToKeyboard.Connect( "triggered", Callable.From<Resource>( SwitchInputMode ) );
		SwitchToGamepad.Connect( "triggered", Callable.From<Resource>( SwitchInputMode ) );

		if ( SplitScreen ) {
			ArmAngleAction.Connect( "triggered", Callable.From( OnArmAngleChanged ) );
			SwitchInputMode( GamepadInputMappings );
		} else {
			SwitchInputMode( KeyboardInputMappings );
		}

		DashTime = GetNode<Timer>( "Timers/DashTime" );
		DashTime.Connect( "timeout", Callable.From( OnDashTimeTimeout ) );

		IdleTimer = GetNode<Timer>( "IdleAnimationTimer" );
		IdleTimer.Connect( "timeout", Callable.From( OnIdleAnimationTimerTimeout ) );

		IdleAnimation = GetNode<AnimatedSprite2D>( "Animations/Idle" );
		IdleAnimation.Connect( "animation_finished", Callable.From( OnIdleAnimationAnimationFinished ) );

		LegAnimation = GetNode<AnimatedSprite2D>( "Animations/Legs" );
		LegAnimation.Connect( "animation_looped", Callable.From( OnLegsAnimationLooped ) );

		TorsoAnimation = GetNode<AnimatedSprite2D>( "Animations/Torso" );
		Animations = GetNode<Node2D>( "Animations" );

		WalkEffect = GetNode<GpuParticles2D>( "Animations/DustPuff" );
		SlideEffect = GetNode<GpuParticles2D>( "Animations/SlidePuff" );
		DashEffect = GetNode<GpuParticles2D>( "Animations/DashEffect" );
		JumpkitSparks = GetNode<AnimatedSprite2D>( "Animations/JumpkitSparks" );

		SlideTime = GetNode<Timer>( "Timers/SlideTime" );
		DashCooldownTime = GetNode<Timer>( "Timers/DashCooldownTime" );

		WeaponSlots = new System.Collections.Generic.List<WeaponSlot>();
		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlots.Add( new WeaponSlot() );
			WeaponSlots[i].SetIndex( i );
		}

		LastUsedArm = ArmRight;

//		RenderingServer.FramePostDraw += () => OnViewportFramePostDraw();
//		RenderingServer.FramePreDraw += () => OnViewportFramePreDraw();

		GetNode( "/root/Console" ).Call( "add_command", "suicide", Callable.From( CmdSuicide ) );
	}

	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		if ( Velocity != Godot.Vector2.Zero ) {
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
			Velocity = Velocity.MoveToward( InputVelocity * speed, (float)delta * ACCEL );
		} else {
			Velocity = Velocity.MoveToward( Godot.Vector2.Zero, (float)delta * FRICTION );
		}

		if ( InputVelocity != Godot.Vector2.Zero ) {
			if ( ( Flags & PlayerFlags.Sliding ) == 0 && ( Flags & PlayerFlags.OnHorse ) == 0 ) {
				LegAnimation.Play( "run" );
				WalkEffect.Emitting = true;
			}
		} else {
			LegAnimation.Play( "idle" );
			WalkEffect.Emitting = false;
			SlideEffect.Emitting = false;
		}

		MoveAndSlide();
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
		base._Process( delta );

		if ( SplitScreen ) {
			if ( ArmAngle > 0.0f ) {
				FlipSpriteRight();
			} else if ( ArmAngle < 0.0f ) {
				FlipSpriteLeft();
			}
		} else {
			Godot.Vector2I screenSize = DisplayServer.WindowGetSize();
			Godot.Vector2 mousePosition;

			// TODO: implement settings manager in C#?
			if ( (int)GetNode( "/root/SettingsData" ).Get( "_window_mode" ) >= 2 ) {
				mousePosition = DisplayServer.MouseGetPosition();
			} else {
				mousePosition = GetViewport().GetMousePosition();
			}
			
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
				FlipSpriteRight();
			} else if ( x < width * 0.5f ) {
				FlipSpriteLeft();
			}
		}

		PlayerSystem.Arm back = null;
		PlayerSystem.Arm front = null;
		if ( TorsoAnimation.FlipH ) {
			back = ArmRight;
			front = ArmLeft;
		} else {
			back = ArmLeft;
			front = ArmRight;
		}

		ArmLeft.Animations.Show();
		ArmRight.Animations.Show();

		ArmLeft.Animations.Rotation = ArmAngle;
		ArmRight.Animations.Rotation = ArmAngle;

		if ( HandsUsed == Hands.Both ) {
			front = LastUsedArm;
			back.Animations.Hide();
		} else {
			back.Animations.Show();
		}

		Animations.MoveChild( back.Animations, 0 );
		Animations.MoveChild( front.Animations, 3 );

		front.Show();

		CalcArmAnimation( ArmLeft );
		CalcArmAnimation( ArmRight );
	}

	public void PickupAmmo( AmmoEntity ammo ) {
		AmmoStack stack = null;
		bool found = false;

		GD.Print( "Pickup up ammo..." );

		switch ( (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "type" ] ) {
		case (int)AmmoEntity.Type.Light: {
			GD.Print( "Pickup up light ammo" );
			for ( int i = 0; i < AmmoLightStacks.Count; i++ ) {
				if ( ammo.Data == AmmoLightStacks[i].AmmoType ) {
					found = true;
					stack = AmmoLightStacks[i];
					break;
				}
			}
			if ( !found ) {
				stack = new AmmoStack();
				AmmoLightStacks.Add( stack );
			}
			break; }
		case (int)AmmoEntity.Type.Heavy: {
			for ( int i = 0; i < AmmoHeavyStacks.Count; i++ ) {
				if ( ammo.Data == AmmoHeavyStacks[i].AmmoType ) {
					found = true;
					stack = AmmoHeavyStacks[i];
					break;
				}
			}
			if ( !found ) {
				stack = new AmmoStack();
				AmmoHeavyStacks.Add( stack );
			}
			break; }
		case (int)AmmoEntity.Type.Pellets: {
			for ( int i = 0; i < AmmoPelletStacks.Count; i++ ) {
				if ( ammo.Data == AmmoPelletStacks[i].AmmoType ) {
					found = true;
					stack = AmmoPelletStacks[i];
					break;
				}
			}
			if ( !found ) {
				stack = new AmmoStack();
				AmmoPelletStacks.Add( stack );
			}
			break; }
		};

		stack.SetType( ammo );
		stack.AddItems( (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "stack_add_amount" ] );

		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			WeaponSlot slot = WeaponSlots[i];
			if ( slot.IsUsed() && (int)( (Godot.Collections.Dictionary)slot.GetWeapon().Data.Get( "properties" ) )[ "ammo_type" ] == (int)( (Godot.Collections.Dictionary)ammo.Data.Get( "properties" ) )[ "type" ] ) {
				GetNode( "/root/Console" ).Call( "print_line", "Assigned weapon slot ammunition...", true );
				slot.GetWeapon().SetReserve( stack );
				slot.GetWeapon().SetAmmo( ammo.Data );
			}
		}

		LastUsedArm = ArmRight;

		if ( (int)GetNode( "/root/GameConfiguration" ).Get( "_game_mode" ) == (int)GameMode.Multiplayer ) {
			( (MultiplayerData)GetTree().CurrentScene ).SendPlayerUpdate( this );
		}
	}
	public void PickupWeapon( WeaponEntity weapon ) {
		GetNode( "/root/Console" ).Call( "print_line", "Picked up weapon...", true );

		int tmp = CurrentWeapon;
		for ( int i = 0; i < MAX_WEAPON_SLOTS; i++ ) {
			if ( !WeaponSlots[i].IsUsed() ) {
				GetNode( "/root/Console" ).Call( "print_line", "Assigned weapon slot " + i.ToString() + "...", true );
				WeaponSlots[i].SetWeapon( weapon );
				CurrentWeapon = i;
				break;
			}
		}
		if ( CurrentWeapon == tmp ) {
			return;
		}

		WeaponsStack.Add( weapon );

		TorsoAnimation.FlipH = false;
		LegAnimation.FlipH = false;

		AmmoStack stack = null;
		switch ( weapon.GetAmmoType() ) {
		case WeaponEntity.AmmoType.Light: {
			if ( AmmoLightStacks.Count > 0 ) {
				stack = AmmoLightStacks.Last();
			}
			break; }
		case WeaponEntity.AmmoType.Heavy: {
			if ( AmmoHeavyStacks.Count > 0 ) {
				stack = AmmoHeavyStacks.Last();
			}
			break; }
		case WeaponEntity.AmmoType.Pellets: {
			if ( AmmoPelletStacks.Count > 0 ) {
				stack = AmmoPelletStacks.Last();
			}
			break; }
		};
		if ( stack != null ) {
			weapon.SetReserve( stack );
			weapon.SetAmmo( stack.AmmoType );
		}
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_equip_weapon_on_pickup" ) ) {
			HUD.SetWeapon( weapon );

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

		if ( (int)GetNode( "/root/GameConfiguration" ).Get( "_game_mode" ) == (int)GameMode.Multiplayer ) {
			( (MultiplayerData)GetTree().CurrentScene ).SendPlayerUpdate( this );
		}
	}
};