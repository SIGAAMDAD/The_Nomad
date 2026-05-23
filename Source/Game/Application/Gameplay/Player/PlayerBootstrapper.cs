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

using Nomad.Core.CVars;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Game.Application.Gameplay.Player.Animation;
using Nomad.Game.Application.Gameplay.Player.Combat;
using Nomad.Game.Application.Gameplay.Player.Input;
using Nomad.Game.Application.Gameplay.Player.Inventory;
using Nomad.Game.Application.Gameplay.Player.JumpKit;
using Nomad.Game.Application.Gameplay.Player.State;
using Nomad.Game.Application.Gameplay.Player.Stats;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Player.State;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerBootstrapper

	===================================================================================
	*/
	/// <summary>
	/// Builds the player runtime graph so PlayerBase can focus on player identity and
	/// public player behavior.
	/// </summary>

	internal static class PlayerBootstrapper
	{
		/*
		===============
		Bootstrap
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="prefab"></param>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		/// <returns></returns>
		public static PlayerRuntime Bootstrap( PlayerId playerId, PlayerPrefab prefab, IGameEventRegistryService eventFactory, ILoggerService logger )
		{
			var cvarSystem = ServiceLocator.GetService<ICVarSystemService>();
			var timeService = ServiceLocator.GetService<ITimeService>();

			var aimCoordinator = new PlayerAimCoordinator( eventFactory );
			var statsRepository = new PlayerBaseStatsRepository( playerId, eventFactory, logger );
			var dependencyGraph = PlayerStatDependencyGraph.CreateDefault();
			var derivedStatService = new PlayerDerivedStatService( playerId, statsRepository, dependencyGraph, eventFactory );
			var flagService = new PlayerFlagService( playerId, eventFactory );
			var resourceService = new PlayerResourceService( playerId, derivedStatService, eventFactory );
			var stateCoordinator = new PlayerStateCoordinator( playerId, PlayerStateId.Idle, eventFactory );
			var saveCoordinator = new PlayerSaveCoordinator( derivedStatService, resourceService, stateCoordinator, prefab, eventFactory );
			var inventoryCoordinator = new PlayerInventoryCoordinator( statsRepository, stateCoordinator, eventFactory, ServiceLocator.GetService<IWorldContentCache>().Items );
			var weaponCoordinator = new PlayerWeaponCoordinator( playerId, inventoryCoordinator, eventFactory );

			ApplyBaseStats( prefab, statsRepository );
			derivedStatService.FlushDirty();

			var movementController = prefab.AddComponent<PlayerMovementController>( comp => {
				comp.Stats = derivedStatService;
				comp.Flags = flagService;
				comp.Id = playerId;
				comp.StateReader = stateCoordinator;
				comp.StateWriter = stateCoordinator;
				comp.AimWriter = aimCoordinator;
				comp.InputSource = new LocalPlayerInputSource( playerId, cvarSystem, eventFactory );
			} );

			var jumpKit = prefab.AddComponent<PlayerJumpKit>( comp => {
				comp.Id = playerId;
			} );

			var audioService = prefab.AddComponent<PlayerAudioService>( comp => {
				comp.Id = playerId;
				comp.FlagService = flagService;
			} );

			var animator = new PlayerAnimationController(
				playerId,
				prefab,
				movementController,
				aimCoordinator,
				stateCoordinator,
				eventFactory
			);

			var bulletTime = prefab.AddComponent<PlayerBulletTime>( comp => {
				comp.FlagService = flagService;
				comp.DerivedStatService = derivedStatService;
				comp.ResourceService = resourceService;
				comp.TimeService = timeService;
			} );

			return new PlayerRuntime(
				aimCoordinator,
				jumpKit,
				animator,
				movementController,
				audioService,
				bulletTime,
				stateCoordinator,
				statsRepository,
				derivedStatService,
				resourceService,
				dependencyGraph,
				flagService,
				saveCoordinator,
				weaponCoordinator,
				inventoryCoordinator
			);
		}

		private static void ApplyBaseStats( PlayerPrefab prefab, PlayerBaseStatsRepository statsRepository )
		{
			foreach ( var pair in prefab.Definition.Stats.BaseStats ) {
				statsRepository.SetBaseStatValue( pair.Key, pair.Value );
			}
		}
	};
};
