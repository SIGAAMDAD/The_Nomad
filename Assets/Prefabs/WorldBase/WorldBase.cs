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

using Godot;
using Nomad.Audio.Interfaces;
using Nomad.Core.CVars;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Infrastructure.Streaming;
using Nomad.Game.Presentation.Screens.Gameplay;
using Nomad.Game.Sdk.Gameplay;
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
		[Export]
		private Node3D _regionRoot;

		[Export]
		private RegionManifestTable _regionManifestTable;

		private readonly PlayerSpawnService _spawnService;
		private readonly PlayerRepository _playerRepository;
		private readonly ISceneManager _sceneManager;
		private GameplayScreen _gameOverlay;

		private readonly WorldChunkStreamer _streamer;

		public WorldBase()
		{
			var serviceLocator = ServiceLocator.Instance;
			var serviceRegistry = ServiceRegistry.Instance;

			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();
			var logger = serviceLocator.GetService<ILoggerService>();

			var gameStateService = serviceLocator.GetService<IGameStateService>();
			var cvarSystem = serviceLocator.GetService<ICVarSystemService>();
			var localizationService = serviceLocator.GetService<ILocalizationService>();
			var worldContent = serviceLocator.GetService<IWorldContentCache>();
			var spawnApplicator = new PlayerSpawnApplicator();
			var profileResolver = new PlayerSpawnProfileResolver();
			var audioDevice = serviceLocator.GetService<IAudioDevice>();
			_sceneManager = serviceLocator.GetService<ISceneManager>();

			audioDevice.LoadBank( "Assets/Audio/Banks/Desktop/sfx.bank" );

			_playerRepository = new PlayerRepository(
				eventFactory,
				logger,
				gameStateService,
				_sceneManager,
				worldContent,
				localizationService,
				cvarSystem,
				"Assets/Prefabs/Player/Player.tscn"
			);
			serviceRegistry.AddSingleton<IPlayerRuntimeRegistry>( _playerRepository );

			_spawnService = new PlayerSpawnService( eventFactory, _playerRepository, spawnApplicator, profileResolver, logger );

			var gameOverlayScene = _sceneManager.LoadPrefab( "Source/Game/Presentation/Screens/Gameplay/GameplayScreen.tscn" );
			_gameOverlay = gameOverlayScene.Root.CastAs<GameplayScreen>();
			CallDeferred( MethodName.AddChild, _gameOverlay );

			var streamerSettings = new RegionStreamingSettings {
				RegionSizeMeters = 512,
				MinRegionX = -16,
				MinRegionZ = -16,
				ActiveRadius = 1,
				HotRadius = 2,
				WarmRadius = 3,
				ColdRadius = 5,
				DefaultPromotionBudget = 1,
				DefaultDemotionBudget = 2
			};

			_streamer = new WorldChunkStreamer( _regionRoot, streamerSettings );
			_streamer.LoadManifestTable( _regionManifestTable );
		}

		protected override void OnUpdate( float delta )
		{
			base.OnUpdate( delta );

			_streamer.TickLatest();
		}

		protected override void OnShutdown()
		{
			base.OnShutdown();

			_streamer?.Shutdown();

			_spawnService?.Dispose();
			_playerRepository?.Dispose();
		}
	};
};
