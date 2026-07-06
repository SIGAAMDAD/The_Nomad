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
using Godot;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Events.Items;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Gameplay;

namespace Nomad.Game.Gameplay.Items
{
	internal sealed class ItemSpawner : IItemSpawnService
	{
		private const string PICKUP_PREFAB_PATH = "Assets/Prefabs/ItemPickup/PickupRoot.tscn";

		private readonly ISceneManager _sceneManager;
		private readonly IItemCatalog _catalog;
		private readonly IDisposable _itemSpawnRequested;
		private readonly INomadBehaviorRegistry _behaviorRegistry;

		private bool _isDisposed = false;

		public IGameEvent<ItemSpawnRequestEventArgs> ItemSpawnRequest => _itemSpawnRequest;
		private readonly IGameEvent<ItemSpawnRequestEventArgs> _itemSpawnRequest = null;

		public ItemSpawner( ISceneManager sceneManager, IItemCatalog catalog, INomadBehaviorRegistry behaviorRegistry, IGameEventRegistryService eventFactory )
		{
			_sceneManager = sceneManager ?? throw new ArgumentNullException( nameof( sceneManager ) );
			_catalog = catalog ?? throw new ArgumentNullException( nameof( catalog ) );
			_behaviorRegistry = behaviorRegistry ?? throw new ArgumentNullException( nameof( behaviorRegistry ) );

			ArgumentNullException.ThrowIfNull( eventFactory );

			_itemSpawnRequest = eventFactory.GetEvent<ItemSpawnRequestEventArgs>(
				ItemSpawnRequestEventArgs.Name,
				ItemSpawnRequestEventArgs.NameSpace
			);
			_itemSpawnRequested = _itemSpawnRequest.Subscribe( OnItemSpawnRequested );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_itemSpawnRequested.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnItemSpawnRequested( in ItemSpawnRequestEventArgs args )
		{
			if ( args.Amount <= 0 || !args.ItemId.IsValid ) {
				return;
			}

			if ( !_catalog.TryGet<ItemDefinition>( args.ItemId, out ItemDefinition? _ ) ) {
				return;
			}

			var packedScene = ResourceLoader.Load<PackedScene>( $"res://{PICKUP_PREFAB_PATH}" );
			var pickup = packedScene.Instantiate<PickupRoot>();
			pickup.Configure( args.ItemId, args.Amount );

			_sceneManager.ActiveScene?.Root.AddChild( new GodotGameObject( pickup ) );
		}
	};
};
