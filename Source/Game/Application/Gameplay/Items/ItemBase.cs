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
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Events.Items;
using Nomad.Game.Domain.Interfaces.Items;

namespace Nomad.Game.Application.Gameplay.Items
{
	/*
	===================================================================================

	ItemBase

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class ItemBase : IItemBase<ItemDefinition>
	{
		public ItemStatus State => _state;
		private readonly ItemStatus _state;

		public ItemDefinition Definition => _definition;
		private readonly ItemDefinition _definition;

		public Guid Id => _id;
		private readonly Guid _id;

		public IGameEvent<ItemStatusChangedEventArgs> StatusChanged => _statusChanged;
		private readonly IGameEvent<ItemStatusChangedEventArgs> _statusChanged;

		public ItemBase( Guid id, ItemDefinition definition, IGameEventRegistryService eventFactory )
		{
			_statusChanged = eventFactory
				.GetEvent<ItemStatusChangedEventArgs>(
					ItemStatusChangedEventArgs.Name,
					ItemStatusChangedEventArgs.NameSpace
				);
		}
	};
};
