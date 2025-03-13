using System;
using System.Data;
using System.Linq;
using Godot;

public partial class WeaponEntity : Node2D {
	public enum FireMode : int {
		Single,
		Burst,
		Automatic,

		Invalid = -1
	};

	public enum MagazineType : int {
		Breech, // breech fed
		Cycle, // uses a magazine

		Invalid = -1
	};

	public enum Properties : uint {
		IsOneHanded			= 0b01000000,
		IsTwoHanded			= 0b00100000,
		IsBladed			= 0b00000001,
		IsBlunt				= 0b00000010,
		IsFirearm			= 0b00001000,

		OneHandedBlade		= IsOneHanded | IsBladed,
		OneHandedBlunt		= IsOneHanded | IsBlunt,
		OneHandedFirearm	= IsOneHanded | IsFirearm,

		TwoHandedBlade		= IsTwoHanded | IsBladed,
		TwoHandedBlunt		= IsTwoHanded | IsBlunt,
		TwoHandedFirearm	= IsTwoHanded | IsFirearm,

		SpawnsObject		= 0b10000000,

		None				= 0b00000000 // here simply for the hell of it
	};

	public enum WeaponState : int {
		Idle,
		Use,
		Reload,

		// magazine fed specific states
		Empty,

		Invalid = -1
	};

	public enum AmmoType : uint {
		Heavy,
		Light,
		Pellets
	};

	public enum ShotgunBullshit : uint {
		Flechette,
		Buckshot,
		Birdshot,
		Shrapnel,
		Slug,

		None
	};

	[Export]
	public Resource Data;

	private Random RandomFactory;
	
	// c# integrated properties so that we aren't reaching into the engine api
	// every time we want a constant
	private Texture2D Icon;
	private int MagazineSize;
	private AmmoType Ammunition = AmmoType.Light;
	private MagazineType MagType = MagazineType.Invalid;
	private FireMode Firemode = FireMode.Invalid;
	private SpriteFrames FirearmFramesLeft;
	private SpriteFrames FirearmFramesRight;
	private SpriteFrames BluntFramesLeft;
	private SpriteFrames BluntFramesRight;
	private SpriteFrames BladedFramesLeft;
	private SpriteFrames BladedFramesRight;
	private float BladedRange;
	private float BluntRange;
	private float BladedDamage;
	private float BluntDamage;

	private PackedScene BulletShell;
	private PackedScene DustCloud;

	private AnimatedSprite2D Animations;
	private Timer UseTime;
	private AudioStreamPlayer2D UseBluntSfx;
	private AudioStreamPlayer2D UseBladedSfx;
	private AudioStreamPlayer2D UseFirearmSfx;
	private Player _Owner;
	private Sprite2D IconSprite;

	private Timer MuzzleFlashTimer;
	private PointLight2D MuzzleLight;
	private RayCast2D RayCast;
	private AmmoStack Reserve;
	private Resource Ammo;
	private System.Collections.Generic.List<Sprite2D> MuzzleFlashes;
	private Sprite2D CurrentMuzzleFlash;
	private int BulletsLeft = 0;

	private WeaponState CurrentState = WeaponState.Idle;

	private AudioStreamPlayer2D NoAmmoSound;
	private Timer ReloadTime;
	private AudioStreamPlayer2D ReloadSfx;

	private SpriteFrames AnimationsLeft;
	private SpriteFrames AnimationsRight;

	private Area2D PickupArea;

	private Properties PropertyBits = Properties.None;
	private Properties DefaultMode = Properties.None;
	private Properties LastUsedMode = Properties.None;

	~WeaponEntity() {
		if ( ReloadTime != null ) {
			ReloadTime.QueueFree();
		}
		if ( ReloadSfx != null ) {
			ReloadSfx.QueueFree();
		}
		if ( NoAmmoSound != null ) {
			NoAmmoSound.QueueFree();
		}
		if ( Reserve != null ) {
			Reserve.QueueFree();
		}
		if ( RayCast != null ) {
			RayCast.QueueFree();
		}
		if ( MuzzleFlashes.Count > 0 ) {
			for ( int i = 0; i < MuzzleFlashes.Count; i++ ) {
				MuzzleFlashes[i].QueueFree();
			}
		}
		MuzzleFlashes.Clear();
		if ( MuzzleFlashTimer != null ) {
			MuzzleFlashTimer.QueueFree();
		}
		if ( MuzzleLight != null ) {
			MuzzleLight.QueueFree();
		}
		if ( PickupArea != null ) {
			PickupArea.QueueFree();
		}
		if ( UseBladedSfx != null ) {
			UseBladedSfx.QueueFree();
		}
		if ( UseBluntSfx != null ) {
			UseBluntSfx.QueueFree();
		}
		if ( UseFirearmSfx != null ) {
			UseFirearmSfx.QueueFree();
		}

		IconSprite.QueueFree();
		UseTime.QueueFree();
	}

	public void SetOwner( Player player ) {
		_Owner = player;
	}
	public WeaponState GetWeaponState() {
		return CurrentState;
	}
	public void SetWeaponState( WeaponState state ) {
		CurrentState = state;
	}
    public AmmoStack GetReserve() {
		return Reserve;
	}
	public int GetBulletCount() {
		return BulletsLeft;
	}
	public void SetBulletCount( int nBullets ) {
		BulletsLeft = nBullets;
	}
	public Texture2D GetIcon() {
		return Icon;
	}
	public Properties GetLastUsedMode() {
		return LastUsedMode;
	}
	public AmmoType GetAmmoType() {
		return (AmmoType)(int)( (Godot.Collections.Dictionary)Data.Get( "properties" ) )[ "ammo_type" ];
	}

	public Properties GetProperties() {
		return PropertyBits;
	}
	public Properties GetDefaultMode() {
		return DefaultMode;
	}

	public SpriteFrames GetFramesLeft() {
		return AnimationsLeft;
	}
	public SpriteFrames GetFramesRight() {
		return AnimationsRight;
	}

	private void ReleasePickupArea() {
		PickupArea.GetChild( 0 ).CallDeferred( "queue_free" );
		PickupArea.CallDeferred( "remove_child", PickupArea.GetChild( 0 ) );

		PickupArea.CallDeferred( "queue_free" );
		CallDeferred( "remove_child", PickupArea );
	}

	private void OnBodyShapeEntered( Rid BodyRID, Node2D body, int BodyShapeIndex, int LocalShapeIndex ) {
		if ( _Owner != null ) {
			return;
		}
		if ( IconSprite != null ) {
			IconSprite.QueueFree();
		}

		ReleasePickupArea();

		_Owner = (Player)body;
		CallDeferred( "reparent", _Owner );
		GlobalPosition = _Owner.GlobalPosition;
		SetUseMode( DefaultMode );
		_Owner.PickupWeapon( this );
	}
	private void OnMuzzleFlashTimerTimeout() {
		CurrentMuzzleFlash.Hide();
		MuzzleLight.Hide();
	}

	private void CreatePickupBounds() {
		PickupArea = new Area2D();
		PickupArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnBodyShapeEntered ) );

		CircleShape2D circle = new CircleShape2D();
		circle.Radius = 7.0f;

		CollisionShape2D collision = new CollisionShape2D();
		collision.Shape = circle;

		PickupArea.AddChild( collision );

		AddChild( PickupArea );
	}

	private void InitProperties() {
		PropertyBits = Properties.None;

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );

		Icon = (Texture2D)Data.Get( "icon" );
		Firemode = (FireMode)(uint)properties[ "firemode" ];
		MagType = (MagazineType)(uint)properties[ "magazine_type" ];
		MagazineSize = (int)properties[ "magsize" ];
		Ammunition = (AmmoType)(uint)properties[ "ammo_type" ];

		if ( (bool)properties[ "is_onehanded" ] ) {
			PropertyBits |= Properties.IsOneHanded;
		}
		if ( (bool)properties[ "is_twohanded" ] ) {
			PropertyBits |= Properties.IsTwoHanded;
		}
		if ( (bool)properties[ "is_bladed" ] ) {
			PropertyBits |= Properties.IsBladed;
			BladedDamage = (float)properties[ "bladed_damage" ];
			BladedRange = (float)properties[ "bladed_range" ];
			BladedFramesLeft = (SpriteFrames)properties[ "bladed_frames_left" ];
			BladedFramesRight = (SpriteFrames)properties[ "bladed_frames_right" ];

			UseBladedSfx = new AudioStreamPlayer2D();
			UseBladedSfx.Stream = (AudioStream)properties[ "use_bladed" ];
			AddChild( UseBladedSfx );
		}
		if ( (bool)properties[ "is_blunt" ] ) {
			PropertyBits |= Properties.IsBlunt;
			BluntDamage = (float)properties[ "blunt_damage" ];
			BluntRange = (float)properties[ "blunt_range" ];
			BluntFramesLeft = (SpriteFrames)properties[ "blunt_frames_left" ];
			BluntFramesRight = (SpriteFrames)properties[ "blunt_frames_right" ];

			UseBluntSfx = new AudioStreamPlayer2D();
			UseBluntSfx.Stream = (AudioStream)properties[ "use_blunt" ];
			AddChild( UseBluntSfx );
		}
		if ( (bool)properties[ "is_firearm" ] ) {
			PropertyBits |= Properties.IsFirearm;
			FirearmFramesLeft = (SpriteFrames)properties[ "firearm_frames_left" ];
			FirearmFramesRight = (SpriteFrames)properties[ "firearm_frames_right" ];

			// only allocate muzzle flash sprites if we actually need them
			MuzzleFlashes = new System.Collections.Generic.List<Sprite2D>();
			RandomFactory = new Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day );

			for ( int i = 0;; i++ ) {
				if ( !FileAccess.FileExists( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" ) ) {
					break;
				}
				Sprite2D texture = new Sprite2D();
				texture.Texture = ResourceLoader.Load<Texture2D>( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" );
				texture.Offset = new Godot.Vector2( 160.0f, 0.0f );
				texture.Scale = new Godot.Vector2( 0.309f, 0.219f );
				texture.Hide();

				Animations.AddChild( texture );
				MuzzleFlashes.Add( texture );
			}

			MuzzleFlashTimer = new Timer();
			MuzzleFlashTimer.WaitTime = 0.2f;
			MuzzleFlashTimer.OneShot = true;
			MuzzleFlashTimer.Connect( "timeout", Callable.From( OnMuzzleFlashTimerTimeout ) );
			AddChild( MuzzleFlashTimer );

			MuzzleLight = new PointLight2D();
			MuzzleLight.Texture = ResourceLoader.Load<Texture2D>( "res://textures/2d_lights_and_shadows_neutral_point_light.webp" );
			MuzzleLight.TextureScale = 5.0f;
			MuzzleLight.Energy = 2.5f;
			MuzzleLight.Color = new Color( "#db7800" );
			MuzzleLight.Hide();
			AddChild( MuzzleLight );

			ReloadTime = new Timer();
			ReloadTime.OneShot = true;
			ReloadTime.WaitTime = (double)( (Godot.Collections.Dictionary)Data.Get( "properties" ) )[ "reload_time" ];
			ReloadTime.Connect( "timeout", Callable.From( OnReloadTimeTimeout ) );
			AddChild( ReloadTime );

			ReloadSfx = new AudioStreamPlayer2D();
			ReloadSfx.Stream = (AudioStream)( (Godot.Collections.Dictionary)Data.Get( "properties" ) )[ "reload_sfx" ];
			AddChild( ReloadSfx );

			NoAmmoSound = new AudioStreamPlayer2D();
			NoAmmoSound.Stream = ResourceLoader.Load<AudioStream>( "res://sounds/weapons/noammo.wav" );
			AddChild( NoAmmoSound );

			UseFirearmSfx = new AudioStreamPlayer2D();
			UseFirearmSfx.Stream = (AudioStream)properties[ "use_firearm" ];
			AddChild( UseFirearmSfx );

			RayCast = new RayCast2D();
			RayCast.Enabled = true;
			RayCast.TargetPosition = Godot.Vector2.Zero;
			AddChild( RayCast );
		}

		if ( (bool)properties[ "default_is_onehanded" ] ) {
			DefaultMode |= Properties.IsOneHanded;
		}
		if ( (bool)properties[ "default_is_twohanded" ] ) {
			DefaultMode |= Properties.IsTwoHanded;
		}
		if ( (bool)properties[ "default_is_bladed" ] ) {
			DefaultMode |= Properties.IsBladed;
		}
		if ( (bool)properties[ "default_is_blunt" ] ) {
			DefaultMode |= Properties.IsBlunt;
		}
		if ( (bool)properties[ "default_is_firearm" ] ) {
			DefaultMode |= Properties.IsFirearm;
		}
		SetUseMode( DefaultMode );
	}

	public void SetEquippedState( bool bEquipped ) {
		if ( !bEquipped ) {
			ReloadTime.Stop();
			ReloadSfx.Stop();
			return;
		}
	}
	public override void _Ready() {
		base._Ready();

		if ( Data == null ) {
			GD.PushError( "Cannot initialize WeaponEntity without a valid ItemDefinition (null)" );
			return;
		}

		Animations = new AnimatedSprite2D();
		AddChild( Animations );

		UseTime = new Timer();
		UseTime.WaitTime = (float)( (Godot.Collections.Dictionary)Data.Get( "properties" ) )[ "use_time" ];
		UseTime.OneShot = true;
		UseTime.Connect( "timeout", Callable.From( OnUseTimeTimeout ) );
		AddChild( UseTime );

		InitProperties();
		CreatePickupBounds();

		if ( _Owner == null ) {
			IconSprite = new Sprite2D();
			IconSprite.Texture = Icon;
			AddChild( IconSprite );
		}

		BulletShell = ResourceLoader.Load<PackedScene>( "res://scenes/effects/bullet_shell.tscn" );
		DustCloud = ResourceLoader.Load<PackedScene>( "res://scenes/effects/debris_cloud.tscn" );
	}

    public void SetUseMode( Properties weaponMode ) {
		LastUsedMode = weaponMode;
		if ( ( weaponMode & Properties.IsFirearm ) != 0 ) {
			AnimationsLeft = FirearmFramesLeft;
			AnimationsRight = FirearmFramesRight;
		} else if ( ( weaponMode & Properties.IsBlunt ) != 0 ) {
			AnimationsLeft = BluntFramesLeft;
			AnimationsRight = BluntFramesRight;
		} else if ( ( weaponMode & Properties.IsBladed ) != 0 ) {
			AnimationsLeft = BladedFramesLeft;
			AnimationsRight = BladedFramesRight;
		}
	}

	public void Save() {
	}
	public void Load() {
	}

	private float UseBladed() {
		return  0.0f;
	}
	private float UseBlunt() {
		return 0.0f;
	}

	public void SetAmmo( Resource ammo ) {
		Ammo = ammo;
	}
	public void SetReserve( AmmoStack stack ) {
		Reserve = stack;
		if ( BulletsLeft < 1 ) {
			// force a reload
			Reload();
		}
	}

	private void SpawnShells() {
		BulletShell bulletShell = BulletShell.Instantiate<BulletShell>();
		bulletShell.GlobalPosition = GlobalPosition;
		GetTree().CurrentScene.AddChild( bulletShell );

		switch ( Ammunition ) {
		case AmmoType.Light:
		case AmmoType.Heavy:
			bulletShell.GroundedSfx.Stream = WeaponSfxCache.BulletShellCasings[ RandomFactory.Next( 0, WeaponSfxCache.BulletShellCasings.Count - 1 ) ];
			break;
		case AmmoType.Pellets:
			bulletShell.GroundedSfx.Stream = WeaponSfxCache.ShotgunShellCasings[ RandomFactory.Next( 0, WeaponSfxCache.ShotgunShellCasings.Count - 1 ) ];
			break;
		};

		bulletShell.Texture = (Texture2D)( (Godot.Collections.Dictionary)Ammo.Get( "properties" ) )[ "casing_icon" ];
	}

	private bool Reload() {
		if ( Reserve == null ) {
			return false;
		}
		if ( Reserve.Amount < 1 && BulletsLeft < 1 ) {
			// no more ammo
			if ( MagType == MagazineType.Cycle ) {
				CurrentState = WeaponState.Empty;
			} else {
				CurrentState = WeaponState.Idle;
			}
			return false;
		}

		if ( ( LastUsedMode & Properties.IsOneHanded ) != 0 ) {
			_Owner.SetLastUsedArm( _Owner.GetWeaponHand( this ) );
			
			// I can't think of a single gun that doesn't take both hands to reload
			_Owner.SetHandsUsed( Player.Hands.Both ); 
		}

		if ( MagType == MagazineType.Breech && Ammo != null ) {
			// ejecting shells
			for ( int i = 0; i < MagazineSize; i++ ) {
				SpawnShells();
			}
		}

		ReloadTime.Start();
		CurrentState = WeaponState.Reload;
		ReloadSfx.Play();

		return true;
	}

	private float UseFirearm() {
		if ( Ammo == null || BulletsLeft < 1 ) {
			NoAmmoSound.Play();
			return 0.0f;
		}

		CurrentState = WeaponState.Use;
		UseTime.Start();

		switch ( Firemode ) {
		case FireMode.Single:
			BulletsLeft -= 1;
			break;
		case FireMode.Burst:
			BulletsLeft -= 2;
			break;
		case FireMode.Invalid:
		default:
			GD.PushError( "Invalid (almost literally) firemode: " + Firemode.ToString() );
			return 0.0f;
		};

		// bullets work like those in Halo 3.
		// start as a hitscan, then if we don't get a hit after 75% of the distance, turn it into a projectile
		// NOTE: correction, they WILL work like that eventually

		CurrentMuzzleFlash = MuzzleFlashes[
			RandomFactory.Next( 0, MuzzleFlashes.Count - 1 )
		];
		CurrentMuzzleFlash.Show();
		CurrentMuzzleFlash.Rotation = _Owner.GetArmAngle();

		MuzzleLight.Show();
		
		MuzzleFlashTimer.Start();

		float y = CurrentMuzzleFlash.Offset.Y;
		if ( _Owner.GetLeftArm().Animations.FlipH ) {
			CurrentMuzzleFlash.Offset = new Godot.Vector2( -160, y );
		} else {
			CurrentMuzzleFlash.Offset = new Godot.Vector2( 160, y );
		}

		CurrentMuzzleFlash.FlipH = _Owner.GetLeftArm().Animations.FlipH;
		if ( MagType == MagazineType.Cycle ) {
			// ejecting shells
			SpawnShells();
		}

		RayCast.Rotation = _Owner.GetArmAngle();
		RayCast.GlobalPosition = _Owner.GlobalPosition;

		UseFirearmSfx.Play();
		
		if ( RayCast.IsColliding() ) {
			GodotObject collision = RayCast.GetCollider();
			collision.Call( "Damage", this, (float)( (Godot.Collections.Dictionary)Ammo.Get( "properties" ) )[ "damage" ] );

			Node2D debris = DustCloud.Instantiate<Node2D>();
			GetTree().CurrentScene.AddChild( debris );
			debris.Call( "create", RayCast.GetCollisionPoint() );
		}

		return (float)( (Godot.Collections.Dictionary)Ammo.Get( "properties" ) )[ "damage" ];
	}

	public float Use( Properties weaponMode ) {
		if ( Engine.TimeScale == 0.0f ) {
			return 0.0f;
		}
		switch ( CurrentState ) {
		case WeaponState.Use:
		case WeaponState.Reload:
			return 0.0f; // can't use it when it's being used
		};

		SetUseMode( weaponMode );

		if ( ( LastUsedMode & Properties.IsFirearm ) != 0 ) {
			return UseFirearm();
		} else if ( ( LastUsedMode & Properties.IsBlunt ) != 0 ) {
			return UseBlunt();
		} else if ( ( LastUsedMode & Properties.IsBladed ) != 0 ) {
			return UseBladed();
		}

		return 0.0f;
	}

	private void OnUseTimeTimeout() {
		if ( ( LastUsedMode & Properties.IsFirearm ) != 0 ) {
			if ( BulletsLeft < 1 ) {
				Reload();
				return;
			}
		}
		CurrentState = WeaponState.Idle;
	}

	private void OnReloadTimeTimeout() {
		BulletsLeft = Reserve.RemoveItems( MagazineSize );
		if ( ( LastUsedMode & Properties.IsOneHanded ) != 0 ) {
			if ( _Owner.GetLastUsedArm() == _Owner.GetLeftArm() ) {
				_Owner.SetHandsUsed( Player.Hands.Left );
			} else if ( _Owner.GetLastUsedArm() == _Owner.GetRightArm() ) {
				_Owner.SetHandsUsed( Player.Hands.Right );
			}
		}

		CurrentState = WeaponState.Idle;
	}

	public WeaponState GetCurrentState() {
		return CurrentState;
	}
};