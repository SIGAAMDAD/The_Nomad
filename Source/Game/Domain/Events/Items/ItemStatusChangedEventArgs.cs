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
using Nomad.Game.Domain.Data.Items;

namespace Nomad.Game.Domain.Events.Items {
	/// <summary>
	/// 
	/// </summary>
	public readonly struct ItemStatusChangedEventArgs {
		/// <summary>
		/// The item the status belongs to.
		/// </summary>
		public Guid ItemId { get; }

		/// <summary>
		/// The entity that is changing the item's status.
		/// </summary>
		public Guid EntityId { get; }

		/// <summary>
		/// 
		/// </summary>
		public ItemStatus OldStatus { get; }

		/// <summary>
		/// 
		/// </summary>
		public ItemStatus NewStatus { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="entityId"></param>
		/// <param name="oldStatus"></param>
		/// <param name="newStatus"></param>
		public ItemStatusChangedEventArgs( Guid itemId, Guid entityId, ItemStatus oldStatus, ItemStatus newStatus ) {
			ItemId = itemId;
			EntityId = entityId;
			OldStatus = oldStatus;
			NewStatus = newStatus;
		}
	};
};