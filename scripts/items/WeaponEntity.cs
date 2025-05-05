using System;
using Godot;
using Renown;

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

	private StringName ResourcePath = "";
	
	// c# integrated properties so that we aren't reaching into the engine api
	// every time we want a constant
	private Texture2D Icon;
	private int MagazineSize = 0;
	private AmmoType Ammunition = AmmoType.Light;
	private MagazineType MagType = MagazineType.Invalid;
	private FireMode Firemode = FireMode.Single;
	private SpriteFrames FirearmFramesLeft;
	private SpriteFrames FirearmFramesRight;
	private SpriteFrames BluntFramesLeft;
	private SpriteFrames BluntFramesRight;
	private SpriteFrames BladedFramesLeft;
	private SpriteFrames BladedFramesRight;
	private float BladedRange = 0.0f;
	private float BluntRange = 0.0f;
	private float BladedDamage = 0.0f;
	private float BluntDamage = 0.0f;
	private AudioStream UseFirearmSfx;
	private AudioStream UseBladedSfx;
	private AudioStream UseBluntSfx;
	private AudioStream ReloadSfx;
	private float UseTime = 0.0f;
	private float ReloadTime = 0.0f;
	private float Weight = 0.0f;

	private float AttackAngle = 0.0f;
	private float LastWeaponAngle = 0.0f;

	private NodePath InitialPath;

	private AnimatedSprite2D Animations;
	private Timer WeaponTimer;
	private Entity _Owner;
	private Sprite2D IconSprite;

	private Timer MuzzleFlashTimer;
	private PointLight2D MuzzleLight;
	private RayCast2D RayCast;
	private AmmoStack Reserve;
	private AmmoEntity Ammo;
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

	public void SetResourcePath( StringName path ) => ResourcePath = path;

	public float GetWeight() => Weight;
	public float GetReloadTime() => ReloadTime;
	public float GetUseTime() => UseTime;
	public NodePath GetInitialPath() => InitialPath;
	public void SetOwner( Entity owner ) => _Owner = owner;
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

	public bool IsBladed() => ( LastUsedMode & Properties.IsBladed ) != 0;
	public bool IsBlunt() => ( LastUsedMode & Properties.IsBlunt ) != 0;
	public bool IsFirearm() => ( LastUsedMode & Properties.IsFirearm ) != 0;

	public WeaponState GetCurrentState() => CurrentState;

	public void SetAttackAngle( float nAttackAngle ) => AttackAngle = nAttackAngle;
	public void OverrideRayCast( RayCast2D rayCast ) => RayCast = rayCast;

	private void PlaySound( AudioStream stream ) {
		AudioChannel.SetDeferred( "stream", stream );
		AudioChannel.CallDeferred( "play" );
	}

	private float RandomFloat( float min, float max ) {
		return (float)( min + RandomFactory.NextDouble() * ( min - max ) );
	}

	private void ReleasePickupArea() {
		if ( PickupArea == null ) {
			return;
		}
		PickupArea.GetChild( 0 ).CallDeferred( "queue_free" );
		PickupArea.CallDeferred( "remove_child", PickupArea.GetChild( 0 ) );

		PickupArea.CallDeferred( "queue_free" );
		CallDeferred( "remove_child", PickupArea );

		PickupArea = null;
	}

	public void TriggerPickup( Entity owner ) {
		IconSprite?.QueueFree();
		IconSprite = null;
		
		ReleasePickupArea();

		_Owner = owner;
		CallDeferred( "reparent", _Owner );
		GlobalPosition = _Owner.GlobalPosition;
		SetUseMode( DefaultMode );
		InitProperties();

		owner.PickupWeapon( this );
	}
	private void OnBodyShapeEntered( Rid BodyRID, Node2D body, int BodyShapeIndex, int LocalShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			IconSprite?.QueueFree();
			IconSprite = null;

			ReleasePickupArea();

			_Owner = entity;
			CallDeferred( "reparent", _Owner );
			GlobalPosition = _Owner.GlobalPosition;
			SetUseMode( DefaultMode );
			InitProperties();

			entity.PickupWeapon( this );
		}
	}
	private void OnMuzzleFlashTimerTimeout() {
		if ( Firemode == FireMode.Automatic ) {
			for ( int i = 0; i < MuzzleFlashes.Count; i++ ) {
				MuzzleFlashes[i].CallDeferred( "hide" );
			}
		} else {
			CurrentMuzzleFlash.CallDeferred( "hide" );
		}
		MuzzleLight.CallDeferred( "hide" );
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

		Animations = new AnimatedSprite2D();
		Animations.Name = "Animations";
		AddChild( Animations );

		WeaponTimer = new Timer();
		WeaponTimer.Name = "WeaponTimer";
		WeaponTimer.OneShot = true;
		AddChild( WeaponTimer );

		AudioChannel = new AudioStreamPlayer2D();
		AudioChannel.Name = "AudioChannel";
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( AudioChannel );

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );
		Weight = (float)Data.Get( "weight" );

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

			BladedFramesLeft = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/" + ResourcePath + (StringName)properties[ "bladed_frames_left" ] );
			BladedFramesRight = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/" + ResourcePath + (StringName)properties[ "bladed_frames_right" ] );

			UseBladedSfx = ResourceCache.GetSound( "res://sounds/" + ResourcePath + (StringName)properties[ "use_bladed" ] );

			if ( RayCast == null ) {
				RayCast = new RayCast2D();
				RayCast.Name = "RayCast";
				RayCast.Enabled = true;
				RayCast.TargetPosition = Godot.Vector2.Zero;
				AddChild( RayCast );
			}
		}
		if ( (bool)properties[ "is_blunt" ] ) {
			PropertyBits |= Properties.IsBlunt;
			BluntDamage = (float)properties[ "blunt_damage" ];
			BluntRange = (float)properties[ "blunt_range" ];

			BluntFramesLeft = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/" + ResourcePath + (StringName)properties[ "blunt_frames_left" ] );
			BluntFramesRight = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/" + ResourcePath + (StringName)properties[ "blunt_frames_right" ] );

			UseBluntSfx = ResourceCache.GetSound( "res://sounds/player/melee.wav" );
		}
		if ( (bool)properties[ "is_firearm" ] ) {
			PropertyBits |= Properties.IsFirearm;

			FirearmFramesLeft = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/" + ResourcePath + (StringName)properties[ "firearm_frames_left" ] );
			FirearmFramesRight = ResourceLoader.Load<SpriteFrames>( "res://resources/animations/" + ResourcePath + (StringName)properties[ "firearm_frames_right" ] );

			Firemode = (FireMode)(uint)properties[ "firemode" ];
			MagType = (MagazineType)(uint)properties[ "magazine_type" ];
			MagazineSize = (int)properties[ "magsize" ];
			Ammunition = (AmmoType)(uint)properties[ "ammo_type" ];

			// only allocate muzzle flash sprites if we actually need them
			MuzzleFlashes = new System.Collections.Generic.List<Sprite2D>();
			RandomFactory = new Random( System.DateTime.Now.Year + System.DateTime.Now.Month + System.DateTime.Now.Day );

			for ( int i = 0;; i++ ) {
				if ( !FileAccess.FileExists( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" ) ) {
					break;
				}
				Sprite2D texture = new Sprite2D();
				texture.Name = "MuzzleFlash_" + i.ToString();
				texture.Texture = ResourceCache.GetTexture( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" );
				texture.Offset = new Godot.Vector2( 160.0f, 0.0f );
				texture.Scale = new Godot.Vector2( 0.309f, 0.219f );
				texture.Hide();

				Animations.AddChild( texture );
				MuzzleFlashes.Add( texture );
			}

			MuzzleFlashTimer = new Timer();
			MuzzleFlashTimer.Name = "MuzzleFlashTimer";
			MuzzleFlashTimer.WaitTime = 0.2f;
			MuzzleFlashTimer.OneShot = true;
			MuzzleFlashTimer.Connect( "timeout", Callable.From( OnMuzzleFlashTimerTimeout ) );
			AddChild( MuzzleFlashTimer );

			MuzzleLight = new PointLight2D();
			MuzzleLight.Name = "MuzzleLight";
			MuzzleLight.Texture = ResourceCache.Light;
			MuzzleLight.TextureScale = 5.0f;
			MuzzleLight.Energy = 2.5f;
			MuzzleLight.Color = new Color( "#db7800" );
			MuzzleLight.Hide();
			AddChild( MuzzleLight );

			ReloadSfx = ResourceCache.GetSound( "res://sounds/" + ResourcePath + (StringName)properties[ "reload_sfx" ] );
			UseFirearmSfx = (AudioStream)properties[ "use_firearm" ];

			if ( RayCast == null ) {
				RayCast = new RayCast2D();
				RayCast.Name = "RayCast";
				RayCast.Enabled = true;
				RayCast.TargetPosition = Godot.Vector2.Zero;
				AddChild( RayCast );
			}

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

		InitialPath = GetPath();
		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		}

		if ( !IsInGroup( "Archive" ) ) {
			AddToGroup( "Archive" );
		}

		if ( Data == null ) {
			Console.PrintError( "Cannot initialize WeaponEntity without a valid ItemDefinition (null)" );
			return;
		}

		Icon = (Texture2D)Data.Get( "icon" );

		if ( ResourcePath.IsEmpty ) {
			ResourcePath = "player/";
		}

		if ( _Owner != null ) {
			InitProperties();
			return;
		}

		CreatePickupBounds();

		if ( _Owner == null ) {
			IconSprite = new Sprite2D();
			IconSprite.Name = "IconSprite";
			IconSprite.Texture = Icon;
			IconSprite.UseParentMaterial = true;
			AddChild( IconSprite );
		}
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		if ( _Owner == null ) {
			return;
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
		using ( var writer = new SaveSystem.SaveSectionWriter( InitialPath ) ) {
			writer.SaveBool( "HasOwner", _Owner != null );
			if ( _Owner != null ) {
				writer.SaveString( "Owner", _Owner.GetPath() );
			}
		}
	}
	public void Load() {
		using ( var reader = ArchiveSystem.GetSection( InitialPath ) ) {
			if ( reader.LoadBoolean( "HasOwner" ) ) {
				CharacterBody2D owner = GetTree().Root.GetNode<CharacterBody2D>( reader.LoadString( "Owner" ) );
				CallDeferred( "OnBodyShapeEntered", owner.GetRid(), owner, 0, 0 );
			}
		}
	}

	private float UseBladed() {
		float angle = AttackAngle;
		
		RayCast.TargetPosition = Godot.Vector2.Right * BladedRange;

		if ( angle != LastWeaponAngle ) {
			// swung
			float damage = BladedRange / Mathf.DegToRad( ( Mathf.RadToDeg( angle ) - Mathf.RadToDeg( LastWeaponAngle ) + 360.0f ) % 360.0f );
			if ( RayCast.GetCollider() is Entity entity && entity != null && entity != _Owner ) {
				entity.Damage( _Owner, damage );
			}

			PlaySound( ResourceCache.GetSound( "res://sounds/player/melee.wav" ) );
			LastWeaponAngle = angle;
		}

		return 0.0f;
	}
	private float UseBlunt() {
		return 0.0f;
	}

	public void SetAmmo( AmmoEntity ammo ) {
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
		BulletShellMesh.AddShell( _Owner, Ammo.Data );
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

		if ( _Owner is Player player && player != null ) {
			if ( ( LastUsedMode & Properties.IsOneHanded ) != 0 ) {
				player.SetLastUsedArm( player.GetWeaponHand( this ) );

				// I can't think of a single gun that doesn't take both hands to reload
				player.SetHandsUsed( Player.Hands.Both ); 
			}
		}

		if ( MagType == MagazineType.Breech && Ammo != null ) {
			// ejecting shells
			for ( int i = 0; i < MagazineSize; i++ ) {
				SpawnShells();
			}
		}

		if ( ResourceCache.Initialized ) {
			WeaponTimer.SetDeferred( "wait_time", ReloadTime );
			WeaponTimer.Connect( "timeout", Callable.From( OnReloadTimeTimeout ) );
			WeaponTimer.CallDeferred( "start" );

			CurrentState = WeaponState.Reload;
			PlaySound( ReloadSfx );
		} else {
			OnReloadTimeTimeout();
		}

		return true;
	}

	private GodotObject CheckBulletHit( ref float frameDamage ) {
		float damage = Ammo.GetDamage();
		frameDamage += damage;
		if ( RayCast.GetCollider() is GodotObject collision && collision != null ) {
			GD.Print( "hit object " + collision );
			if ( collision is Entity entity && entity != null ) {
				float distance = _Owner.GlobalPosition.DistanceTo( entity.GlobalPosition );
				if ( distance > 20.0f ) {
					// out of bleed range, no healing
					frameDamage -= damage;
				}
				distance /= Ammo.GetRange();
				damage *= Ammo.GetDamageFalloff( distance );
				entity.Damage( _Owner, damage );
			} else if ( collision is Hitbox hitbox && hitbox != null ) {
				hitbox.OnHit( _Owner );
			} else {
				frameDamage -= damage;
				DebrisFactory.Create( RayCast.GetCollisionPoint() );
			}
			return collision;
		}
		return null;
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

		CurrentMuzzleFlash = MuzzleFlashes[
			RandomFactory.Next( 0, MuzzleFlashes.Count - 1 )
		];
//		CurrentMuzzleFlash.Reparent( _Owner );
//		CurrentMuzzleFlash.Show();
		CurrentMuzzleFlash.GlobalRotation = AttackAngle;

		MuzzleLight.CallDeferred( "show" );
		
		MuzzleFlashTimer.Start();

		/*
		float y = CurrentMuzzleFlash.Offset.Y;
		if ( _Owner.GetLeftArm().Animations.FlipH ) {
			CurrentMuzzleFlash.Offset = new Godot.Vector2( -160, y );
		} else {
			CurrentMuzzleFlash.Offset = new Godot.Vector2( 160, y );
		}
		CurrentMuzzleFlash.FlipH = _Owner.GetLeftArm().Animations.FlipH;
		*/

		if ( MagType == MagazineType.Cycle ) {
			// ejecting shells
			SpawnShells();
		}

		RayCast.TargetPosition = Godot.Vector2.Right * soundLevel;
		PlaySound( UseFirearmSfx );
		float frameDamage = 0.0f;

		soundLevel = Ammo.GetRange();
		if ( Ammo.GetAmmoType() == AmmoType.Pellets ) {
			if ( Ammo.GetShotgunBullshit() != AmmoEntity.ShotgunBullshit.Slug ) {
				for ( int i = 0; i < Ammo.GetPelletCount(); i++ ) {
					// TODO: implement spread mechanics
					RayCast.TargetPosition = Godot.Vector2.Right.Rotated( Mathf.DegToRad( RandomFloat( 0.0f, 25.0f ) ) ) * soundLevel;
					CheckBulletHit( ref frameDamage );
				}
			} else {
				CheckBulletHit( ref frameDamage );
			}
		} else {
			GodotObject collision = CheckBulletHit( ref frameDamage );
			if ( collision != null ) {
				switch ( Ammo.GetEffects() ) {
				case AmmoEntity.ExtraEffects.Incendiary: {
					if ( collision is Entity entity && entity != null ) {
						entity.AddStatusEffect( "status_burning" );
					}
					break; }
				case AmmoEntity.ExtraEffects.Explosive: {
					if ( collision is Node2D node && node != null ) {
						ExplosionFactory.AddExplosion( node.GlobalPosition );
						if ( node is Entity entity && entity != null ) {
							entity.AddStatusEffect( "status_burning" );
						}
					}
					break; }
				};
			}
		}

		EmitSignalUsed( this );

		return frameDamage;
	}
	public void UseDeferred( Properties weaponMode ) {
		if ( Engine.TimeScale == 0.0f ) {
			return;
		}
		switch ( CurrentState ) {
		case WeaponState.Use:
		case WeaponState.Reload:
			return; // can't use it when it's being used
		};

		SetUseMode( weaponMode );

		if ( ( LastUsedMode & Properties.IsFirearm ) != 0 ) {
			UseFirearm( out _, false );
		} else if ( ( LastUsedMode & Properties.IsBlunt ) != 0 ) {
			UseBlunt();
		} else if ( ( LastUsedMode & Properties.IsBladed ) != 0 ) {
			UseBladed();
		}
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
		if ( ResourceCache.Initialized ) {
			WeaponTimer.Disconnect( "timeout", Callable.From( OnReloadTimeTimeout ) );
		}

		BulletsLeft = Reserve.RemoveItems( MagazineSize );
		if ( _Owner is Player player && player != null ) {
			if ( ( LastUsedMode & Properties.IsOneHanded ) != 0 ) {
				if ( player.GetLastUsedArm() == player.GetLeftArm() ) {
					player.SetHandsUsed( Player.Hands.Left );
				} else if ( player.GetLastUsedArm() == player.GetRightArm() ) {
					player.SetHandsUsed( Player.Hands.Right );
				}
			}
		}
		CurrentState = WeaponState.Idle;
		EmitSignalReloaded( this );
	}
};