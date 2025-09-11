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
using Interactables;

namespace Items {
	/*
	===================================================================================
	
	AmmoStack
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class AmmoStack : Node, IConsumableStack {
		public Ammo AmmoType { get; private set; }

		public int Amount {
			get => _bulletsLeft;
			set => _bulletsLeft = value;
		}

		/// <summary>
		/// The amount of bullets remaining in the stack
		/// </summary>
		private int _bulletsLeft = 0;

		public AmmoStack( Resource? ammoType, int amount ) {
			ArgumentNullException.ThrowIfNull( ammoType );

			AmmoType = new Ammo( ammoType );
			Amount = amount;
		}
		public AmmoStack( string itemPath ) {
			CallDeferred( MethodName.InitFromNode, itemPath );
		}

		/*
		===============
		InitFromNode
		===============
		*/
		private void InitFromNode( string path ) {
			ItemPickup? pickup;

			try {
				pickup = GetNode<ItemPickup>( path );
				if ( pickup == null ) {
					Console.PrintError( $"AmmoStack.InitFromNode: node {path} isn't a valid node" );
					QueueFree();
					return;
				}
			} catch ( InvalidCastException ) {
				Console.PrintError( $"AmmoStack.InitFromNode: node {path} isn't an ItemPickup node (InvalidCastException)" );
				QueueFree();
				return;
			}

			AmmoType = new Ammo( pickup.Data );
			Amount = pickup.Amount;
		}

		/*
		===============
		AddItems
		===============
		*/
		/// <summary>
		/// Adds <paramref name="items"/> number of items to the AmmoStack
		/// </summary>
		/// <param name="items">The number of items to add to the AmmoStack</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="items"/> is less than 0</exception>
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
		/// REmoeves <paramref name="items"/> number of items from the AmmoStack
		/// </summary>
		/// <param name="items">The number of items to remove</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="items"/> is less than 0</exception>
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