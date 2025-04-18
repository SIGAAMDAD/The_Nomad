using System;
using Godot;

public enum AmmoType : uint {
	Heavy,
	Light,
	Pellets
};

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
	private AudioStream UseFirearmSfx;
	private AudioStream UseBladedSfx;
	private AudioStream UseBluntSfx;
	private AudioStream ReloadSfx;
	private float UseTime;
	private float ReloadTime;

	private PackedScene BulletShell;
	private PackedScene DustCloud;

	private AnimatedSprite2D Animations;
	private Timer WeaponTimer;
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

	private AudioStreamPlayer2D AudioChannel;

	private SpriteFrames AnimationsLeft;
	private SpriteFrames AnimationsRight;

	private Area2D PickupArea;

	private Properties PropertyBits = Properties.None;
	private Properties DefaultMode = Properties.None;
	private Properties LastUsedMode = Properties.None;

	[Signal]
	public delegate void ModeChangedEventHandler( WeaponEntity source, Properties useMode );
	[Signal]
	public delegate void ReloadedEventHandler( WeaponEntity source );
	[Signal]
	public delegate void UsedEventHandler( WeaponEntity source );

	public void SetOwner( Player player ) =>_Owner = player;
	public WeaponState GetWeaponState() => CurrentState;
	public void SetWeaponState( WeaponState state ) => CurrentState = state;
    public AmmoStack GetReserve() => Reserve;
	public int GetBulletCount() => BulletsLeft;
	public void SetBulletCount( int nBullets ) => BulletsLeft = nBullets;
	public Texture2D GetIcon() => Icon;
	public Properties GetLastUsedMode() => LastUsedMode;
	public AmmoType GetAmmoType() => Ammunition;

	public Properties GetProperties() => PropertyBits;
	public Properties GetDefaultMode() => DefaultMode;

	public SpriteFrames GetFramesLeft() => AnimationsLeft;
	public SpriteFrames GetFramesRight() => AnimationsRight;

	private void PlaySound( AudioStream stream ) {
		AudioChannel.Stream = stream;
		AudioChannel.Play();
	}

	private void ReleasePickupArea() {
		PickupArea.GetChild( 0 ).CallDeferred( "queue_free" );
		PickupArea.CallDeferred( "remove_child", PickupArea.GetChild( 0 ) );

		PickupArea.CallDeferred( "queue_free" );
		CallDeferred( "remove_child", PickupArea );
	}

	private void OnBodyShapeEntered( Rid BodyRID, Node2D body, int BodyShapeIndex, int LocalShapeIndex ) {
		if ( _Owner != null || body is not Player ) {
			return;
		}
		IconSprite?.QueueFree();

		Animations = new AnimatedSprite2D();
		AddChild( Animations );

		WeaponTimer = GetNode<Timer>( "WeaponTimer" );
		WeaponTimer.SetProcess( false );
		WeaponTimer.SetProcessInternal( false );
		WeaponTimer.OneShot = true;

		ReleasePickupArea();

		_Owner = (Player)body;
		CallDeferred( "reparent", _Owner );
		GlobalPosition = _Owner.GlobalPosition;
		SetUseMode( DefaultMode );
		InitProperties();
		_Owner.PickupWeapon( this );
	}
	private void OnMuzzleFlashTimerTimeout() {
		if ( Firemode == FireMode.Automatic ) {
			for ( int i = 0; i < MuzzleFlashes.Count; i++ ) {
				MuzzleFlashes[i].Hide();
			}
		} else {
			CurrentMuzzleFlash.Hide();
		}
		MuzzleLight.Hide();
	}

	private void CreatePickupBounds() {
		CircleShape2D circle = new CircleShape2D();
		circle.Radius = 7.0f;

		CollisionShape2D collision = new CollisionShape2D();
		collision.Shape = circle;

		PickupArea = new Area2D();
		PickupArea.CollisionLayer = 1 | 2 | 5;
		PickupArea.CollisionMask = 2 | 5;
		PickupArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnBodyShapeEntered ) );
		PickupArea.AddChild( collision );

		AddChild( PickupArea );
	}

	private void InitProperties() {
		PropertyBits = Properties.None;

		AudioChannel = new AudioStreamPlayer2D();
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( AudioChannel );

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );

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

			UseBladedSfx = (AudioStream)properties[ "use_bladed" ];
		}
		if ( (bool)properties[ "is_blunt" ] ) {
			PropertyBits |= Properties.IsBlunt;
			BluntDamage = (float)properties[ "blunt_damage" ];
			BluntRange = (float)properties[ "blunt_range" ];
			BluntFramesLeft = (SpriteFrames)properties[ "blunt_frames_left" ];
			BluntFramesRight = (SpriteFrames)properties[ "blunt_frames_right" ];

			UseBluntSfx = (AudioStream)properties[ "use_blunt" ];
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
				texture.Texture = ResourceCache.GetTexture( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" );
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
			MuzzleLight.Texture = ResourceCache.Light;
			MuzzleLight.TextureScale = 5.0f;
			MuzzleLight.Energy = 2.5f;
			MuzzleLight.Color = new Color( "#db7800" );
			MuzzleLight.Hide();
			AddChild( MuzzleLight );

			ReloadSfx = (AudioStream)properties[ "reload_sfx" ];
			UseFirearmSfx = (AudioStream)properties[ "use_firearm" ];

			RayCast = new RayCast2D();
			RayCast.Enabled = true;
			RayCast.TargetPosition = Godot.Vector2.Zero;
			AddChild( RayCast );

			UseTime = (float)properties[ "use_time" ];
			ReloadTime = (float)properties[ "reload_time" ];
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
			WeaponTimer.Stop();
			AudioChannel.Stop();
			return;
		}
	}

	public override void _Ready() {
		base._Ready();

		if ( Data == null ) {
			Console.PrintError( "Cannot initialize WeaponEntity without a valid ItemDefinition (null)" );
			return;
		}
		
		Icon = (Texture2D)Data.Get( "icon" );

		CreatePickupBounds();

		if ( _Owner == null ) {
			IconSprite = new Sprite2D();
			IconSprite.SetProcess( false );
			IconSprite.SetProcessInternal( false );
			IconSprite.Texture = Icon;
			AddChild( IconSprite );
		}
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		if ( ( LastUsedMode & Properties.IsFirearm ) != 0 ) {
			RayCast.GlobalRotation = _Owner.GetArmAngle();
		}
	}

    public void SetUseMode( Properties weaponMode ) {
		Properties cmp = LastUsedMode;

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

		if ( ( cmp & LastUsedMode ) != 0 ) {
			EmitSignalModeChanged( this, LastUsedMode );
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
		BulletShellMesh.AddShell( _Owner, Ammo );
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

		WeaponTimer.WaitTime = ReloadTime;
		WeaponTimer.Connect( "timeout", Callable.From( OnReloadTimeTimeout ) );
		WeaponTimer.Start();

		CurrentState = WeaponState.Reload;
		PlaySound( ReloadSfx );

		return true;
	}

	private float UseFirearm( out float soundLevel, bool held ) {
		soundLevel = 0.0f;
		if ( ( Ammo == null || BulletsLeft < 1 ) && ( ( ( Firemode == FireMode.Single || Firemode == FireMode.Burst ) && !held ) || Firemode == FireMode.Automatic ) ) {
			PlaySound( ResourceCache.NoAmmoSfx );
			return 0.0f;
		}

		if ( ( Firemode == FireMode.Single || Firemode == FireMode.Burst ) && held ) {
			return 0.0f;
		}

		switch ( Firemode ) {
		case FireMode.Single:
			BulletsLeft -= 1;
			break;
		case FireMode.Burst:
			BulletsLeft -= 2;
			break;
		case FireMode.Automatic:
			BulletsLeft--;
			break;
		case FireMode.Invalid:
		default:
			return 0.0f;
		};

		CurrentState = WeaponState.Use;
		WeaponTimer.WaitTime = UseTime;
		WeaponTimer.Connect( "timeout", Callable.From( OnUseTimeTimeout ) );
		WeaponTimer.Start();

		// bullets work like those in Halo 3.
		// start as a hitscan, then if we don't get a hit after 75% of the distance, turn it into a projectile
		// NOTE: correction, they WILL work like that eventually

		float angle = _Owner.GetArmAngle();

		CurrentMuzzleFlash = MuzzleFlashes[
			RandomFactory.Next( 0, MuzzleFlashes.Count - 1 )
		];
		CurrentMuzzleFlash.Show();
		CurrentMuzzleFlash.Rotation = angle;

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

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Ammo.Get( "properties" );

		soundLevel = (float)properties[ "range" ];
		RayCast.GlobalPosition = _Owner.GlobalPosition;
		RayCast.CollideWithAreas = true;
		RayCast.CollideWithBodies = true;
		RayCast.CollisionMask = 2 | 5;
		RayCast.TargetPosition = Godot.Vector2.Right * soundLevel;

		PlaySound( UseFirearmSfx );
		
		float damage = (float)properties[ "damage" ];
		if ( RayCast.GetCollider() is GodotObject collision && collision != null ) {
			if ( collision is Renown.Thinkers.MobBase entity && entity != null ) {
				float distance = _Owner.GlobalPosition.DistanceTo( entity.GlobalPosition );
				distance /= soundLevel;
				damage *= ( (Curve)properties[ "damage_falloff" ] ).SampleBaked( distance );

				entity.Damage( _Owner, damage );
			} else {
				DebrisFactory.Create( RayCast.GetCollisionPoint() );
			}
		}

		EmitSignalUsed( this );

		return 0.0f;
	}
	public float Use( Properties weaponMode, out float soundLevel, bool held = false ) {
		soundLevel = 0.0f;
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
			return UseFirearm( out soundLevel, held );
		} else if ( ( LastUsedMode & Properties.IsBlunt ) != 0 ) {
			return UseBlunt();
		} else if ( ( LastUsedMode & Properties.IsBladed ) != 0 ) {
			return UseBladed();
		}

		return 0.0f;
	}

	private void OnUseTimeTimeout() {
		WeaponTimer.Disconnect( "timeout", Callable.From( OnUseTimeTimeout ) );

		if ( ( LastUsedMode & Properties.IsFirearm ) != 0 ) {
			if ( BulletsLeft < 1 ) {
				Reload();
				return;
			}
		}
		CurrentState = WeaponState.Idle;
		EmitSignalUsed( this );
	}

	private void OnReloadTimeTimeout() {
		WeaponTimer.Disconnect( "timeout", Callable.From( OnReloadTimeTimeout ) );

		BulletsLeft = Reserve.RemoveItems( MagazineSize );
		if ( ( LastUsedMode & Properties.IsOneHanded ) != 0 ) {
			if ( _Owner.GetLastUsedArm() == _Owner.GetLeftArm() ) {
				_Owner.SetHandsUsed( Player.Hands.Left );
			} else if ( _Owner.GetLastUsedArm() == _Owner.GetRightArm() ) {
				_Owner.SetHandsUsed( Player.Hands.Right );
			}
		}
		CurrentState = WeaponState.Idle;
		EmitSignalReloaded( this );
	}

	public WeaponState GetCurrentState() {
		return CurrentState;
	}
};