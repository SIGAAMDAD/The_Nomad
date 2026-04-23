/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Domain.Data.Items {
	public sealed class ItemStack {
		public Guid ItemId { get; }
		public int Amount { get; private set; }

		public bool IsEmpty => Amount <= 0;

		/*
		===============
		ItemStack
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		public ItemStack( Guid itemId, int amount ) {
			RangeGuard.ThrowIfNegativeOrZero( amount, nameof( amount ) );
			ItemId = itemId;
			Amount = amount;
		}

		/*
		===============
		Add
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount"></param>
		public void Add( int amount ) {
			RangeGuard.ThrowIfNegativeOrZero( amount, nameof( amount ) );
			checked {
				Amount += amount;
			}
		}

		/*
		===============
		TryRemove
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool TryRemove( int amount ) {
			if ( amount <= 0 ) {
				return false;
			}
			if ( Amount < amount ) {
				return false;
			}
			Amount -= amount;
			return true;
		}
	};
};