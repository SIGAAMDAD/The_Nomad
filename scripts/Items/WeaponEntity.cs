/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using PlayerSystem;
using Renown;
using ResourceCache;
using Menus;

public enum AmmoType : uint {
	Heavy,
	Light,
	Pellets,

	Count
};

public enum BladeAttackType : uint {
	Slash,
	Thrust
};

/*
===================================================================================

WeaponEntity

===================================================================================
*/

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
		IsOneHanded = 0b01000000,
		IsTwoHanded = 0b00100000,
		IsBladed = 0b00000001,
		IsBlunt = 0b00000010,
		IsFirearm = 0b00001000,

		OneHandedBlade = IsOneHanded | IsBladed,
		OneHandedBlunt = IsOneHanded | IsBlunt,
		OneHandedFirearm = IsOneHanded | IsFirearm,

		TwoHandedBlade = IsTwoHanded | IsBladed,
		TwoHandedBlunt = IsTwoHanded | IsBlunt,
		TwoHandedFirearm = IsTwoHanded | IsFirearm,

		SpawnsObject = 0b10000000,

		None = 0b00000000
	};

	public enum WeaponState : int {
		Idle,
		Use,
		Reload,

		// magazine fed specific states
		Empty,

		Invalid = -1
	};

	private static readonly float RecoilDamping = 0.85f;
	private static readonly float BaseRecoilForce = 0.5f;
	private static readonly float VelocityRecoilFactor = 0.0005f;
	private static readonly float MinPitch = 0.9f;
	private static readonly float MaxPitch = 1.1f;
	private static readonly float MinResonance = 0.5f;
	private static readonly float MaxResonance = 5.0f;
	private static readonly float MinCutoff = 2000.0f;
	private static readonly float MaxCutoff = 20000.0f;
	private static readonly float MinReverb = 0.1f;
	private static readonly float MaxReverb = 0.8f;

	[Export]
	public Resource Data;

	// c# integrated properties so that we aren't reaching into the engine api
	// every time we want a constant
	public Texture2D? Icon { get; private set; } = null;
	public int MagazineSize { get; private set; } = 0;
	public AmmoType Ammunition { get; private set; } = AmmoType.Light;
	public MagazineType MagType { get; private set; } = MagazineType.Invalid;
	public FireMode Firemode { get; private set; } = FireMode.Single;
	public SpriteFrames FirearmFramesLeft { get; private set; }
	public SpriteFrames FirearmFramesRight { get; private set; }
	public SpriteFrames BluntFramesLeft { get; private set; }
	public SpriteFrames BluntFramesRight { get; private set; }
	public SpriteFrames BladedFramesLeft { get; private set; }
	public SpriteFrames BladedFramesRight { get; private set; }
	public float BladedRange { get; private set; } = 0.0f;
	public float BluntRange { get; private set; } = 0.0f;
	public float BladedDamage { get; private set; } = 0.0f;
	public float BluntDamage { get; private set; } = 0.0f;
	public AudioStream UseFirearmSfx { get; private set; }
	public AudioStream UseBladedSfx { get; private set; }
	public AudioStream UseBluntSfx { get; private set; }
	public AudioStream ReloadSfx { get; private set; }
	public float UseTime { get; private set; } = 0.0f;
	public float ReloadTime { get; private set; } = 0.0f;
	public float Weight { get; private set; } = 0.0f;

	public int Level { get; private set; } = 0;
	public int MaxLevel { get; private set; } = 8;

	/// <summary>
	/// The more you use a weapon, the nastier it gets. If you don't clean it,
	/// it has the potential to jam or the degrade in quality
	/// </summary>
	public float Dirtiness { get; private set; } = 0.0f;

	public Vector2 CurrentRecoilOffset { get; private set; }
	public float CurrentRecoilRotation { get; private set; }

	public Timer WeaponTimer { get; private set; }
	public Entity Holder { get; private set; }

	public AmmoStack Ammo { get; private set; }
	public int BulletsLeft { get; private set; } = 0;

	public WeaponState CurrentState { get; private set; } = WeaponState.Idle;

	public SpriteFrames AnimationsLeft { get; private set; }
	public SpriteFrames AnimationsRight { get; private set; }

	public Properties PropertyBits { get; private set; } = Properties.None;
	public Properties DefaultMode { get; private set; } = Properties.None;
	public Properties LastUsedMode { get; private set; } = Properties.None;

	private float AttackAngle = 0.0f;
	private float LastWeaponAngle = 0.0f;

	private AnimatedSprite2D Animations;

	private Sprite2D MuzzleLight;
	private Sprite2D[] MuzzleFlashes;
	private Sprite2D CurrentMuzzleFlash;

	private ShaderMaterial BladedThrustShader;
	private ShaderMaterial BladedSlashShader;

	private AudioStreamPlayer2D UseChannel;
	private AudioStreamPlayer2D ReloadChannel;

	private AudioEffectReverb ReverbEffect;
	private int BusIndex;

	[Signal]
	public delegate void ModeChangedEventHandler( WeaponEntity source, Properties useMode );
	[Signal]
	public delegate void ReloadedEventHandler( WeaponEntity source );
	[Signal]
	public delegate void UsedEventHandler( WeaponEntity source );

	/*
	===============
	IsBladed
	===============
	*/
	/// <summary>
	/// Returns a boolean value if the <see cref="LastUsedMode"/> was <see cref="Properties.IsBladed"/>
	/// </summary>
	/// <returns>True if <see cref="LastUsedMode"/> is <see cref="Properties.IsBladed"/></returns>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool IsBladed() {
		return ( LastUsedMode & Properties.IsBladed ) != 0;
	}

	/*
	===============
	IsBlunt
	===============
	*/
	/// <summary>
	/// Returns a boolean value if the <see cref="LastUsedMode"/> was <see cref="Properties.IsBlunt"/>
	/// </summary>
	/// <returns>True if <see cref="LastUsedMode"/> is <see cref="Properties.IsBlunt"/></returns>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool IsBlunt() {
		return ( LastUsedMode & Properties.IsBlunt ) != 0;
	}

	/*
	===============
	IsFirearm
	===============
	*/
	/// <summary>
	/// Returns a boolean value if the <see cref="LastUsedMode"/> was <see cref="Properties.IsFirearm"/>
	/// </summary>
	/// <returns>True if <see cref="LastUsedMode"/> is <see cref="Properties.IsFirearm"/></returns>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool IsFirearm() {
		return ( LastUsedMode & Properties.IsFirearm ) != 0;
	}

	/*
	===============
	IsOneHanded
	===============
	*/
	/// <summary>
	/// Returns a boolean value if the <see cref="LastUsedMode"/> was <see cref="Properties.IsOneHanded"/>
	/// </summary>
	/// <returns>True if <see cref="LastUsedMode"/> is <see cref="Properties.IsOneHanded"/></returns>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool IsOneHanded() {
		return ( LastUsedMode & Properties.IsOneHanded ) != 0;
	}

	/*
	===============
	IsTwoHanded
	===============
	*/
	/// <summary>
	/// Returns a boolean value if the <see cref="LastUsedMode"/> was <see cref="Properties.IsTwoHanded"/>
	/// </summary>
	/// <returns>True if <see cref="LastUsedMode"/> is <see cref="Properties.IsTwoHanded"/></returns>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool IsTwoHanded() {
		return ( LastUsedMode & Properties.IsTwoHanded ) != 0;
	}

	/*
	===============
	SetAttackAngle
	===============
	*/
	/// <summary>
	/// Sets the weapon's in-game rotation
	/// </summary>
	/// <param name="attackAngle">The weapon's rotation angle</param>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void SetAttackAngle( float attackAngle ) {
		AttackAngle = attackAngle;
	}

	/*
	===============
	SetHolder
	===============
	*/
	public void SetHolder( Entity holder ) {
		Holder = holder;
	}

	/*
	===============
	Drop
	===============
	*/
	public void Drop() {
	}

	/*
	===============
	TriggerPickup
	===============
	*/
	public void TriggerPickup( Entity owner ) {
		Holder = owner;
		if ( IsInsideTree() ) {
			CallDeferred( MethodName.Reparent, Holder );
		} else {
			// most likely a multiplayer weapon spawn
			Holder.CallDeferred( MethodName.AddChild, this );
		}
		GlobalPosition = Holder.GlobalPosition;
		SetUseMode( DefaultMode );
		InitProperties();

		owner.PickupWeapon( this );
	}

	/*
	===============
	OnBodyShapeEntered
	===============
	*/
	private void OnBodyShapeEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Entity entity && entity != null ) {
			Holder = entity;
			CallDeferred( MethodName.Reparent, Holder );
			GlobalPosition = Holder.GlobalPosition;
			SetUseMode( DefaultMode );
			InitProperties();

			Material = null;

			entity.PickupWeapon( this );
		}
	}

	/*
	===============
	OnMuzzleFlashTimerTimeout
	===============
	*/
	private void OnMuzzleFlashTimerTimeout() {
		if ( Firemode == FireMode.Automatic ) {
			for ( int i = 0; i < MuzzleFlashes.Length; i++ ) {
				MuzzleFlashes[ i ].Texture = null;
			}
		} else {
			CurrentMuzzleFlash.Texture = null;
		}
		MuzzleLight.CallDeferred( MethodName.Hide );
	}

	/*
	===============
	InitProperties
	===============
	*/
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

		Godot.Collections.Dictionary properties = Data.Get( "properties" ).AsGodotDictionary();

		Weight = Data.Get( "weight" ).AsSingle();

		string resourcePath = Holder is Player ? "player/" : "";

		if ( properties[ "is_onehanded" ].AsBool() ) {
			PropertyBits |= Properties.IsOneHanded;
		}
		if ( properties[ "is_twohanded" ].AsBool() ) {
			PropertyBits |= Properties.IsTwoHanded;
		}
		if ( properties[ "is_bladed" ].AsBool() ) {
			PropertyBits |= Properties.IsBladed;
			BladedDamage = properties[ "bladed_damage" ].AsSingle();
			BladedRange = properties[ "bladed_range" ].AsSingle();

			BladedFramesLeft = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "bladed_frames_left" ] );
			BladedFramesRight = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "bladed_frames_right" ] );

			UseBladedSfx = AudioCache.GetStream( "res://sounds/weapons/" + (StringName)properties[ "use_bladed" ] );

			BladedSlashShader = ShaderMaterialCache.GetShaderMaterial( "res://resources/materials/bladed_slash_blur.gdshader" );
			ArgumentNullException.ThrowIfNull( BladedSlashShader );
			BladedSlashShader.ResourceLocalToScene = true;

			BladedThrustShader = ShaderMaterialCache.GetShaderMaterial( "res://resources/materials/bladed_thrust_blur.gdshader" );
			ArgumentNullException.ThrowIfNull( BladedThrustShader );
			BladedThrustShader.ResourceLocalToScene = true;
		}
		if ( properties[ "is_blunt" ].AsBool() ) {
			PropertyBits |= Properties.IsBlunt;
			BluntDamage = properties[ "blunt_damage" ].AsSingle();
			BluntRange = properties[ "blunt_range" ].AsSingle();

			BluntFramesLeft = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "blunt_frames_left" ] );
			BluntFramesRight = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "blunt_frames_right" ] );

			UseBluntSfx = AudioCache.GetStream( "res://sounds/player/melee.wav" );
		}
		if ( properties[ "is_firearm" ].AsBool() ) {
			PropertyBits |= Properties.IsFirearm;

			FirearmFramesLeft = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "firearm_frames_left" ] );
			FirearmFramesRight = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "firearm_frames_right" ] );

			Firemode = (FireMode)properties[ "firemode" ].AsInt32();
			MagType = (MagazineType)properties[ "magazine_type" ].AsInt32();
			MagazineSize = properties[ "magsize" ].AsInt32();
			Ammunition = (AmmoType)properties[ "ammo_type" ].AsInt32();

			ReloadChannel = new AudioStreamPlayer2D();
			ReloadChannel.Name = "ReloadChannel";
			ReloadChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			AddChild( ReloadChannel );

			//			BusIndex = AudioServer.BusCount;
			//			AudioServer.AddBus( BusIndex );
			//			AudioServer.SetBusName( BusIndex, "WeaponBus_" + GetInstanceId() );

			//			ReverbEffect = new AudioEffectReverb();
			//			AudioServer.AddBusEffect( BusIndex, ReverbEffect );

			//			UseChannel.Bus = "WeaponBus_" + GetInstanceId();

			int count = 0;
			for ( int i = 0; ; i++ ) {
				if ( !FileAccess.FileExists( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" ) ) {
					break;
				}
				count++;
			}
			MuzzleFlashes = new Sprite2D[ count ];
			for ( int i = 0; ; i++ ) {
				if ( !FileAccess.FileExists( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" ) ) {
					break;
				}
				Sprite2D texture = new Sprite2D();
				texture.Name = "MuzzleFlash_" + i.ToString();
				texture.Offset = new Godot.Vector2( Icon.GetWidth(), 0.0f );

				Animations.AddChild( texture );
				MuzzleFlashes[ i ] = texture;
			}

			MuzzleLight = new Sprite2D();
			MuzzleLight.Name = "MuzzleLight";
			MuzzleLight.Texture = TextureCache.GetTexture( "res://textures/point_light.dds" );
			MuzzleLight.Scale = new Godot.Vector2( 5.0f, 5.0f );
			MuzzleLight.Modulate = new Color { R = 4.5f, G = 3.5f, B = 2.5f };
			MuzzleLight.Hide();
			AddChild( MuzzleLight );

			ReloadSfx = AudioCache.GetStream( "res://sounds/weapons/" + properties[ "reload_sfx" ].AsStringName() );
			UseFirearmSfx = (AudioStream)properties[ "use_firearm" ];

			UseTime = properties[ "use_time" ].AsSingle();
			ReloadTime = properties[ "reload_time" ].AsSingle();
		}

		if ( properties[ "default_is_onehanded" ].AsBool() ) {
			DefaultMode |= Properties.IsOneHanded;
		}
		if ( properties[ "default_is_twohanded" ].AsBool() ) {
			DefaultMode |= Properties.IsTwoHanded;
		}
		if ( properties[ "default_is_bladed" ].AsBool() ) {
			DefaultMode |= Properties.IsBladed;
		}
		if ( properties[ "default_is_blunt" ].AsBool() ) {
			DefaultMode |= Properties.IsBlunt;
		}
		if ( properties[ "default_is_firearm" ].AsBool() ) {
			DefaultMode |= Properties.IsFirearm;
		}
		SetUseMode( DefaultMode );
	}

	/*
	===============
	SetEquippedState
	===============
	*/
	public void SetEquippedState( bool bEquipped ) {
		if ( !bEquipped ) {
			WeaponTimer.Stop();
			UseChannel.Stop();
			ReloadChannel.Stop();
			return;
		}
	}

	/*
	===============
	SetUseMode
	===============
	*/
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

	/*
	===============
	GetUpgradeCost
	===============
	*/
	public IReadOnlyDictionary<string, int> GetUpgradeCost() {
		switch ( Level ) {
			case 0:
				return new Dictionary<string, int> { [ "Scrap Metal" ] = 1 };
			default:
				break;
		}
		return new Dictionary<string, int>();
	}

	/*
	===============
	ApplyUpgrade
	===============
	*/
	public void ApplyUpgrade() {
		Level = Math.Min( Level + 1, MaxLevel );

		if ( ( PropertyBits & Properties.IsFirearm ) != 0 ) {
			UseTime -= 0.01f;
			ReloadTime -= 0.1f;
		}
		if ( ( PropertyBits & Properties.IsBladed ) != 0 ) {
			BladedDamage += 3.15f;
		}
		if ( ( PropertyBits & Properties.IsBlunt ) != 0 ) {
			BluntDamage += 3.15f;
		}
	}

	/*
	===============
	Save
	===============
	*/
	public void Save() {
		using var writer = new SaveSystem.SaveSectionWriter( GetPath(), ArchiveSystem.SaveWriter );

		writer.SaveString( "Id", Data.Get( "id" ).AsString() );
		writer.SaveBool( "HasAmmo", Ammo != null );
		writer.SaveInt( "Level", Level );
		if ( Level > 0 ) {
			writer.SaveFloat( "UseTime", UseTime );
			writer.SaveFloat( "ReloadTime", ReloadTime );
			writer.SaveFloat( "BluntDamage", BluntDamage );
			writer.SaveFloat( "BladedDamage", BladedDamage );
		}
		if ( Ammo != null ) {
			writer.SaveString( "Ammo", Ammo.GetPath() );
		}

		if ( Holder is Player player && player != null ) {
			bool equipped = false;
			for ( int i = 0; i < Player.MAX_WEAPON_SLOTS; i++ ) {
				if ( this == player.Inventory.WeaponSlots[ i ].Weapon ) {
					writer.SaveInt( "Slot", i );
					equipped = true;
					break;
				}
			}
			writer.SaveBool( "Equipped", equipped );
		}
		writer.SaveInt( "BulletsLeft", BulletsLeft );
	}

	/*
	===============
	Load
	===============
	*/
	/// <summary>
	/// Loads a WeaponEntity's state from disk
	/// </summary>
	/// <param name="holder">The holder of the WeaponEntity</param>
	/// <param name="path">The original node path that is used as a section name</param>
	public void Load( Entity holder, NodePath path ) {
		using var reader = ArchiveSystem.GetSection( path );

		ArgumentNullException.ThrowIfNull( reader );

		Holder = holder;

		CallDeferred( MethodName.SetData, reader.LoadString( "Id" ) );
		CallDeferred( MethodName.InitProperties );

		Level = reader.LoadInt( "Level" );
		if ( Level > 0 ) {
			SetDeferred( PropertyName.ReloadTime, reader.LoadFloat( "ReloadTime" ) );
			SetDeferred( PropertyName.UseTime, reader.LoadFloat( "UseTime" ) );
			SetDeferred( PropertyName.BluntDamage, reader.LoadFloat( "BluntDamage" ) );
			SetDeferred( PropertyName.BladedDamage, reader.LoadFloat( "BladedDamage" ) );
		}
		if ( Holder is Player player && player != null ) {
			int slot = WeaponSlot.INVALID;
			if ( reader.LoadBoolean( "Equipped" ) ) {
				slot = reader.LoadInt( "Slot" );
			}
			if ( reader.LoadBoolean( "HasAmmo" ) ) {
				player.Inventory.LoadWeapon( this, reader.LoadString( "ammo" ), slot );
			}
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
			CurrentState = WeaponState.Idle;
		}
	}

	/*
	===============
	SetData
	===============
	*/
	private void SetData( string id ) {
		Data = ItemCache.GetItem( id );
	}

	/*
	===============
	BladedThrustWindUp
	===============
	*/
	public void BladedThrustWindUp() {
		Material = BladedThrustShader;

		BladedThrustShader.SetShaderParameter( "shader_parameter/intensity", 0.0f );

		CreateTween().TweenMethod( Callable.From<float>( ( value ) =>
			BladedThrustShader.SetShaderParameter( "shader_parameter/intensity", value ) ), 0.0f, 2.0f, 0.45f );
	}

	/*
	===============
	UseBladed
	===============
	*/
	private float UseBladed( BladeAttackType attackType ) {
		Vector2 direction = new Vector2( 1.0f, 0.0f ).Rotated( AttackAngle );

		//
		// play audio
		//
		float angle = Mathf.Abs( AttackAngle );
		float pitch = 1.0f + ( angle / Mathf.Pi ) * 0.30f;

		UseChannel.Stream = UseBladedSfx;
		UseChannel.PitchScale = pitch;
		UseChannel.Play();

		if ( attackType == BladeAttackType.Slash ) {
			float skewAmount = Mathf.Clamp( 2.5f * 0.5f, -0.3f, 0.3f );

			Material = BladedSlashShader;

			( Holder as Player ).ArmRight.Animations.Skew = skewAmount;
			( Holder as Player ).ArmRight.Animations.Position = new Vector2( 2.0f * 2.5f, 0.0f ).Rotated( AttackAngle );

			Tween tween = CreateTween();
			BladedSlashShader.SetShaderParameter( "shader_parameter/progress", 0.0f );
			tween.TweenProperty( BladedSlashShader, "shader_parameter/progress", 1.0f, 0.15f ).SetEase( Tween.EaseType.Out );
			tween.Parallel().TweenProperty( ( Holder as Player ).ArmRight.Animations, "rotation", GlobalRotation + 3.45f, 0.15f ).SetEase( Tween.EaseType.Out );

			tween.TweenCallback( Callable.From( () => {
				Material = null;
				( Holder as Player ).ArmRight.Animations.Skew = 0.0f;
				( Holder as Player ).ArmRight.Animations.Position = Vector2.Zero;
			} ) );
		} else if ( attackType == BladeAttackType.Thrust ) {
			Tween tween = CreateTween();

			tween.TweenProperty( Holder, "global_position", Holder.GlobalPosition + ( direction * BladedRange * 2.0f ), 0.5f );
			BladedThrustShader.SetShaderParameter( "shader_parameter/intensity", 0.0f );
			tween.TweenCallback( Callable.From( () => Material = null ) );
		}

		return 0.0f;
	}

	/*
	===============
	UseBlunt
	===============
	*/
	private float UseBlunt() {
		return 0.0f;
	}

	/*
	===============
	SetAmmoStack
	===============
	*/
	public void SetAmmoStack( AmmoStack ammo ) {
		ArgumentNullException.ThrowIfNull( ammo );
		Ammo = ammo;
		if ( BulletsLeft < 1 ) {
			// force a reload
			Reload();
		}
	}

	/*
	===============
	SpawnShells
	===============
	*/
	private void SpawnShells() {
		BulletShellMesh.AddShellDeferred( Holder, Ammo.AmmoType.ItemId );
	}

	/*
	===============
	Reload
	===============
	*/
	private bool Reload() {
		if ( Ammo == null ) {
			return false;
		}
		if ( Ammo.Amount < 1 && BulletsLeft < 1 ) {
			// no more ammo
			if ( MagType == MagazineType.Cycle ) {
				CurrentState = WeaponState.Empty;
			} else {
				CurrentState = WeaponState.Idle;
			}
			return false;
		}

		if ( Holder is Player player && player != null ) {
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

		WeaponTimer.SetDeferred( Timer.PropertyName.WaitTime, ReloadTime );
		if ( !WeaponTimer.IsConnected( Timer.SignalName.Timeout, Callable.From( OnReloadTimeTimeout ) ) ) {
			WeaponTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnReloadTimeTimeout ) );
		}
		WeaponTimer.CallDeferred( Timer.MethodName.Start );

		CurrentState = WeaponState.Reload;

		ReloadChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, ReloadSfx );
		ReloadChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );

		return true;
	}

	/*
	===============
	CheckBulletHit
	===============
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="frameDamage"></param>
	/// <param name="angle"></param>
	private void CheckBulletHit( ref float frameDamage, float angle ) {
		// TODO: make this cleaner

		float damage = Ammo.AmmoType.Damage;
		frameDamage += damage;

		RayIntersectionInfo collision = GodotServerManager.CheckRayCast( GlobalPosition, angle, Ammo.AmmoType.Range, Holder.GetRid() );
		if ( collision.Collider is Area2D parryBox && parryBox != null && parryBox.HasMeta( "ParryBox" ) ) {
			float distance = Holder.GlobalPosition.DistanceTo( parryBox.GlobalPosition ) / Ammo.AmmoType.Range;
			damage *= Ammo.AmmoType.DamageFalloff.SampleBaked( distance );
			( (Player)parryBox.GetMeta( "Owner" ) ).OnParry( parryBox.GlobalPosition, collision.Position, damage );
		} else if ( collision.Collider is Entity entity && entity != null && entity != Holder ) {
			if ( Holder is Player ) {
				FreeFlow.Hitstop( 0.5f, 0.30f );
			}
			float distance = Holder.GlobalPosition.DistanceTo( entity.GlobalPosition ) / Ammo.AmmoType.Range;
			if ( distance > 120.0f ) {
				// out of bleed range, no healing
				frameDamage -= damage;
			}
			damage *= Ammo.AmmoType.DamageFalloff.SampleBaked( distance );
			entity.Damage( Holder, damage );

			ExtraAmmoEffects effects = Ammo.AmmoType.Effects;
			if ( ( effects & ExtraAmmoEffects.Incendiary ) != 0 ) {
				entity.AddStatusEffect( "status_burning" );
			} else if ( ( effects & ExtraAmmoEffects.Explosive ) != 0 ) {
				entity.CallDeferred( MethodName.AddChild, SceneCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			}
		} else if ( collision.Collider is Grenade grenade && grenade != null ) {
			grenade.OnBlowup();
		} else if ( collision.Collider is Hitbox hitbox && hitbox != null && (Entity)hitbox.GetMeta( "Owner" ) != Holder ) {
			if ( Holder is Player ) {
				// slow motion for the extra feels
				FreeFlow.Hitstop( 0.25f, 0.50f );
			}

			Entity owner = (Entity)hitbox.GetMeta( "Owner" );
			float distance = Holder.GlobalPosition.DistanceTo( ( (Entity)hitbox.GetMeta( "Owner" ) ).GlobalPosition ) / Ammo.AmmoType.Range;
			if ( distance > 120.0f ) {
				// out of bleed range, no healing
				frameDamage -= damage;
			}
			damage *= Ammo.AmmoType.DamageFalloff.SampleBaked( distance );

			hitbox.OnHit( Holder, damage );
			ExtraAmmoEffects effects = Ammo.AmmoType.Effects;
			if ( ( effects & ExtraAmmoEffects.Incendiary ) != 0 ) {
				( (Node2D)hitbox.GetMeta( "Owner" ) as Entity ).AddStatusEffect( "status_burning" );
			} else if ( ( effects & ExtraAmmoEffects.Explosive ) != 0 ) {
				owner.CallDeferred( MethodName.AddChild, SceneCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			}
		} else {
			frameDamage -= damage;
			ExtraAmmoEffects effects = Ammo.AmmoType.Effects;
			if ( ( effects & ExtraAmmoEffects.Explosive ) != 0 ) {
				Explosion explosion = SceneCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>();
				explosion.GlobalPosition = collision.Position;
				collision.Collider.CallDeferred( MethodName.AddChild, explosion );
			}
			DebrisFactory.Create( collision.Position );
		}
	}

	/*
	===============
	UseFirearm
	===============
	*/
	private float UseFirearm( out float soundLevel, bool held ) {
		soundLevel = 0.0f;
		if ( Ammo == null || BulletsLeft < 1 ) {
			ReloadChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, AudioCache.GetStream( "res://sounds/weapons/noammo.wav" ) );
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

		int recoilMultiplier = 0;
		switch ( Firemode ) {
			case FireMode.Single:
				BulletsLeft -= 1;
				recoilMultiplier = 1;
				break;
			case FireMode.Burst:
				BulletsLeft -= 2;
				recoilMultiplier = 2;
				break;
			case FireMode.Automatic:
				BulletsLeft--;
				recoilMultiplier = 1;
				break;
			case FireMode.Invalid:
			default:
				return 0.0f;
		}

		Vector2 recoil = -new Vector2(
			50.0f * Mathf.Cos( AttackAngle ),
			50.0f * Mathf.Sin( AttackAngle )
		);
		if ( Holder is Player ) {
			Player.ShakeCameraDirectional( Ammo.AmmoType.Velocity / 4.0f, recoil );
		}

		float recoilMagnitude = ( BaseRecoilForce + ( Ammo.AmmoType.Velocity * VelocityRecoilFactor ) ) * recoilMultiplier;

		CurrentRecoilOffset += recoil * recoilMagnitude;

		CurrentRecoilRotation += ( RNJesus.FloatRange( 0.0f, 1.0f ) > 0.5f ? 1.0f : -1.0f ) * recoilMagnitude * 0.5f;
		//		CurrentRecoilRotation = Mathf.Clamp( CurrentRecoilRotation, -5.0f, 5.0f );

		CurrentState = WeaponState.Use;
		WeaponTimer.WaitTime = UseTime;
		WeaponTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnUseTimeTimeout ) );
		WeaponTimer.Start();

		// bullets work like those in Halo 3.
		// start as a hitscan, then if we don't get a hit after 75% of the distance, turn it into a projectile
		// NOTE: correction, they WILL work like that eventually

		int index = RNJesus.IntRange( 0, MuzzleFlashes.Length - 1 );
		CurrentMuzzleFlash = MuzzleFlashes[ index ];
		CurrentMuzzleFlash.Reparent( Holder );
		CurrentMuzzleFlash.GlobalRotation = AttackAngle;
		CurrentMuzzleFlash.Texture = TextureCache.GetTexture( "res://textures/env/muzzle/mf" + index.ToString() + ".dds" );

		MuzzleLight.CallDeferred( MethodName.Show );

		GpuParticles2D GunSmoke = SceneCache.GetScene( "res://scenes/effects/gun_smoke.tscn" ).Instantiate<GpuParticles2D>();
		GunSmoke.GlobalPosition = new Vector2( Icon.GetWidth(), 0.0f );
		CurrentMuzzleFlash.CallDeferred( MethodName.AddChild, GunSmoke );

		GetTree().CreateTimer( 0.2f ).Connect( Timer.SignalName.Timeout, Callable.From( OnMuzzleFlashTimerTimeout ) );

		/*
		float y = CurrentMuzzleFlash.Offset.Y;
		if ( Holder.GetLeftArm().Animations.FlipH ) {
			CurrentMuzzleFlash.Offset = new Godot.Vector2( -160, y );
		} else {
			CurrentMuzzleFlash.Offset = new Godot.Vector2( 160, y );
		}
		CurrentMuzzleFlash.FlipH = Holder.GetLeftArm().Animations.FlipH;
		*/

		if ( MagType == MagazineType.Cycle ) {
			// ejecting shells
			SpawnShells();
		}

		UseChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, UseFirearmSfx );

		float ammoRatio = (float)BulletsLeft / MagazineSize;
		float cutoff = Mathf.Lerp( MinCutoff, MaxCutoff, ammoRatio );
		float resonance = Mathf.Lerp( MaxResonance, MinResonance, ammoRatio );
		float reverb = Mathf.Lerp( MaxReverb, MinReverb, ammoRatio );

		UseChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.PitchScale, (float)GD.RandRange( MinPitch, MaxPitch ) );
		UseChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );
		float frameDamage = 0.0f;

		soundLevel = Ammo.AmmoType.Range;
		if ( Ammo.AmmoType.Type == AmmoType.Pellets ) {
			if ( Ammo.AmmoType.ShotgunBullshit != ShotgunBullshit.Slug ) {
				System.Threading.Tasks.Parallel.For( 0, Ammo.AmmoType.PelletCount,
					( i ) => CheckBulletHit( ref frameDamage, AttackAngle + Mathf.DegToRad( (float)GD.RandRange( 0.0f, 35.0f ) ) )
				);
			} else {
				CheckBulletHit( ref frameDamage, AttackAngle );
			}
		} else {
			CheckBulletHit( ref frameDamage, AttackAngle );
		}

		return frameDamage;
	}

	/*
	===============
	UseFirearmDeferred
	===============
	*/
	public void UseFirearmDeferred( Properties weaponMode ) {
		if ( Engine.TimeScale == 0.0f ) {
			return;
		}
		switch ( CurrentState ) {
			case WeaponState.Use:
			case WeaponState.Reload:
				return; // can't use it when it's being used
		}

		SetUseMode( weaponMode );

		UseFirearm( out _, false );

		EmitSignalUsed( this );
	}

	/*
	===============
	UseBladedDeferred
	===============
	*/
	public void UseBladedDeferred( Properties weaponMode, BladeAttackType attackType ) {
		if ( Engine.TimeScale == 0.0f ) {
			return;
		}
		switch ( CurrentState ) {
			case WeaponState.Use:
				return; // can't use it when it's being used
		}

		SetUseMode( weaponMode );

		UseBladed( attackType );

		EmitSignalUsed( this );
	}

	/*
	===============
	Use
	===============
	*/
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
		}
		;

		SetUseMode( weaponMode );

		return UseFirearm( out soundLevel, held );
	}

	/*
	===============
	Use
	===============
	*/
	public float Use( Properties weaponMode, out float soundLevel, BladeAttackType attackType ) {
		soundLevel = 0.0f;
		EmitSignalUsed( this );

		if ( Engine.TimeScale == 0.0f ) {
			return 0.0f;
		}
		switch ( CurrentState ) {
			case WeaponState.Use:
				return 0.0f; // can't use it when it's being used
		}

		SetUseMode( weaponMode );

		return UseBladed( attackType );
	}

	/*
	===============
	OnUseTimeTimeout
	===============
	*/
	private void OnUseTimeTimeout() {
		WeaponTimer.Disconnect( Timer.SignalName.Timeout, Callable.From( OnUseTimeTimeout ) );

		if ( ( LastUsedMode & Properties.IsFirearm ) != 0 ) {
			if ( BulletsLeft < 1 ) {
				Reload();
				return;
			}
		}
		CurrentState = WeaponState.Idle;
		EmitSignalUsed( this );
	}

	/*
	===============
	OnReloadTimeTimeout
	===============
	*/
	private void OnReloadTimeTimeout() {
		if ( WeaponTimer.IsConnected( Timer.SignalName.Timeout, Callable.From( OnReloadTimeTimeout ) ) ) {
			WeaponTimer.Disconnect( Timer.SignalName.Timeout, Callable.From( OnReloadTimeTimeout ) );
		}

		UseChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.PitchScale, 1.0f );

		BulletsLeft = Ammo.RemoveItems( MagazineSize );
		if ( Holder is Player player && player != null ) {
			if ( ( LastUsedMode & Properties.IsOneHanded ) != 0 ) {
				if ( player.LastUsedArm == player.ArmLeft ) {
					player.SetHandsUsed( Player.Hands.Left );
				} else if ( player.LastUsedArm == player.ArmRight ) {
					player.SetHandsUsed( Player.Hands.Right );
				}
			}
		}
		CurrentState = WeaponState.Idle;
		EmitSignalReloaded( this );
	}

	/*
	===============
	_Ready
	===============
	*/
	/// <summary>
	/// godot initialization override
	/// </summary>
	public override void _Ready() {
		base._Ready();

		if ( !IsInGroup( "Archive" ) ) {
			AddToGroup( "Archive" );
		}
		if ( !ArchiveSystem.IsLoaded() && Data == null ) {
			Console.PrintError( "Cannot initialize WeaponEntity without a valid ItemDefinition (null)" );
			QueueFree();
			return;
		}
		if ( ArchiveSystem.IsLoaded() ) {
			return;
		}

		Icon = (Texture2D)Data.Get( "icon" );

		if ( Holder != null && Data != null ) {
			InitProperties();
			return;
		}

		if ( GameConfiguration.GameMode == GameMode.Online || GameConfiguration.GameMode == GameMode.Multiplayer ) {
			//SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
		}
	}

	/*
	===============
	_Process
	===============
	*/
	public override void _Process( double delta ) {
		base._Process( delta );

		CurrentRecoilOffset *= RecoilDamping;
		CurrentRecoilRotation *= RecoilDamping;

		if ( CurrentRecoilOffset.Length() < 0.1f ) {
			CurrentRecoilOffset = Godot.Vector2.Zero;
		}
		if ( Mathf.Abs( CurrentRecoilRotation ) < 0.1f ) {
			CurrentRecoilRotation = 0.0f;
		}
	}
};