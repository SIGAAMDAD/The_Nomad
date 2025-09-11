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

using Godot;
using Menus;
using Renown;
using ResourceCache;
using System;
using System.Runtime.CompilerServices;

namespace Items {
	public sealed partial class WeaponFirearm : WeaponEntity {
		private static readonly StringName @PropertiesPropertyName = "properties";

		private static readonly float RECOIL_DAMPING = 0.85f;
		private static readonly float BASE_RECOIL_FORCE = 0.5f;
		private static readonly float VELOCITY_RECOIL_FACTOR = 0.0005f;
		private static readonly float MIN_PITCH = 0.9f;
		private static readonly float MAX_PITCH = 1.1f;
		private static readonly float MIN_RESONANCE = 0.5f;
		private static readonly float MAX_RESONANCE = 5.0f;
		private static readonly float MIN_CUTOFF = 2000.0f;
		private static readonly float MAX_CUTOFF = 20000.0f;
		private static readonly float MIN_REVERB = 0.1f;
		private static readonly float MAX_REVERB = 0.8f;

		public int MagazineSize { get; private set; } = 0;
		public AmmoType Ammunition { get; private set; } = AmmoType.Light;
		public MagazineType MagType { get; private set; } = MagazineType.Invalid;
		public FireMode Firemode { get; private set; } = FireMode.Single;
		public float ReloadTime { get; private set; } = 0.0f;

		public Vector2 CurrentRecoilOffset { get; private set; } = Vector2.Zero;
		public float CurrentRecoilRotation { get; private set; } = 0.0f;

		public AmmoStack Ammo { get; private set; }
		public int BulletsLeft { get; private set; } = 0;

		private Sprite2D MuzzleLight;
		private Sprite2D[] MuzzleFlashes;
		private Sprite2D CurrentMuzzleFlash;

		/*
		===============
		IsBladed
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool IsBladed() {
			return false;
		}

		/*
		===============
		IsBlunt
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool IsBlunt() {
			return false;
		}

		/*
		===============
		IsFirearm
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool IsFirearm() {
			return true;
		}

		/*
		===============
		Save
		===============
		*/
		public void Save() {
			using var writer = new SaveSystem.SaveSectionWriter( GetPath(), ArchiveSystem.SaveWriter );

			SaveBase( writer );

			writer.SaveBool( "HasAmmo", Ammo != null );
			writer.SaveInt( nameof( BulletsLeft ), BulletsLeft );

			if ( Level > 0 ) {
				writer.SaveFloat( nameof( ReloadTime ), ReloadTime );
			}
			if ( Ammo != null ) {
				writer.SaveString( nameof( Ammo ), Ammo.GetPath() );
			}
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
		public override void Load( Entity holder, NodePath path ) {
			using var reader = ArchiveSystem.GetSection( path );
			if ( reader == null ) {
				return;
			}
			if ( Level > 0 ) {
				SetDeferred( PropertyName.ReloadTime, reader.LoadFloat( nameof( ReloadTime ) ) );
			}

			int bulletCount = reader.LoadInt( nameof( BulletsLeft ) );
			if ( ( PropertyBits & Properties.IsFirearm ) != 0 ) {
				if ( bulletCount == 0 ) {
					CurrentState = WeaponState.Reload;
				} else {
					CurrentState = WeaponState.Idle;
				}
				BulletsLeft = bulletCount;
			} else {
				CurrentState = WeaponState.Idle;
			}

			LoadBase( holder, reader );
		}

		/*
		===============
		ApplyUpgrade
		===============
		*/
		public override void ApplyUpgrade() {
			base.ApplyUpgrade();

			UseTime -= 0.01f;
			ReloadTime -= 0.05f;
		}

		/*
		===============
		Use
		===============
		*/
		public float Use( Properties useMode, out float soundLevel, bool held ) {
			soundLevel = 0.0f;

			switch ( CurrentState ) {
				case WeaponState.Use:
				case WeaponState.Reload:
					return 0.0f;
			}

			if ( Ammo == null || BulletsLeft < 1 ) {
				UseChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, AudioCache.GetStream( "res://sounds/weapons/noammo.wav" ) );
				UseChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );

				return 0.0f;
			}

			if ( !CanFire( held ) ) {
				return 0.0f;
			}

			int recoilMultiplier;
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

			float recoilMagnitude = ( BASE_RECOIL_FORCE + ( Ammo.AmmoType.Velocity * VELOCITY_RECOIL_FACTOR ) ) * recoilMultiplier;

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
				BulletShellMesh.AddShellDeferred( Holder, Ammo.AmmoType.ItemId );
			}

			UseChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.Stream, UseSfx );

			float ammoRatio = (float)BulletsLeft / MagazineSize;
			float cutoff = Mathf.Lerp( MIN_CUTOFF, MAX_CUTOFF, ammoRatio );
			float resonance = Mathf.Lerp( MAX_RESONANCE, MIN_RESONANCE, ammoRatio );
			float reverb = Mathf.Lerp( MAX_REVERB, MIN_REVERB, ammoRatio );

			UseChannel.SetDeferred( AudioStreamPlayer2D.PropertyName.PitchScale, (float)GD.RandRange( MIN_PITCH, MAX_PITCH ) );
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
		UseDeferred
		===============
		*/
		public void UseDeferred( Properties weaponMode ) {
			if ( Engine.TimeScale == 0.0f ) {
				return;
			}

			Use( weaponMode, out _, false );
			EmitSignalUsed( this );
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
		Reload
		===============
		*/
		public bool Reload() {
			if ( Ammo == null ) {
				return false;
			} else if ( Ammo.Amount < 1 && BulletsLeft < 1 ) {
				// no more ammo
				if ( MagType == MagazineType.Cycle ) {
					CurrentState = WeaponState.Empty;
				} else {
					CurrentState = WeaponState.Idle;
				}
				return false;
			}

			if ( Holder is Player player && player != null && IsOneHanded() ) {
				player.SetLastUsedArm( player.GetWeaponHand( this ) );

				// I can't think of a single gun that doesn't take at least two hands to reload
				player.SetHandsUsed( Player.Hands.Both );
			}

			if ( MagType == MagazineType.Breech ) {
				// ejecting shells
				for ( int i = 0; i < MagazineSize; i++ ) {
					BulletShellMesh.AddShellDeferred( Holder, Ammo.AmmoType.ItemId );
				}
			}

			CurrentState = WeaponState.Reload;

			WeaponTimer.SetDeferred( Timer.PropertyName.WaitTime, ReloadTime );
			WeaponTimer.CallDeferred( Timer.MethodName.Start );
			ReloadChannel.CallDeferred( AudioStreamPlayer2D.MethodName.Play );

			return true;
		}

		/*
		===============
		InitProperties
		===============
		*/
		protected override void InitProperties() {
			Godot.Collections.Dictionary properties = Data.Get( PropertiesPropertyName ).AsGodotDictionary();

			if ( !properties.TryGetValue( "is_firearm", out Variant isValid ) ) {
				Console.PrintError( "WeaponFirearm.InitProperties: property dictionary doesn't contain required key \"is_firearm\"!" );
				CallDeferred( MethodName.QueueFree );
				return;
			} else if ( !isValid.AsBool() ) {
				Console.PrintError( "WeaponEntity.InitProperties: ItemDefinition given to ItemPickup object is categorized as a firearm but isn't a... firearm?" );
				CallDeferred( MethodName.QueueFree );
				return;
			}

			base.InitProperties();

			PropertyBits |= Properties.IsFirearm;

			ReloadChannel = new AudioStreamPlayer2D() {
				Name = nameof( ReloadChannel ),
				VolumeDb = SettingsData.GetEffectsVolumeLinear(),
				Stream = AudioCache.GetStream( "res://sounds/weapons/" + properties[ "reload_sfx" ].AsStringName() )
			};
			AddChild( ReloadChannel );

			Firemode = (FireMode)properties[ "firemode" ].AsInt32();
			MagType = (MagazineType)properties[ "magazine_type" ].AsInt32();
			MagazineSize = properties[ "magsize" ].AsInt32();
			Ammunition = (AmmoType)properties[ "ammo_type" ].AsInt32();

			string resourcePath = Holder is Player ? "player/" : "";
			FramesLeft = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "firearm_frames_left" ] );
			FramesRight = SpriteFramesCache.GetSpriteFrames( "res://resources/animations/" + resourcePath + (StringName)properties[ "firearm_frames_right" ] );

			int count = 0;
			for ( int i = 0; !FileAccess.FileExists( $"res://textures/env/muzzle/mf{i}.dds" ); i++ ) {
				count++;
			}
			MuzzleFlashes = new Sprite2D[ count ];
			for ( int i = 0; i < count; i++ ) {
				MuzzleFlashes[ i ] = new Sprite2D() {
					Name = $"MuzzleFlash{i}",
					Offset = new Vector2( Icon.GetWidth(), 0.0f )
				};
				Animations.AddChild( MuzzleFlashes[ i ] );
			}

			MuzzleLight = new Sprite2D();
			MuzzleLight.Name = "MuzzleLight";
			MuzzleLight.Texture = TextureCache.GetTexture( "res://textures/point_light.dds" );
			MuzzleLight.Scale = new Godot.Vector2( 5.0f, 5.0f );
			MuzzleLight.Modulate = new Color { R = 4.5f, G = 3.5f, B = 2.5f };
			MuzzleLight.Hide();
			AddChild( MuzzleLight );

			UseSfx = (AudioStream)properties[ "use_firearm" ];

			UseTime = properties[ "use_time" ].AsSingle();
			ReloadTime = properties[ "reload_time" ].AsSingle();
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
			GameEventBus.DisconnectAllForObject( WeaponTimer );
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
		CanFire
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private bool CanFire( bool held ) {
			if ( ( Firemode == FireMode.Single || Firemode == FireMode.Burst ) && held ) {
				return false;
			} else if ( Dirtiness > 90.0f && RNJesus.IntRange( 0, 9 ) > 5 ) {
				return false;
			} else if ( Dirtiness > 80.0f && RNJesus.IntRange( 0, 19 ) > 13 ) {
				return false;
			}
			return true;
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
		GetDamageFromCollision
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float GetDistanceFromCollision( in Vector2 position ) {
			return Holder.GlobalPosition.DistanceTo( position );
		}

		/*
		===============
		HandleParryBoxHit
		===============
		*/
		private void HandleParryBoxHit( in Area2D parryBox, in RayIntersectionInfo collision ) {
			( (Player)parryBox.GetMeta( "Owner" ) ).OnParry(
				parryBox.GlobalPosition,
				collision.Position,
				Ammo.AmmoType.Damage * Ammo.AmmoType.DamageFalloff.SampleBaked( GetDistanceFromCollision( parryBox.GlobalPosition ) )
			);
		}

		/*
		===============
		HandleEntityHit
		===============
		*/
		private void HandleEntityHit( in Entity entity, ref float frameDamage ) {
			if ( Holder is Player ) {
				FreeFlow.Hitstop( 0.5f, 0.30f );
			}
			float distance = GetDistanceFromCollision( entity.GlobalPosition );
			float damage = Ammo.AmmoType.Damage;
			if ( distance < 120.0f ) {
				frameDamage += damage;
			}
			damage *= Ammo.AmmoType.DamageFalloff.SampleBaked( distance );
			entity.Damage( Holder, damage );

			ExtraAmmoEffects effects = Ammo.AmmoType.Effects;
			if ( ( effects & ExtraAmmoEffects.Incendiary ) != 0 ) {
				entity.AddStatusEffect( "status_burning" );
			} else if ( ( effects & ExtraAmmoEffects.Explosive ) != 0 ) {
				entity.CallDeferred( MethodName.AddChild, SceneCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			}
		}

		/*
		===============
		HandleHitboxHit
		===============
		*/
		/// <summary>
		/// Processes a hitbox getting hit
		/// </summary>
		/// <param name="hitbox"></param>
		/// <param name="frameDamage"></param>
		/// <param name="damage"></param>
		/// <exception cref="Exception"></exception>
		private void HandleHitboxHit( in Hitbox hitbox, ref float frameDamage ) {
			if ( Holder is Player ) {
				// slow motion for the extra feels
				FreeFlow.Hitstop( 0.25f, 0.50f );
			}
			if ( hitbox.GetMeta( "Owner" ).AsGodotObject() is not Entity owner ) {
				throw new Exception( $"Hitbox {hitbox.GetInstanceId()} owner isn't an Entity!" );
			}

			float distance = GetDistanceFromCollision( owner.GlobalPosition );
			float damage = Ammo.AmmoType.Damage;
			if ( distance < 120.0f ) {
				frameDamage += damage;
			}
			damage *= Ammo.AmmoType.DamageFalloff.SampleBaked( distance );

			hitbox.OnHit( Holder, damage );

			ExtraAmmoEffects effects = Ammo.AmmoType.Effects;
			if ( ( effects & ExtraAmmoEffects.Incendiary ) != 0 ) {
				owner.AddStatusEffect( "status_burning" );
			} else if ( ( effects & ExtraAmmoEffects.Explosive ) != 0 ) {
				owner.CallDeferred( MethodName.AddChild, SceneCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			}
		}

		private void HandleWallHit( ref float frameDamage, in RayIntersectionInfo collision ) {
			frameDamage -= Ammo.AmmoType.Damage;

			ExtraAmmoEffects effects = Ammo.AmmoType.Effects;
			if ( ( effects & ExtraAmmoEffects.Explosive ) != 0 ) {
				Explosion explosion = SceneCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>();
				explosion.GlobalPosition = collision.Position;
				collision.Collider.CallDeferred( MethodName.AddChild, explosion );
			}
			DebrisFactory.Create( in collision.Position );
		}

		/*
		===============
		CheckBulletHit
		===============
		*/
		private void CheckBulletHit( ref float frameDamage, float angle ) {
			RayIntersectionInfo collision = GodotServerManager.CheckRayCast( GlobalPosition, angle, Ammo.AmmoType.Range, Holder.GetRid() );
			if ( collision.Collider is Area2D parryBox && parryBox != null && parryBox.HasMeta( "ParryBox" ) ) {
				HandleParryBoxHit( in parryBox, in collision );
			} else if ( collision.Collider is Entity entity && entity != null && entity != Holder ) {
				HandleEntityHit( in entity, ref frameDamage );
			} else if ( collision.Collider is Grenade grenade && grenade != null ) {
				grenade.OnBlowup();
			} else if ( collision.Collider is Hitbox hitbox && hitbox != null && (Entity)hitbox.GetMeta( "Owner" ) != Holder ) {
				HandleHitboxHit( in hitbox, ref frameDamage );
			} else {
				HandleWallHit( ref frameDamage, in collision );
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();
		}

		/*
		===============
		_Process
		===============
		*/
		public override void _Process( double delta ) {
			base._Process( delta );

			CurrentRecoilOffset *= RECOIL_DAMPING;
			CurrentRecoilRotation *= RECOIL_DAMPING;

			if ( CurrentRecoilOffset.Length() < 0.1f ) {
				CurrentRecoilOffset = Godot.Vector2.Zero;
			}
			if ( Mathf.Abs( CurrentRecoilRotation ) < 0.1f ) {
				CurrentRecoilRotation = 0.0f;
			}
		}
	};
};