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
using Nomad.Core.Logger;
using Nomad.Core.ServiceRegistry.Interfaces;
using Nomad.Game.Application.Gameplay.Player.Animation;
using Nomad.Game.Application.Gameplay.Player.JumpKit;
using Nomad.Game.Application.Gameplay.Player.Stats;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Logger.Globals;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerBase
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public abstract class PlayerBase : IPlayerBase {
		public Guid Id => _id;
		private readonly Guid _id;

		//
		// Components
		//
		private readonly PlayerJumpKit _jumpKit;
		private readonly PlayerAnimationRepository _animator;
		private readonly PlayerMovementController _movementController;
		private readonly PlayerAudioService _audioService;

		//
		// Services & Repositories
		//
		private readonly IPlayerBaseStatsRepository _statsRepository;
		private readonly IPlayerDerivedStatService _derivedStatService;
		private readonly IPlayerResourceService _resourceService;
		private readonly PlayerStatDependencyGraph _dependencyGraph;
		private readonly IPlayerFlagService _flagService;
		
		private readonly PlayerPrefab _prefab;

		private bool _isDisposed = false;

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
		/// <param name="guid"></param>
		/// <param name="prefab"></param>
		/// <param name="scope"></param>
		/// <param name="eventFactory"></param>
		/// <param name="logger"></param>
		public PlayerBase( Guid guid, PlayerPrefab prefab, IServiceRegistry scope, IGameEventRegistryService eventFactory, ILoggerService logger ) {
			_prefab = prefab;
			_id = guid;
			_die = eventFactory.GetEvent<PlayerDieEventArgs>(
				$"{_id}:{EventNames.PLAYER_DIE}",
				EventNames.NAMESPACE
			);

			_statsRepository = new PlayerBaseStatsRepository( eventFactory, logger );
			_dependencyGraph = new PlayerStatDependencyGraph();
			_derivedStatService = new PlayerDerivedStatService( _statsRepository, _dependencyGraph, eventFactory );
			_flagService = new PlayerFlagService( eventFactory );
			_resourceService = new PlayerResourceService( _derivedStatService, eventFactory );

			foreach ( var pair in prefab.Definition.Stats.BaseStats ) {
				_statsRepository.SetBaseStatValue( pair.Key, pair.Value );
			}
			_derivedStatService.FlushDirty();

			_movementController = prefab.AddComponent<PlayerMovementController>(comp => {
				comp.Stats = _derivedStatService;
				comp.Flags = _flagService;
			} );
			_jumpKit = prefab.AddComponent<PlayerJumpKit>();
			_audioService = prefab.AddComponent<PlayerAudioService>();
			_animator = new PlayerAnimationRepository( prefab, _movementController );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			if ( !_isDisposed ) {
				_die?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public void ApplySpawnProfile( IPlayerSpawnApplicator spawnApplicator, PlayerSpawnProfileDefinition profile, in PlayerSpawnContext context ) {
			spawnApplicator.Apply( this, profile, _derivedStatService, _resourceService, _flagService, in context );
		}
	};
};
