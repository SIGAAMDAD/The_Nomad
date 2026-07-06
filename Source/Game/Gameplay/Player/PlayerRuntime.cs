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
using Nomad.Game.Gameplay.Player.Animation;
using Nomad.Game.Gameplay.Player.Combat;
using Nomad.Game.Gameplay.Player.Inventory;
using Nomad.Game.Gameplay.Player.JumpKit;
using Nomad.Game.Gameplay.Player.State;
using Nomad.Game.Gameplay.Player.Stats;
using Nomad.Game.Gameplay.Player.Movement;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Gameplay.Player
{
	/*
	===================================================================================

	PlayerRuntime

	===================================================================================
	*/
	/// <summary>
	/// Holds the player services and prefab components created during player bootstrap.
	/// </summary>

	internal sealed class PlayerRuntime : IDisposable
	{
		public readonly PlayerPrefab Prefab;

		public PlayerJumpKit JumpKit { get; }
		public PlayerAnimationCoordinator Animator { get; }
		public PlayerMovementController MovementController { get; }
		public PlayerParkourController ParkourController { get; }
		public PlayerAudioService AudioService { get; }
		public PlayerBulletTime BulletTime { get; }

		public PlayerStateCoordinator StateCoordinator { get; }
		public PlayerBaseStatsRepository StatsRepository { get; }
		public PlayerDerivedStatService DerivedStatService { get; }
		public PlayerResourceService ResourceService { get; }
		public PlayerStatDependencyGraph DependencyGraph { get; }
		public PlayerFlagService FlagService { get; }
		public PlayerSaveCoordinator SaveCoordinator { get; }

		public PlayerWeaponCoordinator WeaponCoordinator { get; }
		public PlayerInventoryCoordinator InventoryCoordinator { get; }

		public PlayerRuntime(
			PlayerPrefab prefab,
			PlayerJumpKit jumpKit,
			PlayerAnimationCoordinator animator,
			PlayerMovementController movementController,
			PlayerParkourController parkourController,
			PlayerAudioService audioService,
			PlayerBulletTime bulletTime,
			PlayerStateCoordinator stateCoordinator,
			PlayerBaseStatsRepository statsRepository,
			PlayerDerivedStatService derivedStatService,
			PlayerResourceService resourceService,
			PlayerStatDependencyGraph dependencyGraph,
			PlayerFlagService flagService,
			PlayerSaveCoordinator saveCoordinator,
			PlayerWeaponCoordinator weaponCoordinator,
			PlayerInventoryCoordinator inventoryCoordinator
		)
		{
			Prefab = prefab;
			JumpKit = jumpKit;
			Animator = animator;
			MovementController = movementController;
			ParkourController = parkourController;
			AudioService = audioService;
			BulletTime = bulletTime;
			StateCoordinator = stateCoordinator;
			StatsRepository = statsRepository;
			DerivedStatService = derivedStatService;
			ResourceService = resourceService;
			DependencyGraph = dependencyGraph;
			FlagService = flagService;
			SaveCoordinator = saveCoordinator;
			WeaponCoordinator = weaponCoordinator;
			InventoryCoordinator = inventoryCoordinator;

			Prefab.GetTree().ProcessFrame += OnProcessFrame;
			Prefab.GetTree().PhysicsFrame += OnPhysicsFrame;
		}

		public void OnProcessFrame()
		{
			float deltaTime = (float)Prefab.GetProcessDeltaTime();

			BulletTime.OnProcessFrame( deltaTime );
			AudioService.OnProcessFrame( deltaTime );
		}

		public void OnPhysicsFrame()
		{
			float deltaTime = (float)Prefab.GetPhysicsProcessDeltaTime();

			MovementController.OnPhysicsUpdate( deltaTime );
		}

		public void Dispose()
		{
			Prefab.GetTree().ProcessFrame -= OnProcessFrame;
			Prefab.GetTree().PhysicsFrame -= OnPhysicsFrame;

			BulletTime?.Dispose();
			ParkourController?.Dispose();
			Animator?.Dispose();
		}
	};
};
