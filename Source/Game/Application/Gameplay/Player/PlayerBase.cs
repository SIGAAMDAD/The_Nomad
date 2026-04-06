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
using Nomad.Game.Application.Gameplay.Player.JumpKit;
using Nomad.Game.Application.Gameplay.Player.Stats;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Scene.GameObjects;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerBase
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public abstract class PlayerBase {		
		private readonly EngineCharacter2D _base;
		private readonly PlayerJumpKit _jumpKit;
		private readonly IPlayerStatsRepository _statsRepository;

		public Guid Id => _id;
		private readonly Guid _id;

		public IGameEvent<PlayerDieEventArgs> Die => _die;
		private readonly IGameEvent<PlayerDieEventArgs> _die;

		public IGameEvent<PlayerStatChangedEventArgs> StatChanged => _statChanged;
		private readonly IGameEvent<PlayerStatChangedEventArgs> _statChanged;

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
			_id = guid;
			_die = eventFactory.GetEvent<PlayerDieEventArgs>( $"{_id}:{EventNames.PLAYER_DIE}", EventNames.NAMESPACE );
			_statChanged = eventFactory.GetEvent<PlayerStatChangedEventArgs>( $"{_id}:{EventNames.PLAYER_STAT_CHANGED}", EventNames.NAMESPACE );

			_jumpKit = _base.AddComponent<PlayerJumpKit>();
			_statsRepository = new PlayerStatsRepository( eventFactory, logger );

			scope.AddSingleton( _statsRepository );
		}
	};
};