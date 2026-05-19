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
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Application.Gameplay.Player.Animation;
using Nomad.Game.Application.Gameplay.Player.JumpKit;
using Nomad.Game.Application.Gameplay.Player.State;
using Nomad.Game.Application.Gameplay.Player.Stats;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Game.Application.Gameplay.Inventory;
using Nomad.Game.Application.Gameplay.Entity;
using Nomad.Game.Domain.Events.Entity;
using Nomad.Game.Application.Gameplay.Player.Input;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Entities;
using Nomad.Game.Domain.Data.Player.State;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerBase

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class PlayerBase : EntityBase, IPlayerBase
	{
		public PlayerId PlayerId => new PlayerId( Id );

		//
		// Components
		//
		private readonly PlayerJumpKit _jumpKit;
		private readonly PlayerAnimationCoordinator _animator;
		private readonly PlayerMovementController _movementController;
		private readonly PlayerAudioService _audioService;
		private readonly PlayerBulletTime _bulletTime;

		//
		// Services & Repositories
		//
		private readonly PlayerStateCoordinator _stateCoordinator;
		private readonly PlayerBaseStatsRepository _statsRepository;
		private readonly PlayerDerivedStatService _derivedStatService;
		private readonly PlayerResourceService _resourceService;
		private readonly PlayerStatDependencyGraph _dependencyGraph;
		private readonly PlayerFlagService _flagService;
		private readonly PlayerSaveCoordinator _saveCoordinator;
		private readonly InventoryContainer _container;

		private readonly PlayerPrefab _prefab;

		public IGameEvent<PlayerDieEventArgs> Die => _die;
		private readonly IGameEvent<PlayerDieEventArgs> _die;

		/*
		===============
		PlayerBase
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="prefab"></param>
		/// <param name="scope"></param>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		public PlayerBase( PlayerId playerId, PlayerPrefab prefab, IServiceRegistry scope, IGameEventRegistryService eventFactory, ILoggerService logger )
			: base( new EntityId( playerId.Id ), eventFactory )
		{
			_prefab = prefab;
			_die = eventFactory.GetEvent<PlayerDieEventArgs>(
				PlayerDieEventArgs.Name,
				PlayerDieEventArgs.NameSpace
			);

			entityDie.Subscribe( OnBaseDie );

			_statsRepository = new PlayerBaseStatsRepository( playerId, eventFactory, logger );
			_dependencyGraph = PlayerStatDependencyGraph.CreateDefault();
			_derivedStatService = new PlayerDerivedStatService( playerId, _statsRepository, _dependencyGraph, eventFactory );
			_flagService = new PlayerFlagService( playerId, eventFactory );
			_resourceService = new PlayerResourceService( playerId, _derivedStatService, eventFactory );
			_stateCoordinator = new PlayerStateCoordinator( playerId, PlayerStateId.Idle, eventFactory );
			_saveCoordinator = new PlayerSaveCoordinator( _derivedStatService, _resourceService, _stateCoordinator, _prefab, eventFactory );

			foreach ( var pair in prefab.Definition.Stats.BaseStats ) {
				_statsRepository.SetBaseStatValue( pair.Key, pair.Value );
			}
			_derivedStatService.FlushDirty();

			_movementController = prefab.AddComponent<PlayerMovementController>( comp => {
				comp.Stats = _derivedStatService;
				comp.Flags = _flagService;
				comp.Id = playerId;
				comp.InputSource = new LocalPlayerInputSource( playerId, eventFactory );
			} );
			_jumpKit = prefab.AddComponent<PlayerJumpKit>( comp => {
				comp.Id = playerId;
			} );
			_audioService = prefab.AddComponent<PlayerAudioService>( comp => {
				comp.Id = playerId;
				comp.FlagService = _flagService;
			} );
			_animator = new PlayerAnimationCoordinator( playerId, prefab, PlayerAnimationState.Idle, _stateCoordinator, _movementController );
			_bulletTime = prefab.AddComponent<PlayerBulletTime>( comp => {
				comp.FlagService = _flagService;
				comp.DerivedStatService = _derivedStatService;
				comp.ResourceService = _resourceService;
			} );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			_die?.Dispose();
		}

		/*
		===============
		ApplySpawnProfile
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="spawnApplicator"></param>
		/// <param name="profile"></param>
		/// <param name="context"></param>
		public void ApplySpawnProfile( IPlayerSpawnApplicator spawnApplicator, PlayerSpawnProfileDefinition profile, in PlayerSpawnContext context )
		{
			spawnApplicator.Apply( this, profile, _derivedStatService, _resourceService, _flagService, in context );
		}

		private void OnBaseDie( in EntityDieEventArgs args )
		{
			_die.Publish(
				new PlayerDieEventArgs(
					playerId: PlayerId,
					killerId: args.AttackerId
				)
			);
		}
	};
};
