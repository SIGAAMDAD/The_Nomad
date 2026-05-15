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

using Nomad.Audio.Interfaces;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Infrastructure.Gameplay.Items;
using Nomad.Game.Presentation.Screens.Gameplay;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Prefabs
{
	/*
	===================================================================================
	
	WorldBase
	
	===================================================================================
	*/
	/// <summary>
	/// The base "world" object.
	/// </summary>

	public partial class WorldBase : EngineSceneObject
	{
		private PlayerSpawnService _spawnService;
		private ISceneManager _sceneManager;
		private GameplayScreen _gameOverlay;

		public WorldBase()
		{
			var serviceLocator = ServiceLocator.Instance;
			var serviceRegistry = ServiceRegistry.Instance;

			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();
			var logger = serviceLocator.GetService<ILoggerService>();
			var fileSystem = serviceLocator.GetService<IFileSystem>();

			var gameStateService = serviceLocator.GetService<IGameStateService>();
			var spawnApplicator = new PlayerSpawnApplicator();
			var profileResolver = new PlayerSpawnProfileResolver();
			var audioDevice = serviceLocator.GetService<IAudioDevice>();
			_sceneManager = serviceLocator.GetService<ISceneManager>();

			audioDevice.LoadBank( "Assets/Audio/Banks/Desktop/sfx.bank" );

			_spawnService = new PlayerSpawnService( eventFactory, new PlayerRepository( eventFactory, serviceRegistry, logger, gameStateService, ServiceLocator.GetService<ISceneManager>(), "Assets/Prefabs/Player/Player.tscn" ), spawnApplicator, profileResolver, logger );

			var itemCatalog = new ItemCatalog( fileSystem );
			itemCatalog.AddLoader( ItemType.Ammunition, AmmoDefinition.Load );
			itemCatalog.AddLoader( ItemType.Firearm, FirearmDefinition.Load );
			serviceRegistry.AddSingleton( new ItemCatalog( fileSystem ) );
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit()
		{
			base.OnInit();

			var gameOverlayScene = _sceneManager.LoadPrefab( "Source/Game/Presentation/Screens/Gameplay/GameplayScreen.tscn" );
			_gameOverlay = gameOverlayScene.Root.CastAs<GameplayScreen>();
			AddChild( _gameOverlay );
		}

		protected override void OnShutdown()
		{
			base.OnShutdown();

			_spawnService?.Dispose();
		}
	};
};
