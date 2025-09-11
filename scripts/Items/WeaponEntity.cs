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
using PlayerSystem.Inventory;
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

	[Export]
	public Resource Data;

	public SpriteFrames? FramesLeft { get; protected set; }
	public SpriteFrames? FramesRight { get; protected set; }
	public Texture2D? Icon{ get; protected set; }
	public AudioStream UseSfx { get; protected set; }
	public float UseTime { get; protected set; } = 0.0f;
	public float Weight { get; protected set; } = 0.0f;

	public int Level { get; private set; } = 0;
	public int MaxLevel { get; private set; } = 8;

	/// <summary>
	/// The more you use a weapon, the nastier it gets. If you don't clean it,
	/// it has the potential to jam or the degrade in quality
	/// </summary>
	public float Dirtiness { get; protected set; } = 0.0f;

	/// <summary>
	/// The speed at which a weapon gets dirty
	/// </summary>
	public float FilthRate { get; protected set; } = 1.0f;

	public Timer WeaponTimer { get; private set; }
	public Entity Holder { get; private set; }

	public WeaponState CurrentState { get; protected set; } = WeaponState.Idle;

	public Properties PropertyBits { get; protected set; } = Properties.None;
	public Properties DefaultMode { get; protected set; } = Properties.None;
	public Properties LastUsedMode { get; protected set; } = Properties.None;

	protected float AttackAngle = 0.0f;
	protected float LastWeaponAngle = 0.0f;

	protected AnimatedSprite2D Animations;

	protected ShaderMaterial BladedThrustShader;
	protected ShaderMaterial BladedSlashShader;

	protected AudioStreamPlayer2D UseChannel;
	protected AudioStreamPlayer2D ReloadChannel;

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
	public virtual bool IsBladed() {
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
	public virtual bool IsBlunt() {
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
	public virtual bool IsFirearm() {
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
			InitProperties();

			Material = null;

			entity.PickupWeapon( this );
		}
	}

	/*
	===============
	InitProperties
	===============
	*/
	protected virtual void InitProperties() {
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
		/*
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
		*/
		/*
		if ( properties[ "is_blunt" ].AsBool() ) {
			PropertyBits |= Properties.IsBlunt;
			BluntDamage = properties[ "blunt_damage" ].AsSingle();
			BluntRange = properties[ "blunt_range" ].AsSingle();

			BluntFramesLeft = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "blunt_frames_left" ] );
			BluntFramesRight = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "blunt_frames_right" ] );

			UseBluntSfx = AudioCache.GetStream( "res://sounds/player/melee.wav" );
		}
		*/

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
	public virtual void ApplyUpgrade() {
		Level = Math.Min( Level + 1, MaxLevel );

		FilthRate -= 0.15f;
	}

	/*
	===============
	Save
	===============
	*/
	public void SaveBase( SaveSystem.SaveSectionWriter writer ) {
		writer.SaveString( "Id", Data.Get( "id" ).AsString() );
		writer.SaveInt( "Level", Level );
		if ( Level > 0 ) {
			writer.SaveFloat( "UseTime", UseTime );
		}

		if ( Holder is Player player && player != null ) {
			bool equipped = false;
			for ( WeaponSlotIndex i = 0; i < WeaponSlotIndex.Count; i++ ) {
				if ( this == player.WeaponSlots[ i ].Weapon ) {
					writer.SaveInt( "Slot", (int)i );
					equipped = true;
					break;
				}
			}
			writer.SaveBool( "Equipped", equipped );
		}
	}

	/*
	===============
	Load
	===============
	*/
	public virtual void Load( Entity holder, NodePath path ) {
	}

	/*
	===============
	LoadBase
	===============
	*/
	protected void LoadBase( Entity holder, SaveSystem.SaveSectionReader reader ) {
		ArgumentNullException.ThrowIfNull( reader );

		Holder = holder;

		CallDeferred( MethodName.SetData, reader.LoadString( "Id" ) );
		CallDeferred( MethodName.InitProperties );

		Level = reader.LoadInt( "Level" );
		if ( Level > 0 ) {
			SetDeferred( PropertyName.UseTime, reader.LoadFloat( "UseTime" ) );
		}
		if ( Holder is Player player && player != null ) {
			WeaponSlotIndex slot = WeaponSlotIndex.Invalid;
			if ( reader.LoadBoolean( "Equipped" ) ) {
				slot = (WeaponSlotIndex)reader.LoadInt( "Slot" );
			}
			if ( reader.LoadBoolean( "HasAmmo" ) ) {
				player.Inventory.LoadWeapon( this, reader.LoadString( "ammo" ), (int)slot );
			}
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
	/*
	public void BladedThrustWindUp() {
		Material = BladedThrustShader;

		BladedThrustShader.SetShaderParameter( "shader_parameter/intensity", 0.0f );

		CreateTween().TweenMethod( Callable.From<float>( ( value ) =>
			BladedThrustShader.SetShaderParameter( "shader_parameter/intensity", value ) ), 0.0f, 2.0f, 0.45f );
	}
	*/

	/*
	===============
	UseBladed
	===============
	*/
	/*
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
	*/

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
};