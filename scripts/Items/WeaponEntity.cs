using System;
using System.Collections.Generic;
using Godot;
using PlayerSystem;
using Renown;

public enum AmmoType : uint {
	Heavy,
	Light,
	Pellets
};

public partial class WeaponEntity : Node2D, PlayerSystem.Upgrades.IUpgradable {
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

		None				= 0b00000000
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

	// c# integrated properties so that we aren't reaching into the engine api
	// every time we want a constant
	public Texture2D Icon {
		get;
		private set;
	}
	private int MagazineSize = 0;
	public AmmoType Ammunition {
		get;
		private set;
	} = AmmoType.Light;
	public MagazineType MagType {
		get;
		private set;
	} = MagazineType.Invalid;
	public FireMode Firemode {
		get;
		private set;
	} = FireMode.Single;
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
	public float UseTime {
		get;
		private set;
	} = 0.0f;
	public float ReloadTime {
		get;
		private set;
	} = 0.0f;
	public float Weight {
		get;
		private set;
	} = 0.0f;

	public int Level { get; private set; } = 0;
	public int MaxLevel { get; private set; } = 8;

	/// <summary>
	/// The more you use a weapon, the nastier it gets. If you don't clean it,
	/// it has the potential to jam or the degrade in quality
	/// </summary>
	public float Dirtiness {
		get;
		private set;
	} = 0.0f;

	private float CurrentRecoil = 0.0f;

	private float AttackAngle = 0.0f;
	private float LastWeaponAngle = 0.0f;

	private AnimatedSprite2D Animations;
	private Timer WeaponTimer;
	public Entity _Owner;

	private Timer MuzzleFlashTimer;
	private PointLight2D MuzzleLight;
	private RayCast2D RayCast;
	public AmmoStack Reserve {
		get;
		private set;
	}
	public AmmoEntity Ammo {
		get;
		private set;
	}
	private List<Sprite2D> MuzzleFlashes;
	private Sprite2D CurrentMuzzleFlash;
	public int BulletsLeft {
		get;
		private set;
	} = 0;

	public WeaponState CurrentState {
		get;
		private set;
	} = WeaponState.Idle;

	private AudioStreamPlayer2D UseChannel;
	private AudioStreamPlayer2D ReloadChannel;

	public SpriteFrames AnimationsLeft {
		get;
		private set;
	}
	public SpriteFrames AnimationsRight {
		get;
		private set;
	}

	public Properties PropertyBits {
		get;
		private set;
	} = Properties.None;
	public Properties DefaultMode {
		get;
		private set;
	} = Properties.None;
	public Properties LastUsedMode {
		get;
		private set;
	} = Properties.None;

	private NetworkSyncObject SyncObject = new NetworkSyncObject( 24 );

	[Signal]
	public delegate void ModeChangedEventHandler( WeaponEntity source, Properties useMode );
	[Signal]
	public delegate void ReloadedEventHandler( WeaponEntity source );
	[Signal]
	public delegate void UsedEventHandler( WeaponEntity source );

	public bool IsBladed() => ( LastUsedMode & Properties.IsBladed ) != 0;
	public bool IsBlunt() => ( LastUsedMode & Properties.IsBlunt ) != 0;
	public bool IsFirearm() => ( LastUsedMode & Properties.IsFirearm ) != 0;

	public void SetAttackAngle( float nAttackAngle ) => AttackAngle = nAttackAngle;
	public void OverrideRayCast( RayCast2D rayCast ) => RayCast = rayCast;

	public void Drop() {
		UsedItemPickup pickup = ResourceCache.GetScene( "res://scenes/interactables/used_item_pickup.tscn" ).Instantiate<UsedItemPickup>();

		pickup.Entity = this;

		// unlink from the owner
		Reserve = null;

		pickup.GlobalPosition = _Owner.GlobalPosition;
		pickup.AdjustPosition();
		GetTree().CurrentScene.AddChild( pickup );

		_Owner.CallDeferred( "remove_child", this );
		_Owner = null;
	}
	public void TriggerPickup( Entity owner ) {
		_Owner = owner;
		if ( IsInsideTree() ) {
			CallDeferred( "reparent", _Owner );
		} else {
			// most likely a multiplayer weapon spawn
			_Owner.CallDeferred( "add_child", this );
		}
		GlobalPosition = _Owner.GlobalPosition;
		SetUseMode( DefaultMode );
		InitProperties();

		owner.PickupWeapon( this );
	}
	private void OnBodyShapeEntered( Rid BodyRID, Node2D body, int BodyShapeIndex, int LocalShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			_Owner = entity;
			CallDeferred( "reparent", _Owner );
			GlobalPosition = _Owner.GlobalPosition;
			SetUseMode( DefaultMode );
			InitProperties();

			Material = null;

			entity.PickupWeapon( this );
		}
	}
	private void OnMuzzleFlashTimerTimeout() {
		if ( Firemode == FireMode.Automatic ) {
			for ( int i = 0; i < MuzzleFlashes.Count; i++ ) {
				MuzzleFlashes[i].CallDeferred( MethodName.Hide );
			}
		} else {
			CurrentMuzzleFlash.CallDeferred( MethodName.Hide );
		}
		MuzzleLight.CallDeferred( MethodName.Hide );
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

		UseChannel = new AudioStreamPlayer2D();
		UseChannel.Name = "UseChannel";
		UseChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( UseChannel );

		ReloadChannel = new AudioStreamPlayer2D();
		ReloadChannel.Name = "ReloadChannel";
		ReloadChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( ReloadChannel );

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );
		Weight = (float)Data.Get( "weight" );

		string resourcePath = _Owner is Player ? "player/" : "";

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

			BladedFramesLeft = ResourceCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "bladed_frames_left" ] );
			BladedFramesRight = ResourceCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "bladed_frames_right" ] );

			UseBladedSfx = ResourceCache.GetSound( "res://sounds/" + resourcePath + (StringName)properties[ "use_bladed" ] );

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

			BluntFramesLeft = ResourceCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "blunt_frames_left" ] );
			BluntFramesRight = ResourceCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "blunt_frames_right" ] );

			UseBluntSfx = ResourceCache.GetSound( "res://sounds/player/melee.wav" );
		}
		if ( (bool)properties[ "is_firearm" ] ) {
			PropertyBits |= Properties.IsFirearm;

			FirearmFramesLeft = ResourceCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "firearm_frames_left" ] );
			FirearmFramesRight = ResourceCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "firearm_frames_right" ] );

			Firemode = (FireMode)(uint)properties[ "firemode" ];
			MagType = (MagazineType)(uint)properties[ "magazine_type" ];
			MagazineSize = (int)properties[ "magsize" ];
			Ammunition = (AmmoType)(uint)properties[ "ammo_type" ];

			// only allocate muzzle flash sprites if we actually need them
			MuzzleFlashes = new System.Collections.Generic.List<Sprite2D>();

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

			ReloadSfx = ResourceCache.GetSound( "res://sounds/" + resourcePath + (StringName)properties[ "reload_sfx" ] );
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
			UseChannel.Stop();
			ReloadChannel.Stop();
			return;
		}
	}

	private void NetworkSync( bool held = false ) {
		SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
		SyncObject.Write( GetPath().GetHashCode() );
		SyncObject.Write( (byte)CurrentState );
		SyncObject.Write( (uint)LastUsedMode );
		if ( CurrentState == WeaponState.Use ) {
			SyncObject.Write( held );
		}
		SyncObject.Sync();
	}
	private void ReceivePacket( System.IO.BinaryReader packet ) {
		SyncObject.BeginRead( packet );
		CurrentState = (WeaponState)SyncObject.ReadByte();
		LastUsedMode = (Properties)SyncObject.ReadUInt32();

		if ( CurrentState == WeaponState.Use ) {
			Use( LastUsedMode, out _, SyncObject.ReadBoolean() );
		}
	}

	public override void _Ready() {
		base._Ready();

		if ( !IsInGroup( "Archive" ) ) {
			AddToGroup( "Archive" );
		}

		if ( !ArchiveSystem.Instance.IsLoaded() && Data == null ) {
			Console.PrintError( "Cannot initialize WeaponEntity without a valid ItemDefinition (null)" );
			QueueFree();
			return;
		}
		if ( ArchiveSystem.Instance.IsLoaded() ) {
			return;
		}

		Icon = (Texture2D)Data.Get( "icon" );

		if ( _Owner != null && Data != null ) {
			InitProperties();
			return;
		}

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
//			SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
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

	public IReadOnlyDictionary<string, int> GetUpgradeCost() => Level switch {
		0 => new Dictionary<string, int> { [ "Scrap Metal" ] = 1 },
		_ => new Dictionary<string, int> { }
	};

	public void ApplyUpgrade() {
		Level = Math.Min( Level + 1, MaxLevel );

		if ( ( PropertyBits & Properties.IsFirearm ) != 0 ) {
			UseTime -= 0.01f;
			ReloadTime -= 0.01f;
		}
		if ( ( PropertyBits & Properties.IsBladed ) != 0 ) {
			BladedDamage += 3.15f;
		}
		if ( ( PropertyBits & Properties.IsBlunt ) != 0 ) {
			BluntDamage += 3.15f;
		}
	}

	public void Save() {
		using var writer = new SaveSystem.SaveSectionWriter( GetPath() );

		writer.SaveString( "Id", (string)Data.Get( "id" ) );
		writer.SaveBool( "HasAmmo", Ammo != null );
		if ( Ammo != null ) {
			writer.SaveString( "Ammo", Ammo.GetPath() );
		}

		if ( _Owner is Player player && player != null ) {
			bool equipped = false;
			for ( int i = 0; i < Player.MAX_WEAPON_SLOTS; i++ ) {
				if ( this == player.WeaponSlots[ i ].GetWeapon() ) {
					writer.SaveInt( "Slot", i );
					equipped = true;
					break;
				}
			}
			writer.SaveBool( "Equipped", equipped );
		}
		writer.SaveInt( "BulletsLeft", BulletsLeft );
	}
	public void Load( NodePath path ) {
		using var reader = ArchiveSystem.GetSection( path );

		CallDeferred( "SetData", reader.LoadString( "Id" ) );
		CallDeferred( "InitProperties" );
		if ( _Owner is Player player && player != null ) {
			int slot = WeaponSlot.INVALID;
			if ( reader.LoadBoolean( "Equipped" ) ) {
				slot = reader.LoadInt( "Slot" );
			}
			string ammo = null;
			if ( reader.LoadBoolean( "HasAmmo" ) ) {
				ammo = reader.LoadString( "Ammo" );
			}
			player.LoadWeapon( this, ammo, slot );
		}

		int nBulletCount = reader.LoadInt( "BulletsLeft" );
		if ( ( PropertyBits & Properties.IsFirearm ) != 0 ) {
			if ( nBulletCount == 0 ) {
				CurrentState = WeaponState.Reload;
			} else {
				CurrentState = WeaponState.Idle;
			}
			BulletsLeft = nBulletCount;
		} else {
			CurrentState = WeaponEntity.WeaponState.Idle;
		}
	}
	private void SetData( string id ) {
		Data = (Resource)ResourceCache.ItemDatabase.Call( "get_item", id );
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
		BulletShellMesh.AddShellDeferred( _Owner, Ammo.Data );
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
			WeaponTimer.SetDeferred( Timer.PropertyName.WaitTime, ReloadTime );
			if ( !WeaponTimer.IsConnected( Timer.SignalName.Timeout, Callable.From( OnReloadTimeTimeout ) ) ) {
				WeaponTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnReloadTimeTimeout ) );
			}
			WeaponTimer.CallDeferred( Timer.MethodName.Start );

			CurrentState = WeaponState.Reload;

			ReloadChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, ReloadSfx );
			ReloadChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
		} else {
			OnReloadTimeTimeout();
		}

		return true;
	}

	private void CheckBulletHit( ref float frameDamage ) {
		float damage = Ammo.GetDamage();
		frameDamage += damage;

		GodotObject collision = RayCast.GetCollider();
		if ( collision is Area2D parryBox && parryBox != null && parryBox.HasMeta( "ParryBox" ) ) {
			float distance = _Owner.GlobalPosition.DistanceTo( parryBox.GlobalPosition ) / Ammo.GetRange();
			damage *= Ammo.GetDamageFalloff( distance );
			( (Player)parryBox.GetMeta( "Owner" ) ).OnParry( RayCast, damage );
		} else if ( collision is Entity entity && entity != null && entity != _Owner ) {
			float distance = _Owner.GlobalPosition.DistanceTo( entity.GlobalPosition ) / Ammo.GetRange();
			if ( distance > 120.0f ) {
				// out of bleed range, no healing
				frameDamage -= damage;
			}
			damage *= Ammo.GetDamageFalloff( distance );
			entity.Damage( _Owner, damage );

			AmmoEntity.ExtraEffects effects = Ammo.GetEffects();
			if ( ( effects & AmmoEntity.ExtraEffects.Incendiary ) != 0 ) {
				entity.AddStatusEffect( "status_burning" );
			} else if ( ( effects & AmmoEntity.ExtraEffects.Explosive ) != 0 ) {
				entity.CallDeferred( MethodName.AddChild, ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			}
		} else if ( collision is Grenade grenade && grenade != null ) {
			grenade.OnBlowup();
		} else if ( collision is Hitbox hitbox && hitbox != null && (Entity)hitbox.GetMeta( "Owner" ) != _Owner ) {
			if ( _Owner is Player ) {
				// slow motion for the extra feels
				Engine.TimeScale = 0.5f;
				AudioServer.PlaybackSpeedScale = 0.5f;
				GetTree().CreateTimer( 0.30f ).Connect( Timer.SignalName.Timeout, Callable.From( () => {
					Engine.TimeScale = 1.0f;
					AudioServer.PlaybackSpeedScale = 1.0f;
				} ) );
			}

			Entity owner = (Entity)hitbox.GetMeta( "Owner" );
			float distance = _Owner.GlobalPosition.DistanceTo( ( (Entity)hitbox.GetMeta( "Owner" ) ).GlobalPosition ) / Ammo.GetRange();
			if ( distance > 120.0f ) {
				// out of bleed range, no healing
				frameDamage -= damage;
			}
			damage *= Ammo.GetDamageFalloff( distance );

			hitbox.OnHit( _Owner, damage );
			AmmoEntity.ExtraEffects effects = Ammo.GetEffects();
			if ( ( effects & AmmoEntity.ExtraEffects.Incendiary ) != 0 ) {
				( (Node2D)hitbox.GetMeta( "Owner" ) as Entity ).AddStatusEffect( "status_burning" );
			} else if ( ( effects & AmmoEntity.ExtraEffects.Explosive ) != 0 ) {
				owner.CallDeferred( MethodName.AddChild, ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			}
		} else {
			frameDamage -= damage;
			AmmoEntity.ExtraEffects effects = Ammo.GetEffects();
			if ( ( effects & AmmoEntity.ExtraEffects.Explosive ) != 0 ) {
				Explosion explosion = ResourceCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>();
				explosion.GlobalPosition = RayCast.GetCollisionPoint();
				collision.CallDeferred( MethodName.AddChild, explosion );
			}
			DebrisFactory.Create( RayCast.GetCollisionPoint() );
		}
	}
	private float UseFirearm( out float soundLevel, bool held ) {
		soundLevel = 0.0f;
		if ( ( Ammo == null || BulletsLeft < 1 ) && ( ( ( Firemode == FireMode.Single || Firemode == FireMode.Burst ) && !held ) || Firemode == FireMode.Automatic ) ) {
			ReloadChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, ResourceCache.NoAmmoSfx );
			ReloadChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
			return 0.0f;
		}

		bool canFire = true;
		if ( ( Firemode == FireMode.Single || Firemode == FireMode.Burst ) && held ) {
			canFire = false;
		} else {
			// check for jams
			if ( Dirtiness > 90.0f && RNJesus.IntRange( 0, 9 ) > 5 ) {
				canFire = false;
			} else if ( Dirtiness > 80.0f && RNJesus.IntRange( 0, 19 ) > 13 ) {
				canFire = false;
			}
		}

		if ( !canFire ) {
			return 0.0f;
		}
		
		Player.ShakeCameraDirectional( 40.0f, -new Godot.Vector2( 1.0f, 0.0f ).Rotated( LevelData.Instance.ThisPlayer.GetArmAngle() ) );

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
		}
		;

		CurrentState = WeaponState.Use;
		WeaponTimer.WaitTime = UseTime;
		WeaponTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnUseTimeTimeout ) );
		WeaponTimer.Start();

		// bullets work like those in Halo 3.
		// start as a hitscan, then if we don't get a hit after 75% of the distance, turn it into a projectile
		// NOTE: correction, they WILL work like that eventually

		CurrentMuzzleFlash = MuzzleFlashes[
			RNJesus.IntRange( 0, MuzzleFlashes.Count - 1 )
		];
		CurrentMuzzleFlash.Reparent( _Owner );
		CurrentMuzzleFlash.Show();
		CurrentMuzzleFlash.GlobalRotation = AttackAngle;

		MuzzleLight.CallDeferred( MethodName.Show );
		
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

		UseChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, UseFirearmSfx );
		UseChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
		float frameDamage = 0.0f;

		soundLevel = Ammo.GetRange();
		if ( Ammo.GetAmmoType() == AmmoType.Pellets ) {
			if ( Ammo.GetShotgunBullshit() != AmmoEntity.ShotgunBullshit.Slug ) {
				for ( int i = 0; i < Ammo.GetPelletCount(); i++ ) {
					// TODO: implement spread mechanics
					RayCast.TargetPosition = Godot.Vector2.Right.Rotated( Mathf.DegToRad( RNJesus.FloatRange( 0.0f, 35.0f ) ) ) * soundLevel;
					CheckBulletHit( ref frameDamage );
				}
			} else {
				CheckBulletHit( ref frameDamage );
			}
		} else {
			CheckBulletHit( ref frameDamage );
		}

		RayCast.TargetPosition = Godot.Vector2.Right * soundLevel;

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
		
		EmitSignalUsed( this );
	}
	public float Use( Properties weaponMode, out float soundLevel, bool held = false ) {
		soundLevel = 0.0f;
		EmitSignalUsed( this );
		
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

///		NetworkSync( held );

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
		if ( ResourceCache.Initialized && WeaponTimer.IsConnected( "timeout", Callable.From( OnReloadTimeTimeout ) ) ) {
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