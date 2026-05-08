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

using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Game.Domain.Events.Items;
using Nomad.Game.Domain.Interfaces.Items;

namespace Nomad.Game.Application.Gameplay.Items
{
	/*
	===================================================================================

	ItemSpawnService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class ItemSpawnService : IItemSpawnService
	{
		private readonly ItemRepository _repository;

		public IGameEvent<ItemSpawnRequestEventArgs> ItemSpawnRequest => _itemSpawnRequest;
		private readonly IGameEvent<ItemSpawnRequestEventArgs> _itemSpawnRequest = default;

		public ItemSpawnService( IGameEventRegistryService eventFactory, IFileSystem fileSystem )
		{
			_repository = new ItemRepository( fileSystem );

			_itemSpawnRequest = eventFactory
				.GetEvent<ItemSpawnRequestEventArgs>(
					ItemSpawnRequestEventArgs.Name,
					ItemSpawnRequestEventArgs.NameSpace
				);
			_itemSpawnRequest.Subscribe( OnItemSpawnRequested );
		}

		private void OnItemSpawnRequested( in ItemSpawnRequestEventArgs args )
		{

		}
	};
};
