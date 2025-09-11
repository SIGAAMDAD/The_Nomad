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
using System;
using System.Runtime.CompilerServices;
using Menus;
using PlayerSystem.Upgrades;
using ResourceCache;
using PlayerSystem.Inventory;
using Items;
using PlayerSystem;
using PlayerSystem.Input;

public partial class Player {
	/*
	===================================================================================

	MovementHandler

	===================================================================================
	*/
	/// <summary>
	/// Handles all movement input signals
	/// </summary>

	public sealed partial class MovementHandler : GodotObject {
		public static readonly float ACCEL = 500.0f;
		public static readonly float FRICTION = 1000.0f;
		public static readonly float MAX_SPEED = 440.0f;
		public static readonly float JUMP_VELOCITY = -400.0f;

		private static readonly WeaponEntity.Properties[] WeaponModeList = [
			WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBladed,
			WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsBlunt,
			WeaponEntity.Properties.IsOneHanded | WeaponEntity.Properties.IsFirearm,

			WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBladed,
			WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsBlunt,
			WeaponEntity.Properties.IsTwoHanded | WeaponEntity.Properties.IsFirearm
		];

		private float WindUpProgress = 0.0f;
		private float WindUpDuration = 0.0f;
		private float IdleTime = 0.0f;
		private float NextShiftTime = 0.0f;

		private Player? Owner;

		private DashKit? DashKit;

		private Vector2 DashDirection = Vector2.Zero;
		private Vector2 InputVelocity = Vector2.Zero;

		/*
		===============
		MovementHandler
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="owner"></param>
		public MovementHandler( PlayerSystem.Input.IPlayerInput? input, Player? owner ) {
			ArgumentNullException.ThrowIfNull( owner );

			Owner = owner;

			DashKit = new DashKit( owner );
			GameEventBus.ConnectSignal( DashKit, DashKit.SignalName.DashBurnedOut, this, OnDashBurnout );
			GameEventBus.ConnectSignal( DashKit, DashKit.SignalName.DashBurnoutChanged, this, Callable.From<float>( ( dashBurnout ) => Owner.EmitSignal( Player.SignalName.DashBurnoutChanged, dashBurnout ) ) );
			GameEventBus.ConnectSignal( DashKit, DashKit.SignalName.DashEnd, this, OnDashEnd );

			ConnectBinds( input );
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		/// Updates the player's velocity based on <see cref="InputVelocity"/>
		/// </summary>
		/// <param name="delta"></param>
		public void Update( float delta ) {
			Vector2 velocity = Owner.Velocity;
			if ( InputVelocity == Vector2.Zero && velocity == Vector2.Zero ) {
				return;
			}

			if ( InputVelocity != Vector2.Zero ) {
				velocity = HandleAcceleration( in velocity, delta );

				float easedSpeedFactor = WindUpProgress < 0.1f ? 2.0f * WindUpProgress * WindUpProgress : 1.0f - Mathf.Pow( -2.0f * WindUpProgress + 2.0f, 2.0f ) / 2.0f;
				float armOffset = Mathf.Sin( Time.GetTicksMsec() / 120.0f ) * 2.0f * ( 1.0f - easedSpeedFactor );
				Owner.ArmLeft.Animations.SetDeferred( AnimatedSprite2D.PropertyName.Offset, new Vector2( 0.0f, armOffset ) );
			} else {
				velocity = HandleDeceleration( in velocity, delta );
				UpdateIdleBreath( delta );
			}

			if ( velocity != Godot.Vector2.Zero ) {
				Owner.IdleReset();
			}

			Owner.Velocity = velocity;
			Owner.MoveAndSlide();
		}

		/*
		===============
		IsInputBlocked
		===============
		*/
		/// <summary>
		/// <para>Checks if the player's input is currently being blocked.</para>
		/// <para>Returns true if <see cref="Player.Flags"/> have <see cref="Player.PlayerFlags.BlockedInput"/>
		/// or if we're currently interacting with the inventory.</para>
		/// <para>The latter is to prevent accidents occuring when messing with the inventory.</para>
		/// </summary>
		/// <param name="isInventory"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private bool IsInputBlocked( bool isInventory = false ) {
			return ( Owner.Flags & Player.PlayerFlags.BlockedInput ) != 0 || ( !isInventory && ( Owner.Flags & Player.PlayerFlags.Inventory ) != 0 );
		}

		/*
		===============
		OnDashBurnout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnDashBurnout() {
			Owner.RemoveFlags( Player.PlayerFlags.Dashing );
			Owner.AddChild( SceneCache.GetScene( "res://scenes/effects/explosion.tscn" ).Instantiate<Explosion>() );
			Owner.Damage( Owner, 30.0f );
			Owner.AddStatusEffect( "status_burning" );
			Player.ShakeCameraDirectional( 50.0f, DashDirection );

			Steam.SteamAchievements.ActivateAchievement( "ACH_AHHH_GAHHH_HAAAAAAA" );
		}

		/*
		===============
		OnMoveTriggered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="moveAction"></param>
		private void OnMoveTriggered( Resource moveAction ) {
			if ( IsInputBlocked() ) {
				return;
			}
			Owner.SetInputVelocity( moveAction.Get( "value_axis_2d" ).AsVector2() );
		}

		/*
		===============
		OnDashTriggered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dashAction"></param>
		private void OnDashTriggered( Resource dashAction ) {
			if ( IsInputBlocked() || !Owner.DashKit.CanDash() ) {
				return;
			}

			Owner.IdleReset();
			Owner.RemoveFlags( Player.PlayerFlags.Dashing );

			Owner.DashEffect.Emitting = true;
			Owner.DashLight.Show();
			DashDirection = Owner.Velocity;

			Owner.DashKit.OnDash();

			Owner.EmitSignal( Player.SignalName.DashStart );
		}

		/*
		===============
		OnDashEnd
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnDashEnd() {
			Owner.EmitSignal( Player.SignalName.DashEnd );
			Owner.DashLight.Hide();
			Owner.DashEffect.Emitting = false;
			Owner.RemoveFlags( Player.PlayerFlags.Dashing );
		}

		/*
		===============
		OnSlideTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSlideTimeout() {
			Owner.SlideEffect.Emitting = false;
			Owner.RemoveFlags( Player.PlayerFlags.Sliding );
		}

		/*
		===============
		OnSlide
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="slideAction"></param>
		private void OnSlide( Resource slideAction ) {
			if ( IsInputBlocked() || ( Owner.Flags & Player.PlayerFlags.Dashing ) != 0 ) {
				return;
			}
			Owner.IdleReset();
			Owner.AddFlags( Player.PlayerFlags.Sliding );
			Owner.SlideTime.Start();
			Owner.SlideEffect.Emitting = true;

			Owner.PlaySound( Owner.MiscChannel, SoundCache.GetEffectRange( SoundEffect.Slide0, 2 ) );

			Owner.LegAnimation.Play( "slide" );
		}

		private void OnUseWeaponTriggered( Resource useWeaponAction ) {
			if ( IsInputBlocked() ) {
				return;
			}

			Owner.IdleReset();

			WeaponSlotIndex slot = Owner.LastUsedArm.Slot;
			if ( slot == WeaponSlotIndex.Invalid ) {
				return; // nothing equipped
			}
			if ( Owner.WeaponSlots[ slot ].IsUsed() ) {
				WeaponEntity weapon = Owner.WeaponSlots[ slot ].Weapon;
				weapon.SetAttackAngle( Owner.ArmAngle );

				float soundLevel = 0.0f;
				/*
				if ( weapon.IsBladed() && ( Owner.Flags & Player.PlayerFlags.UsingMelee ) == 0 ) {
					Owner.AddFlags( Player.PlayerFlags.UsingMelee );

					if ( InputVelocity.X < 0.0f ) {
						bool slash = Owner.TorsoAnimation.FlipH;

						BladeAttackType = slash ? BladeAttackType.Slash : BladeAttackType.Thrust;
						if ( slash ) {
							FrameDamage += weapon.Use( weapon.LastUsedMode, out soundLevel, BladeAttackType );
						} else {
							weapon.BladedThrustWindUp();
						}
					} else if ( InputVelocity.X > 0.0f ) {
						bool slash = !TorsoAnimation.FlipH;

						BladeAttackType = slash ? BladeAttackType.Slash : BladeAttackType.Thrust;
						if ( slash ) {
							FrameDamage += weapon.Use( weapon.LastUsedMode, out soundLevel, BladeAttackType );
						} else {
							weapon.BladedThrustWindUp();
						}
					} else {
						BladeAttackType = BladeAttackType.Slash;
						FrameDamage += weapon.Use( weapon.LastUsedMode, out soundLevel, BladeAttackType );
					}
				} else
				*/
				if ( weapon.IsFirearm() ) {
					Owner.FrameDamage += ( weapon as WeaponFirearm ).Use( weapon.LastUsedMode, out soundLevel, ( Owner.Flags & Player.PlayerFlags.UsingWeapon ) != 0 );
				}

				Owner.EmitSignal( Player.SignalName.UsedWeapon, weapon );
				if ( Owner.FrameDamage > 0.0f ) {
					Owner.IncreaseBlood( Owner.FrameDamage * 0.0001f );
					FreeFlow.IncreaseCombo();
				}
				Owner.AddFlags( Player.PlayerFlags.UsingWeapon );
				Owner.SetSoundLevel( soundLevel );
			}
		}
		private void OnArmAngleChanged() {
			if ( IsInputBlocked() ) {
				return;
			}

			/*
			if ( CurrentMappingContext == ResourceCache.KeyboardInputMappings ) {
				GetArmAngle();
			} else {
				ArmAngle = ArmAngleAction.Get( "value_axis_2d" ).AsVector2().Angle();
			}
			*/
			Owner.AimAssist.GlobalRotation = Owner.ArmAngle;

			if ( GameConfiguration.GameMode != GameMode.Multiplayer ) {
				//SyncShadow();
			}
		}
		private void OnPrevWeaponTriggered( Resource prevWeaponAction ) {
			if ( IsInputBlocked() ) {
				return;
			}

			WeaponSlotIndex index = Owner.WeaponSlots.CurrentWeapon < 0 ? WeaponSlotIndex.Invalid - 1 : Owner.WeaponSlots.CurrentWeapon - 1;
			while ( index != WeaponSlotIndex.Invalid ) {
				if ( Owner.WeaponSlots[ index ].IsUsed() ) {
					break;
				}
				index--;
			}

			if ( index == WeaponSlotIndex.Invalid || !Owner.WeaponSlots[ index ].IsUsed() ) {
				index = WeaponSlotIndex.Invalid;
			}

			Arm otherArm;
			if ( Owner.LastUsedArm == Owner.ArmLeft ) {
				otherArm = Owner.ArmRight;
				Owner.SetHandsUsed( Player.Hands.Left );
			} else if ( Owner.LastUsedArm == Owner.ArmRight ) {
				otherArm = Owner.ArmLeft;
				Owner.SetHandsUsed( Player.Hands.Right );
			} else {
				Console.PrintError( "OnNextWeapon: invalid LastUsedArm" );
				Owner.SetLastUsedArm( Owner.ArmRight );
				return;
			}

			// adjust arm state
			if ( index != WeaponSlotIndex.Invalid ) {
				WeaponEntity weapon = Owner.WeaponSlots[ index ].Weapon;
				if ( ( weapon.LastUsedMode & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
					otherArm.SetSlot( WeaponSlotIndex.Invalid );
					Owner.SetHandsUsed( Player.Hands.Both );
				}

				Owner.EmitSignal( Player.SignalName.SwitchedWeapon, weapon );
				Owner.WeaponSlots[ index ].SetMode( Owner.WeaponSlots[ (WeaponSlotIndex)index ].Weapon.LastUsedMode );
			} else {
				Owner.EmitSignal( Player.SignalName.SwitchedWeapon, null );
			}
			Owner.PlaySound( Owner.MiscChannel, SoundCache.StreamCache[ SoundEffect.ChangeWeapon ] );

			Owner.WeaponSlots.SetEquippedWeapon( index );
			Owner.LastUsedArm.SetSlot( Owner.WeaponSlots.CurrentWeapon );

			Owner.EmitSignal( Player.SignalName.HandsStatusUpdated, (uint)Owner.HandsUsed );
		}
		private void OnNextWeaponTriggered( Resource nextWeaponAction ) {
			if ( IsInputBlocked() ) {
				return;
			}
			ArgumentNullException.ThrowIfNull( Owner );
			ArgumentNullException.ThrowIfNull( Owner.WeaponSlots );

			WeaponSlotIndex index = Owner.WeaponSlots.CurrentWeapon == WeaponSlotIndex.Invalid - 1 ? 0 : Owner.WeaponSlots.CurrentWeapon + 1;
			while ( index < WeaponSlotManager.MAX_WEAPON_SLOTS ) {
				if ( Owner.WeaponSlots[ index ].IsUsed() ) {
					break;
				}
				index++;
			}

			if ( index == WeaponSlotManager.MAX_WEAPON_SLOTS || !Owner.WeaponSlots[ index ].IsUsed() ) {
				index = WeaponSlotIndex.Invalid;
			}

			Arm otherArm;
			if ( Owner.LastUsedArm == Owner.ArmLeft ) {
				otherArm = Owner.ArmRight;
				Owner.SetHandsUsed( Player.Hands.Left );
			} else if ( Owner.LastUsedArm == Owner.ArmRight ) {
				otherArm = Owner.ArmLeft;
				Owner.SetHandsUsed( Player.Hands.Right );
			} else {
				Console.PrintError( "OnNextWeapon: invalid LastUsedArm" );
				Owner.SetLastUsedArm( Owner.ArmRight );
				return;
			}

			// adjust arm state
			if ( index != WeaponSlotIndex.Invalid ) {
				WeaponEntity? weapon = Owner.WeaponSlots[ index ].Weapon;
				ArgumentNullException.ThrowIfNull( weapon );

				if ( ( weapon.LastUsedMode & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
					otherArm.SetSlot( WeaponSlotIndex.Invalid );
					Owner.SetHandsUsed( Player.Hands.Both );
				}

				Owner.EmitSignal( Player.SignalName.SwitchedWeapon, weapon );
				Owner.WeaponSlots[ index ].SetMode( weapon.LastUsedMode );
			} else {
				Owner.EmitSignal( Player.SignalName.SwitchedWeapon, null );
			}
			Owner.PlaySound( Owner.MiscChannel, SoundCache.StreamCache[ SoundEffect.ChangeWeapon ] );

			Owner.WeaponSlots.SetEquippedWeapon( index );
			Owner.LastUsedArm.SetSlot( Owner.WeaponSlots.CurrentWeapon );

			Owner.EmitSignal( Player.SignalName.HandsStatusUpdated, (uint)Owner.HandsUsed );
		}
		private void OnBulletTimeTriggered( Resource bulletTimeAction ) {
			if ( IsInputBlocked() || Owner.StatManager.GetStatValue( "Rage" ) <= 0.0f ) {
				return;
			}

			Owner.IdleReset();
			Owner.EmitSignal( Player.SignalName.BulletTimeStart );

			if ( ( Owner.Flags & Player.PlayerFlags.BulletTime ) != 0 ) {
				Owner.ExitBulletTime();
			} else {
				Owner.PlaySound( Owner.MiscChannel, SoundCache.StreamCache[ SoundEffect.SlowmoBegin ] );

				Owner.AddFlags( Player.PlayerFlags.BulletTime );
				Engine.TimeScale = 0.40f;
			}
		}
		private void OnUseBothHands() {
			if ( IsInputBlocked() ) {
				return;
			}
			Owner.SetHandsUsed( Player.Hands.Both );
		}
		private void SwitchWeaponWielding() {
			if ( IsInputBlocked() ) {
				return;
			}
			ArgumentNullException.ThrowIfNull( Owner );

			Arm src;
			Arm dst;

			switch ( Owner.HandsUsed ) {
				case Player.Hands.Left:
					src = Owner.ArmLeft;
					dst = Owner.ArmRight;
					break;
				case Player.Hands.Right:
					src = Owner.ArmRight;
					dst = Owner.ArmLeft;
					break;
				case Player.Hands.Both:
				default:
					src = Owner.LastUsedArm;
					dst = src;
					break;
			}

			if ( src.Slot == WeaponSlotIndex.Invalid ) {
				// nothing in the source hand, deny
				return;
			}

			Owner.SetLastUsedArm( dst );

#if DEBUG
			ArgumentNullException.ThrowIfNull( Owner.WeaponSlots );
#endif

			WeaponEntity? srcWeapon = Owner.WeaponSlots[ src.Slot ].Weapon;
			ArgumentNullException.ThrowIfNull( srcWeapon );
			if ( ( srcWeapon.LastUsedMode & WeaponEntity.Properties.IsTwoHanded ) != 0 && ( srcWeapon.LastUsedMode & WeaponEntity.Properties.IsOneHanded ) == 0 ) {
				// cannot change hands, no one-handing allowed
				return;
			}

			// check if the destination hand has something in it, if true, then swap
			src.SwapSlot( dst );
			Owner.EmitSignal( Player.SignalName.WeaponStatusUpdated, srcWeapon, (uint)srcWeapon.LastUsedMode );
		}
		private void SwitchWeaponHand() {
			if ( IsInputBlocked() ) {
				return;
			}
			ArgumentNullException.ThrowIfNull( Owner );
			// TODO: implement use both hands

			switch ( Owner.HandsUsed ) {
				case Player.Hands.Left:
					Owner.SetHandsUsed( Player.Hands.Right ); // set to right
					Owner.SetLastUsedArm( Owner.ArmRight );
					break;
				case Player.Hands.Right:
					Owner.SetHandsUsed( Player.Hands.Left ); // set to left
					Owner.SetLastUsedArm( Owner.ArmLeft );
					break;
				case Player.Hands.Both:
					break;
				default:
					Console.PrintError( "Player.SwitchWeaponHand: invalid hand, setting to default of right" );
					Owner.SetHandsUsed( Player.Hands.Right );
					break;
			}
			if ( Owner.LastUsedArm.Slot != WeaponSlotIndex.Invalid ) {
				EquipSlot( Owner.LastUsedArm.Slot );
			}

			Owner.EmitSignal( Player.SignalName.SwitchHandUsed, (uint)Owner.HandsUsed );
		}
		public void EquipSlot( WeaponSlotIndex slot ) {
			Owner.WeaponSlots.SetEquippedWeapon( slot );

			WeaponEntity weapon = Owner.WeaponSlots[ slot ].Weapon;
			if ( weapon != null ) {
				// apply rules of various weapon properties
				if ( ( weapon.LastUsedMode & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
					Owner.ArmLeft.SetSlot( Owner.WeaponSlots.CurrentWeapon );
					Owner.ArmRight.SetSlot( Owner.WeaponSlots.CurrentWeapon );

					// this will automatically override any other modes
					Owner.WeaponSlots[ Owner.ArmLeft.Slot ].SetMode( weapon.DefaultMode );
				}

				Owner.WeaponSlots[ Owner.LastUsedArm.Slot ].SetMode( weapon.PropertyBits );
			} else {
				Owner.ArmLeft.SetSlot( WeaponSlotIndex.Invalid );
				Owner.ArmRight.SetSlot( WeaponSlotIndex.Invalid );
			}

			// update hand data
			Owner.LastUsedArm.SetSlot( Owner.WeaponSlots.CurrentWeapon );

			Owner.EmitSignal( Player.SignalName.SwitchedWeapon, weapon );
		}
		private void OnUseWeaponCompleted( Resource useWeaponAction ) {
			if ( ( Owner.Flags & Player.PlayerFlags.UsingMelee ) != 0 && Owner.BladeAttackType == BladeAttackType.Thrust ) {
				WeaponSlotIndex slot = Owner.LastUsedArm.Slot;
				if ( slot == WeaponSlotIndex.Invalid ) {
					return; // nothing equipped
				}
				if ( Owner.WeaponSlots[ slot ].IsUsed() ) {
					WeaponEntity weapon = Owner.WeaponSlots[ slot ].Weapon;
					weapon.SetAttackAngle( Owner.ArmAngle );
					if ( Owner.FrameDamage > 0.0f ) {
						Owner.IncreaseBlood( Owner.FrameDamage * 0.0001f );
						FreeFlow.IncreaseCombo();
					}
				}
			}

			Owner.RemoveFlags( Player.PlayerFlags.UsingWeapon | Player.PlayerFlags.UsingMelee );
		}

		private void OnMeleeFinished() {
			Owner.ArmLeft.Animations.AnimationFinished -= OnMeleeFinished;
			Owner.DeactivateParry();
		}
		private void OnMelee( Resource meleeAction ) {
			if ( IsInputBlocked() ) {
				return;
			}


			Owner.ArmLeft.Animations.SpriteFrames = Owner.DefaultLeftArmAnimations;
			Owner.ArmLeft.Animations.AnimationFinished += OnMeleeFinished;
			Owner.PlaySound( Owner.MiscChannel, AudioCache.GetStream( "res://sounds/player/melee.ogg" ) );
		}
		/*
		===============
		OnUseGadgetTriggered
		===============
		*/
		/// <summary>
		/// Handles gadget use input
		/// </summary>
		private void OnUseGadgetTriggered( Resource useGadgetAction ) {
			if ( IsInputBlocked() ) {
				return;
			}
			Owner.ArmAttachment?.Use();
		}

		/*
		===============
		OnInteractionTriggered
		===============
		*/
		private void OnInteractionTriggered( Resource interactionAction ) {
			if ( IsInputBlocked() ) {
				return;
			}
			Owner.EmitSignal( Player.SignalName.Interaction );
		}

		/*
		===============
		ConnectBinds
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		private void ConnectBinds( IPlayerInput? input ) {
			if ( input == null ) {
				throw new InvalidOperationException( "The PlayerSystem.Input.InputController must be initialized before MovementHandler!" );
			}

			input.SetBindAction( InputController.ControlBind.Move, GUIDEActionSignal.Triggered, OnMoveTriggered );
			input.SetBindAction( InputController.ControlBind.Dash, GUIDEActionSignal.Triggered, OnDashTriggered );
			input.SetBindAction( InputController.ControlBind.Slide, GUIDEActionSignal.Triggered, OnSlide );
			input.SetBindAction( InputController.ControlBind.Interact, GUIDEActionSignal.Triggered, OnInteractionTriggered );
		}

		/*
		===============
		ApplyDashingSpeedBonus
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float ApplyDashingSpeedBonus() {
			return ( Owner.Flags & Player.PlayerFlags.Dashing ) != 0 ? 8800.0f : 0.0f;
		}

		/*
		===============
		ApplySlidingSpeedBonus
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float ApplySlidingSpeedBonus() {
			return ( Owner.Flags & Player.PlayerFlags.Sliding ) != 0 ? 1200.0f : 0.0f;
		}

		/*
		===============
		ApplyEncumburedSpeed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float ApplyEncumburedSpeed() {
			return Owner.Inventory.TotalInventoryWeight >= InventoryManager.MAXIMUM_INVENTORY_WEIGHT * 0.85f ?
				0.5f : 1.0f;
		}

		/*
		===============
		HandleAcceleration
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		private Vector2 HandleAcceleration( in Vector2 velocity, float delta ) {
			float speed = MAX_SPEED * ApplyEncumburedSpeed();
			float accel = ACCEL + ApplyDashingSpeedBonus() + ApplySlidingSpeedBonus();

			WindUpProgress = Mathf.Clamp( WindUpProgress + WindUpDuration, 0.0f, 1.0f );
			IdleTime = 0.0f;

			return velocity.MoveToward( InputVelocity * speed, delta * accel );
		}

		/*
		===============
		HandleDeceleration
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="velocity"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		private Vector2 HandleDeceleration( in Vector2 velocity, float delta ) {
			WindUpProgress = Mathf.Clamp( WindUpProgress - WindUpDuration * 2.0f, 0.0f, 1.0f );
			return velocity.MoveToward( Vector2.Zero, delta * FRICTION );
		}

		/*
		===============
		UpdateIdleBreath
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		private async void UpdateIdleBreath( float delta ) {
			if ( SettingsData.AnimationQuality < AnimationQuality.Medium ) {
				return;
			}

			IdleTime += delta;

			float breath = Mathf.Sin( IdleTime * 8.5f ) * 0.30f;
			Owner.TorsoAnimation.Position = new Vector2( 0.0f, breath );

			if ( IdleTime > NextShiftTime ) {
				NextShiftTime = IdleTime + GD.Randf() * 3.0f + 1.0f;

				float shiftAmount = GD.Randf() * 1.5f - 0.75f;
				const float duration = 0.8f;

				Owner.CreateTween().TweenProperty( Owner.LegAnimation, "position", new Vector2( shiftAmount, Owner.LegAnimation.Position.Y ), duration ).SetEase( Tween.EaseType.Out );
				await ToSignal( Owner.GetTree().CreateTimer( duration ), Timer.SignalName.Timeout );
				Owner.CreateTween().TweenProperty( Owner.LegAnimation, "position", Vector2.Zero, duration * 0.7f ).SetEase( Tween.EaseType.InOut );
			}
		}
	};
};