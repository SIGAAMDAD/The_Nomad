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
using System.Collections.Generic;
using PlayerSystem.Perks;
using PlayerSystem.Runes;
using PlayerSystem.Totems;
using System.Runtime.CompilerServices;
using System;
using Interactables;
using Menus;
using Items;

namespace PlayerSystem.Inventory {

	/*
	===================================================================================
	
	InventoryManager
	
	===================================================================================
	*/
	/// <summary>
	/// Manages the player's local inventory
	/// </summary>

	public partial class InventoryManager : GodotObject {
		/// <summary>
		/// The maximum amount of weight a player can carry on them before becoming <see cref="Player.PlayerFlags.Encumbured"/>,
		/// thus slowing them down
		/// </summary>
		public static readonly float MAXIMUM_INVENTORY_WEIGHT = 500.0f;

		/// <summary>
		/// The maximum amount of weight the player is currently allowed to carry
		/// </summary>
		public static readonly int MAXIMUM_RUNE_SLOTS = 5;

		/// <summary>
		/// The maximum amount of item types that can be allocated to quick access slots
		/// </summary>
		public static readonly int MAXIMUM_QUICK_ACCESS_SLOTS = 4;

		/// <summary>
		/// The maximum amount of equippable runes a player can have at a time
		/// </summary>
		public static readonly int MAX_RUNES = 5;

		/// <summary>
		/// The maximum amount of equippable perks a player can have at a time
		/// </summary>
		public static readonly int MAX_PERKS = 1;

		/// <summary>
		/// The boons that the player has already found
		/// </summary>
		public HashSet<Perk> UnlockedBoons { get; private set; }

		/// <summary>
		/// The runes that the player has already found
		/// </summary>
		public HashSet<Rune> UnlockedRunes { get; private set; }

		/// <summary>
		/// The currently equipped <see cref="PlayerSystem.Totems.Totem"/>
		/// </summary>
		public Totem Totem { get; private set; }

		/// <summary>
		/// The currently equipped <see cref="PlayerSystem.Perks.Perk"/>
		/// </summary>
		public Perk PerkSlot { get; private set; }

		/// <summary>
		/// The currently equipped Runes
		/// </summary>
		public Rune[]? RuneSlots { get; private set; } = null;

		/// <summary>
		/// The currently equipped items assigned to the in-game hotbar
		/// </summary>
		public ConsumableStack[]? QuickAccessSlots { get; private set; } = null;

		/// <summary>
		/// The permanent storage container for the inventory, where everything not being carried is stored
		/// </summary>
		public Dictionary<GodotObject, int>? Storage { get; private set; } = null;

		/// <summary>
		/// The ammo currently being carried by the player
		/// </summary>
		public Godot.Collections.Dictionary<int, AmmoStack>? AmmoStacks { get; private set; }

		/// <summary>
		/// The weapons currently being carried by the player
		/// </summary>
		public Godot.Collections.Dictionary<int, WeaponEntity>? WeaponsStack { get; private set; }

		/// <summary>
		/// The consumables currently being carried by the player
		/// </summary>
		public Godot.Collections.Dictionary<int, ConsumableStack>? ConsumableStacks { get; private set; }

		/// <summary>
		/// The amount of money the player has
		/// </summary>
		public float Money { get; private set; } = 0.0f;

		/// <summary>
		/// The current total weight of inventory items
		/// </summary>
		public float TotalInventoryWeight { get; private set; } = 0.0f;

		/// <summary>
		/// The owning Player object
		/// </summary>
		private Player? Owner;

		[Signal]
		public delegate void WeaponStatusUpdatedEventHandler( WeaponEntity entity, WeaponEntity.Properties properties );

		/*
		===============
		InventoryManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		public InventoryManager( Player? player ) {
			ArgumentNullException.ThrowIfNull( player );

			Owner = player;

			AmmoStacks = new Godot.Collections.Dictionary<int, AmmoStack>();
			WeaponsStack = new Godot.Collections.Dictionary<int, WeaponEntity>();
			ConsumableStacks = new Godot.Collections.Dictionary<int, ConsumableStack>();

			RuneSlots = new Rune[ MAX_RUNES ];
			QuickAccessSlots = new ConsumableStack[ MAXIMUM_QUICK_ACCESS_SLOTS ];
			Storage = new Dictionary<GodotObject, int>();

			UnlockedBoons = new HashSet<Perk>();
			UnlockedRunes = new HashSet<Rune>();
		}

		/*
		===============
		CheckEncumbered
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void CheckEncumbered() {
			if ( TotalInventoryWeight >= MAXIMUM_INVENTORY_WEIGHT * 0.80f ) {
				Owner.SetFlags( Owner.Flags | Player.PlayerFlags.Encumbured );
			} else {
				Owner.SetFlags( Owner.Flags & ~Player.PlayerFlags.Encumbured );
			}
		}

		/*
		===============
		IncreaseInventoryWeight
		===============
		*/
		private void IncreaseInventoryWeight( float amount ) {
			TotalInventoryWeight += amount;
			CheckEncumbered();
		}

		/*
		===============
		DecreaseInventoryWeight
		===============
		*/
		private void DecreaseInventoryWeight( float amount ) {
			TotalInventoryWeight -= amount;
			CheckEncumbered();
		}

		/*
		===============
		DecreaseMoney
		===============
		*/
		/// <summary>
		/// Decreases the player's money by the amount provided
		/// </summary>
		/// <remarks>
		/// Triggers the <see cref="Player.LoseMoney"/> signal.
		/// This function will ignore the call if amount is less than or equal to zero
		/// </remarks>
		/// <param name="amount">The amount of money to remove</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual void DecreaseMoney( float amount ) {
			if ( amount <= 0.0f ) {
				Console.PrintWarning( $"InventoryManager.DecreaseMoney: invalid call, amount <= 0.0f ({amount})" );
				return;
			}
			Money -= amount;
			Owner.EmitSignal( Player.SignalName.LoseMoney, this, amount );
		}

		/*
		===============
		IncreaseMoney
		===============
		*/
		/// <summary>
		/// Increases the player's money by the amount provided
		/// </summary>
		/// <remarks>
		/// Triggers the <see cref="Player.GainMoney"/> signal.
		/// This function will ignore the call if amount is less than or equal to zero
		/// </remarks>
		/// <param name="amount">The amount of money to add</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual void IncreaseMoney( float amount ) {
			if ( amount <= 0.0f ) {
				Console.PrintWarning( $"InventoryManager.IncreaseMoney: invalid call, amount <= 0.0f ({amount})" );
				return;
			}
			Money += amount;
			Owner.EmitSignal( Player.SignalName.GainMoney, this, amount );
		}

		/*
		===============
		Save
		===============
		*/
		/// <summary>
		/// Writes the player's current inventory to disk
		/// </summary>
		/// <remarks>
		/// Any exceptions thrown here would be caught in ArchiveSystem.SaveGame and the save data would be cleaned up
		/// </remarks>
		/// <param name="writer">The section writer from the owning Player object</param>
		public void Save( SaveSystem.SaveSectionWriter writer ) {
			ArgumentNullException.ThrowIfNull( writer );
			ArgumentNullException.ThrowIfNull( AmmoStacks );
			ArgumentNullException.ThrowIfNull( WeaponsStack );
			ArgumentNullException.ThrowIfNull( ConsumableStacks );

			int stackIndex;

			writer.SaveInt( "AmmoStacksCount", AmmoStacks.Count );
			stackIndex = 0;
			foreach ( var stack in AmmoStacks ) {
				stackIndex++;
			}

			writer.SaveInt( "WeaponStacksCount", WeaponsStack.Count );
			stackIndex = 0;
			foreach ( var stack in WeaponsStack ) {
				writer.SaveString( $"WeaponStackNode{stackIndex}", stack.Value.GetPath() );
				stackIndex++;
			}

			writer.SaveInt( "ConsumableStacksCount", ConsumableStacks.Count );
			stackIndex = 0;
			foreach ( var stack in ConsumableStacks ) {
				writer.SaveInt( $"ConsumableStacksAmount{stackIndex}", stack.Value.Amount );
				writer.SaveString( $"ConsumableStacksType{stackIndex}", stack.Value.ItemType.Get( "id" ).AsString() );
				stackIndex++;
			}
		}

		/*
		===============
		Load
		===============
		*/
		/// <summary>
		/// Loads inventory state from disk
		/// </summary>
		/// <param name="reader">The section reader from the owning Player object</param>
		/// <param name="player">The owning player object</param>
		public void Load( SaveSystem.SaveSectionReader reader, Player player ) {
			ArgumentNullException.ThrowIfNull( reader );
			ArgumentNullException.ThrowIfNull( player );
			ArgumentNullException.ThrowIfNull( AmmoStacks );
			ArgumentNullException.ThrowIfNull( WeaponsStack );
			ArgumentNullException.ThrowIfNull( ConsumableStacks );

			AmmoStacks.Clear();
			int numAmmoStacks = reader.LoadInt( "AmmoStacksCount" );
			for ( int i = 0; i < numAmmoStacks; i++ ) {
			}

			WeaponsStack.Clear();
			int numWeapons = reader.LoadInt( "WeaponStacksCount" );
			for ( int i = 0; i < numWeapons; i++ ) {
				WeaponEntity weapon = new WeaponEntity();
				weapon.Load( player, reader.LoadString( string.Format( "WeaponStackNode{0}", i ) ) );
				player.AddChild( weapon );
				WeaponsStack.Add( weapon.GetPath().GetHashCode(), weapon );
			}

			ConsumableStacks.Clear();
			int numConsumableStacks = reader.LoadInt( "ConsumableStacksCount" );
			for ( int i = 0; i < numConsumableStacks; i++ ) {
				ConsumableStack stack = new ConsumableStack(
					(Resource)( (Resource)player.InventoryDatabase.Get( "database" ) ).Call(
						"get_item", reader.LoadString( $"ConsumableStacksType{i}" )
					),
					reader.LoadInt( $"ConsumableStacksAmount{i}" )
				);
				ConsumableStacks.Add( stack.GetItemID().GetHashCode(), stack );
			}
		}

		/*
		===============
		LoadWeapon
		===============
		*/
		/// <summary>
		/// Loads the provided weapon object into a weapon slot and initializes ammo reserves if needed
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="ammo"></param>
		/// <param name="slot"></param>
		/// <exception cref="ArgumentException"></exception>
		public void LoadWeapon( WeaponEntity weapon, string ammo, int slot ) {
			ArgumentNullException.ThrowIfNull( AmmoStacks );

			if ( slot != (int)WeaponSlotIndex.Invalid ) {
				weapon.SetEquippedState( true );
				Owner.WeaponSlots[ (WeaponSlotIndex)slot ].SetWeapon( weapon );
			}

			if ( ammo != null && ammo.Length > 0 ) {
				AmmoStack? stack = null;
				foreach ( var it in AmmoStacks ) {
					if ( it.Key == ammo.GetHashCode() ) {
						stack = it.Value;
						break;
					}
				}
				if ( stack != null ) {
					weapon.CallDeferred( WeaponFirearm.MethodName.SetAmmoStack, stack );
				} else {
					throw new ArgumentOutOfRangeException( $"InventoryManager.LoadWeapon: ammo type {ammo} wasn't found!" );
				}
			}
		}

		/*
		===============
		PickupAmmo
		===============
		*/
		/// <summary>
		/// Adds an ItemPickup's ammo data to the <see cref="AmmoStacks"/>
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name="ammo">The ItemPickup that's being interacted with</param>
		public void PickupAmmo( in ItemPickup ammo ) {
			ArgumentNullException.ThrowIfNull( ammo );
			ArgumentNullException.ThrowIfNull( AmmoStacks );

			AmmoStack? stack = null;
			bool found = false;

			string id = ammo.Data.Get( "id" ).AsString();
			Owner.EmitSignal( Player.SignalName.AmmoPickedUp, id );

			//
			// first check if we already have a stack for the ammo type
			// if not, add it
			//
			foreach ( var it in AmmoStacks ) {
				if ( id == it.Value.AmmoType.ItemId ) {
					found = true;
					stack = it.Value;
					break;
				}
			}
			if ( !found ) {
				stack = new AmmoStack( ammo.Data, ammo.Amount );

				int hashCode = id.GetHashCode();
				stack.SetMeta( "hash", hashCode );
				AmmoStacks.Add( hashCode, stack );
			}

			// make sure the stack is valid
			ArgumentNullException.ThrowIfNull( stack );

			stack.AddItems( ammo.Amount == -1 ? ammo.Data.Get( "properties" ).AsGodotDictionary()[ "stack_add_amount" ].AsInt32() : ammo.Amount );

			Owner.PlaySound( Owner.MiscChannel, stack.AmmoType.PickupSfx );

			for ( WeaponSlotIndex i = 0; i < WeaponSlotIndex.Count; i++ ) {
				WeaponSlot slot = Owner.WeaponSlots[ i ];

				// sanity check
				ArgumentNullException.ThrowIfNull( slot.Weapon );

				if ( slot.Weapon is WeaponFirearm firearm && firearm != null ) {
					if ( firearm.Ammo.AmmoType.Type == stack.AmmoType.Type ) {
						firearm.SetAmmoStack( stack );
						if ( Owner.LastUsedArm.Slot == i ) {
							Owner.EmitSignal( Player.SignalName.WeaponStatusUpdated, slot.Weapon, (uint)slot.Mode );
						}
					}
				}
			}
		}

		/*
		===============
		PickupWeapon
		===============
		*/
		/// <summary>
		/// Picks up a WeaponEntity, stores it in the <see cref="WeaponsStack"/>, and if
		/// there's no weapon in the slots, it'll be automatically equipped
		/// </summary>
		/// <remarks>
		/// Emits <see cref="Player.WeaponPickedUp"/> signal
		/// </remarks>
		/// <param name="weapon">The weapon to be picked up</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="weapon"/> is null</exception>
		public void PickupWeapon( in WeaponEntity weapon ) {
			// sanity
			ArgumentNullException.ThrowIfNull( WeaponsStack );
			ArgumentNullException.ThrowIfNull( AmmoStacks );

			Godot.Collections.Array<Resource> categories = weapon.Data.Get( "categories" ).AsGodotArray<Resource>()
				?? throw new InvalidOperationException( $"weapon {weapon.GetInstanceId()} ItemDefinition doesn't contain a categories definition" );

			// find the appropriate slot
			WeaponSlotIndex index = WeaponSlotIndex.Invalid;
			for ( int i = 0; i < categories.Count; i++ ) {
				index = categories[ i ].Get( "id" ).AsString() switch {
					"WEAPON_CATEGORY_PRIMARY" => WeaponSlotIndex.Primary,
					"WEAPON_CATEGORY_HEAVY_PRIMARY" => WeaponSlotIndex.HeavyPrimary,
					"WEAPON_CATEGORY_SIDEARM" => WeaponSlotIndex.Sidearm,
					"WEAPON_CATEGORY_HEAVY_SIDEARM" => WeaponSlotIndex.HeavySidearm,
					_ => WeaponSlotIndex.Count
				};
			}

			if ( index == WeaponSlotIndex.Invalid ) {
				throw new ArgumentOutOfRangeException( $"WeaponEntity {weapon.GetInstanceId()} doesn't have a suitable category" );
			} else if ( Owner.WeaponSlots[ index ].IsUsed() ) {
				Owner.WeaponSlots[ index ].SetWeapon( weapon );
				Owner.WeaponSlots.SetEquippedWeapon( index );
			}

			int hashCode = (int)weapon.GetInstanceId();
			weapon.SetMeta( "hash", hashCode );
			WeaponsStack.Add( hashCode, weapon );
			TotalInventoryWeight += weapon.Weight;

			// it gets a little buggy here...
			Owner.TorsoAnimation.FlipH = false;
			Owner.LegAnimation.FlipH = false;

			weapon.Connect( WeaponEntity.SignalName.ModeChanged, Callable.From<WeaponEntity, WeaponEntity.Properties>( ( source, useMode ) => Owner.EmitSignal( Player.SignalName.WeaponStatusUpdated, source, (uint)source.DefaultMode ) ) );

			if ( weapon is WeaponFirearm firearm && firearm != null ) {
				AmmoStack? stack = null;
				foreach ( var ammo in AmmoStacks ) {
					if ( ammo.Value.AmmoType.Type == firearm.Ammo.AmmoType.Type ) {
						stack = ammo.Value;
						break;
					}
				}
				if ( stack != null ) {
					firearm.SetAmmoStack( stack );
				}
			}

			if ( SettingsData.EquipWeaponOnPickup ) {
				// apply rules of various weapon properties
				if ( ( weapon.DefaultMode & WeaponEntity.Properties.IsTwoHanded ) != 0 ) {
					Owner.SetHandsUsed( Player.Hands.Both );

					Owner.ArmLeft.SetSlot( WeaponSlotIndex.Invalid );

					Owner.SetLastUsedArm( Owner.ArmRight );
					Owner.LastUsedArm.SetSlot( index );

					// this will automatically overwrite any other modes
					Owner.WeaponSlots[ Owner.LastUsedArm.Slot ].SetMode( weapon.DefaultMode );
				} else if ( ( weapon.DefaultMode & WeaponEntity.Properties.IsOneHanded ) != 0 ) {
					if ( Owner.LastUsedArm == null ) {
						Owner.SetLastUsedArm( Owner.ArmRight );
					}

					// sanity
					ArgumentNullException.ThrowIfNull( Owner.LastUsedArm );

					Owner.LastUsedArm.SetSlot( Owner.WeaponSlots.CurrentWeapon );
					if ( Owner.LastUsedArm == Owner.ArmRight ) {
						Owner.SetHandsUsed( Player.Hands.Right );
					} else if ( Owner.LastUsedArm == Owner.ArmLeft ) {
						Owner.SetHandsUsed( Player.Hands.Left );
					}
					Owner.WeaponSlots[ Owner.LastUsedArm.Slot ].SetMode( weapon.DefaultMode );
				}

				// update the hand data
				Owner.LastUsedArm.SetSlot( Owner.WeaponSlots.CurrentWeapon );
				Owner.WeaponSlots[ Owner.LastUsedArm.Slot ].SetMode( weapon.PropertyBits );

				Owner.EmitSignal( Player.SignalName.SwitchedWeapon, weapon );
				Owner.EmitSignal( Player.SignalName.HandsStatusUpdated, (uint)Owner.HandsUsed );
			}

			Owner.EmitSignal( Player.SignalName.WeaponPickedUp );
		}

		/*
		===============
		DropWeapon
		===============
		*/
		/// <summary>
		/// Removes a WeaponEntity object from the inventory, and unequips the weapon if it was previously equipped
		/// </summary>
		/// <remarks>
		/// Calls <see cref="EmitSignalWeaponStatusUpdated"/> with null and <see cref="WeaponEntity.Properties.None"/> if the weapon was equipped
		/// </remarks>
		/// <param name="hashCode">The unique identifier of the WeaponEntity</param>
		/// <exception cref="System.Exception">Thrown if the WeaponEntity found from hashCode is null, indicating corruption</exception>
		public void DropWeapon( int hashCode ) {
			ArgumentNullException.ThrowIfNull( WeaponsStack );

			if ( !WeaponsStack.TryGetValue( hashCode, out WeaponEntity? weapon ) ) {
				Console.PrintError( $"Player.DropWeapon: invalid hash id {hashCode}" );
				return;
			}
			if ( weapon == null ) {
				throw new System.Exception( $"InventoryManager.DropWeapon: weapon from valid hashCode {hashCode} in WeaponsStack is null" );
			}

			WeaponsStack.Remove( hashCode );
			weapon.Drop();

			for ( WeaponSlotIndex i = 0; i < WeaponSlotIndex.Count; i++ ) {
				if ( Owner.WeaponSlots[ i ].Weapon == weapon ) {
					if ( i == Owner.WeaponSlots.CurrentWeapon ) {
						Owner.WeaponSlots.SetEquippedWeapon( WeaponSlotIndex.Invalid );
						EmitSignalWeaponStatusUpdated( null, WeaponEntity.Properties.None );
					}
					Owner.WeaponSlots[ i ].SetWeapon( null );
				}
			}
		}

		/*
		===============
		DropAmmo
		===============
		*/
		/// <summary>
		/// Removes an AmmoStack from the inventory
		/// </summary>
		/// <param name="hashCode">The unique identifier of the AmmoStack</param>
		public void DropAmmo( int hashCode ) {
			ArgumentNullException.ThrowIfNull( AmmoStacks );
			if ( !AmmoStacks.ContainsKey( hashCode ) ) {
				Console.PrintError( $"InventoryManager.DropAmmo: invalid hash id {hashCode}" );
				return;
			}
			AmmoStacks.Remove( hashCode );
		}
	};
};