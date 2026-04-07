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
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Events.Globals;
using Nomad.Game.Application.Gameplay.Player;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Logger.Globals;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Prefabs {
	/*
	===================================================================================
	
	WorldBase
	
	===================================================================================
	*/
	/// <summary>
	/// The base "world" object.
	/// </summary>
	
	public partial class WorldBase : EngineObject2D {
		private readonly PlayerSpawnService _spawnService;

		public WorldBase() {
			var eventFactory = GameEventRegistry.Instance;
			var serviceRegistry = ServiceRegistry.Instance;
			var logger = Logging.Instance;

			var gameStateService = ServiceLocator.GetService<IGameStateService>();
			var spawnApplicator = new PlayerSpawnApplicator();
			var profileResolver = new PlayerSpawnProfileResolver();
			var audioDevice = ServiceLocator.GetService<IAudioDevice>();

			audioDevice.LoadBank( "Assets/Audio/Banks/Desktop/sfx.bank" );

			_spawnService = new PlayerSpawnService( eventFactory, new PlayerRepository( eventFactory, serviceRegistry, logger, gameStateService, ServiceLocator.GetService<ISceneManager>(), "Assets/Prefabs/Player/Player.tscn" ), spawnApplicator, profileResolver );
		}
	};
};
