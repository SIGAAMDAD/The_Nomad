/*
using Godot;
using System.Collections.Generic;

public partial class WeaponFirearm : WeaponEntity {
	public AmmoStack Reserve { get; private set; }
	public AmmoEntity Ammo { get; private set; }
	public int BulletsLeft { get; private set; } = 0;
	private Sprite2D MuzzleLight;
	private List<Sprite2D> MuzzleFlashes;
	private Sprite2D CurrentMuzzleFlash;

	public override void InitProperties() {
		base.InitProperties();

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );

		string resourcePath = _Owner is Player ? "player/" : "";

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

			BusIndex = AudioServer.BusCount;
			AudioServer.AddBus( BusIndex );
			AudioServer.SetBusName( BusIndex, "WeaponBus_" + GetInstanceId() );

			ReverbEffect = new AudioEffectReverb();
			AudioServer.AddBusEffect( BusIndex, ReverbEffect );

			UseChannel.Bus = "WeaponBus_" + GetInstanceId();

			for ( int i = 0; ; i++ ) {
				if ( !FileAccess.FileExists( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" ) ) {
					break;
				}
				Sprite2D texture = new Sprite2D();
				texture.Name = "MuzzleFlash_" + i.ToString();
				//				texture.Texture = ResourceCache.GetTexture( "res://textures/env/muzzle/mf" + i.ToString() + ".dds" );
				texture.Offset = new Godot.Vector2( Icon.GetWidth(), 0.0f );

				Animations.AddChild( texture );
				MuzzleFlashes.Add( texture );
			}

			MuzzleLight = new Sprite2D();
			MuzzleLight.Name = "MuzzleLight";
			MuzzleLight.Texture = ResourceCache.Light;
			MuzzleLight.Scale = new Godot.Vector2( 5.0f, 5.0f );
			MuzzleLight.Modulate = new Color { R = 4.5f, G = 3.5f, B = 2.5f };
			MuzzleLight.Hide();
			AddChild( MuzzleLight );

			ReloadSfx = ResourceCache.GetSound( "res://sounds/weapons/" + (StringName)properties[ "reload_sfx" ] );
			UseFirearmSfx = (AudioStream)properties[ "use_firearm" ];

			UseTime = (float)properties[ "use_time" ];
			ReloadTime = (float)properties[ "reload_time" ];
		} else {
			Console.PrintError( string.Format( "WeaponFirearm.InitProperties: weapon \"{0}\" isn't a firearm!", Data.Get( "id" ).AsString() ) );
		}
	}
	public void Save() {
		using var writer = new SaveSystem.SaveSectionWriter( GetPath() );

		base.SaveBase( writer );
		writer.SaveBool( "HasAmmo", Ammo != null );
		writer.SaveInt( "BulletsLeft", BulletsLeft );
		if ( Ammo != null ) {
			writer.SaveString( "Ammo", Ammo.GetPath() );
		}
	}


	public override float Use( Properties weaponMode, out float soundLevel, bool held ) {
		soundLevel = 0.0f;
		if ( Ammo == null || BulletsLeft < 1 ) {
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
		};

		Vector2 recoil = -new Vector2(
			50.0f * Mathf.Cos( AttackAngle ),
			50.0f * Mathf.Sin( AttackAngle )
		);
		if ( _Owner is Player ) {
			Player.ShakeCameraDirectional( Ammo.Velocity / 4.0f, recoil );
		}

		float recoilMagnitude = ( BaseRecoilForce + ( Ammo.Velocity * VelocityRecoilFactor ) ) * recoilMultiplier;

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

		int index = RNJesus.IntRange( 0, MuzzleFlashes.Count - 1 );
		CurrentMuzzleFlash = MuzzleFlashes[ index ];
		CurrentMuzzleFlash.Reparent( _Owner );
		CurrentMuzzleFlash.GlobalRotation = AttackAngle;
		CurrentMuzzleFlash.Texture = ResourceCache.GetTexture( "res://textures/env/muzzle/mf" + index.ToString() + ".dds" );

		MuzzleLight.CallDeferred( MethodName.Show );

		GpuParticles2D GunSmoke = ResourceCache.GetScene( "res://scenes/effects/gun_smoke.tscn" ).Instantiate<GpuParticles2D>();
		GunSmoke.GlobalPosition = new Vector2( Icon.GetWidth(), 0.0f );
		CurrentMuzzleFlash.CallDeferred( MethodName.AddChild, GunSmoke );

		GetTree().CreateTimer( 0.2f ).Connect( Timer.SignalName.Timeout, Callable.From( OnMuzzleFlashTimerTimeout ) );

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

		soundLevel = Ammo.Range;
		if ( Ammo.AmmoType == AmmoType.Pellets ) {
			if ( Ammo.ShotFlags != AmmoEntity.ShotgunBullshit.Slug ) {
				System.Threading.Tasks.Parallel.For( 0, Ammo.PelletCount,
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
	public override void _Ready() {
		base._Ready();
	}
};
*/