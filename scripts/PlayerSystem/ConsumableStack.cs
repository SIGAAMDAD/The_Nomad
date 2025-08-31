/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

using Godot;
using System;
using System.Runtime.CompilerServices;

namespace PlayerSystem {
	/*
	===================================================================================
	
	ConsumableStack
	
	===================================================================================
	*/
	/// <summary>
	/// Item stack class but for consumables like stims, healthpacks, etc.
	/// </summary>
	
	public partial class ConsumableStack : GodotObject {
		public Resource? ItemType { get; private set; } = null;
		public int Amount { get; private set; } = 0;

		/*
		===============
		ConsumableStack
		===============
		*/
		/// <summary>
		/// Creates a new consumable stack with the given itemType
		/// </summary>
		/// <param name="itemType">The ItemDefinition of the item that will be used in the stack</param>
		/// <param name="amount">The amount of the item in the stack</param>
		public ConsumableStack( Resource itemType, int amount ) {
			ItemType = itemType;
			Amount = amount;
		}

		/*
		===============
		GetItemID
		===============
		*/
		/// <summary>
		/// Wrapper function around the rather ugly ItemType.Get( "id" ).AsString()
		/// </summary>
		/// <returns>The ItemType item_id assigned in to the ItemDefinition</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string GetItemID() {
			return ItemType.Get( "id" ).AsString();
		}

		/*
		===============
		SetType
		===============
		*/
		/// <summary>
		/// Sets the type of consumable resource to be used in the stack
		/// </summary>
		/// <param name="consumable">The ConsumableEntity to use</param>
		public void SetType( ConsumableEntity consumable ) {
			// Should I even allow this outside the constructor...? FIXME?
			
			ArgumentNullException.ThrowIfNull( consumable, nameof( consumable ) );

			ItemType = consumable.Data;
			Amount = 0;
		}

		/*
		===============
		AddItems
		===============
		*/
		/// <summary>
		/// Adds a set count of items to the stack
		/// </summary>
		/// <param name="items">The amount of items to add to the stack</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void AddItems( int items ) {
			ArgumentOutOfRangeException.ThrowIfNegative( items );

			Amount += items;
		}

		/*
		===============
		RemoveItems
		===============
		*/
		/// <summary>
		/// Removes a set count of items from the stack
		/// </summary>
		/// <param name="items">The amount of items to retrieve from the stack</param>
		/// <returns>The amount of remaining items in the stack if Amount - items is less than 0, or the requested amount</returns>
		public int RemoveItems( int items ) {
			ArgumentOutOfRangeException.ThrowIfNegative( items );

			if ( Amount - items < 0 ) {
				int tmp = Amount;
				Amount = 0;
				return tmp;
			}

			Amount -= items;
			return items;
		}
	};
};