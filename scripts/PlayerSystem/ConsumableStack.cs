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

namespace PlayerSystem {
	public struct ConsumableStack {
		public Resource ItemType = null;
		public int Amount = 0;
		public string ItemId = "";

		public ConsumableStack() {
		}

		public void SetType( ConsumableEntity consumable ) {
			ItemType = consumable.Data;
			ItemId = (string)ItemType.Get( "id" );
			Amount = 0;
		}
		public void AddItems( int nItems ) {
			Amount += nItems;
		}
		public int RemoveItems( int nItems ) {
			if ( Amount - nItems < 0 ) {
				int tmp = Amount;
				Amount = 0;
				return tmp;
			}

			Amount -= nItems;
			return nItems;
		}
	};
};