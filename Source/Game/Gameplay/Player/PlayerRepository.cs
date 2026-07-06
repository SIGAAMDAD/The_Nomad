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
using System.Collections.Concurrent;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.OnlineServices;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Gameplay;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Game.Sdk.Player.State;
using Nomad.Game.Prefabs;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay;

namespace Nomad.Game.Gameplay.Player
{
	/*
	===================================================================================

	PlayerRepository

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class PlayerRepository : IPlayerRuntimeRegistry, IDisposable
	{
		private readonly ConcurrentDictionary<PlayerId, PlayerBase> _players = new();
		private readonly string _playerPrefab;

		private readonly ISceneManager _sceneManager;
		private readonly IGameStateService _gameStateService;
		private readonly IGameEventRegistryService _eventFactory;
		private readonly ILoggerService _logger;
		private readonly IWorldContentCache _worldContent;
		private readonly ILocalizationService _localizationService;
		private readonly ICVarSystemService _cvarSystem;

		private readonly IDisposable _gameStateChanged;

		private bool _isDisposed = false;

		/*
		===============
		PlayerRepository
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		/// <param name="gameStateService"></param>
		/// <param name="sceneManager"></param>
		/// <param name="playerPrefab"></param>
		public PlayerRepository(
			IGameEventRegistryService eventFactory,
			ILoggerService logger,
			IGameStateService gameStateService,
			ISceneManager sceneManager,
			IWorldContentCache worldContent,
			ILocalizationService localizationService,
			ICVarSystemService cvarSystem,
			string playerPrefab
		)
		{
			ArgumentGuard.ThrowIfNullOrWhiteSpace( playerPrefab, nameof( playerPrefab ) );

			_gameStateService = gameStateService ?? throw new ArgumentNullException( nameof( gameStateService ) );
			_playerPrefab = playerPrefab;
			_sceneManager = sceneManager ?? throw new ArgumentNullException( nameof( sceneManager ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
			_worldContent = worldContent ?? throw new ArgumentNullException( nameof( worldContent ) );
			_localizationService = localizationService ?? throw new ArgumentNullException( nameof( localizationService ) );
			_cvarSystem = cvarSystem ?? throw new ArgumentNullException( nameof( cvarSystem ) );

			_gameStateChanged = _gameStateService.StateChanged.Subscribe( OnGameStateChanged );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_gameStateChanged?.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		CreatePlayer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="origin"></param>
		/// <returns></returns>
		public PlayerBase CreatePlayer( in PlayerSpawnRequestedEventArgs args )
		{
			var composite = _sceneManager.LoadPrefab( _playerPrefab );
			var prefab = composite.Root.CastAs<PlayerPrefab>();
			prefab.PeerId = ResolvePlayerId( in args );

			var playerBase = new PlayerAggregate( prefab.PeerId, composite.Root.CastAs<PlayerPrefab>(), args.Context.LocalPlayerIndex, _eventFactory, _logger );
			_players[prefab.PeerId] = playerBase;

			BindHud( prefab, playerBase );

			_logger.PrintLine( $"Adding player to active scene '{_sceneManager.ActiveScene.Name}'" );
			_sceneManager.ActiveScene.Root.AddChild( prefab );

			return playerBase;
		}

		private static PlayerId ResolvePlayerId( in PlayerSpawnRequestedEventArgs args )
		{
			if ( args.Context.RequestedPlayerId.IsValid ) {
				return args.Context.RequestedPlayerId;
			}

			return new PlayerId( new PeerId( Constants.LOCAL_GUID ) );
		}

		private void BindHud( PlayerPrefab prefab, PlayerBase player )
		{
			var hudView = prefab.GetNodeOrNull<HeadsUpDisplayView>( "HeadsUpDisplay" );
			if ( hudView == null ) {
				return;
			}

			hudView.Bind(
				new HudRoot(
					player.PlayerId,
					hudView,
					player.Runtime.InventoryCoordinator.WeaponSlots,
					_worldContent.ItemInstances,
					_localizationService,
					_cvarSystem,
					_eventFactory
				)
			);
		}

		public bool TryGetInventory( PlayerId playerId, out IPlayerInventoryCoordinator? inventory )
		{
			playerId.ThrowIfInvalid( nameof( TryGetInventory ) );

			if ( !_players.TryGetValue( playerId, out PlayerBase? player ) ) {
				inventory = null;
				return false;
			}

			inventory = player.Runtime.InventoryCoordinator;
			return true;
		}

		public bool TryGetState( PlayerId playerId, out IPlayerStateReader? reader, out IPlayerStateWriter? writer )
		{
			playerId.ThrowIfInvalid( nameof( TryGetState ) );

			if ( !_players.TryGetValue( playerId, out PlayerBase? player ) ) {
				reader = null;
				writer = null;
				return false;
			}

			reader = player.Runtime.StateCoordinator;
			writer = player.Runtime.StateCoordinator;
			return true;
		}

		public bool TryGetWeaponSlots( PlayerId playerId, out IWeaponSlotService? slots )
		{
			playerId.ThrowIfInvalid( nameof( TryGetState ) );

			if ( !_players.TryGetValue( playerId, out PlayerBase? player ) ) {
				slots = null;
				return false;
			}

			slots = player.Runtime.InventoryCoordinator.WeaponSlots;
			return true;
		}

		/*
		===============
		OnGameStateChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnGameStateChanged( in GameStateChangedEventArgs args )
		{
			if ( args.CurrentState == GameState.Menu ) {
				_players.Clear();
			}
		}
	};
};
