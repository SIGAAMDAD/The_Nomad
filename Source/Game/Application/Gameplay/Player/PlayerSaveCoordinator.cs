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
using Nomad.EngineUtils;
using Nomad.Game.Application.Gameplay.Player.JumpKit;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Game.Prefabs;
using Nomad.Save.Data;
using Nomad.Save.Events;

namespace Nomad.Game.Application.Gameplay.Player {
	/*
	===================================================================================
	
	PlayerSaveCoordinator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PlayerSaveCoordinator {
		private readonly IPlayerDerivedStatService _derivedStatService;
		private readonly IPlayerResourceService _resourceService;
		private readonly PlayerPrefab _prefab;
		private readonly IPlayerFlagService _flagService;

		/*
		===============
		PlayerSaveCoordinator
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="derivedStatService"></param>
		/// <param name="resourceService"></param>
		/// <param name="prefab"></param>
		/// <param name="eventFactory"></param>
		public PlayerSaveCoordinator( IPlayerDerivedStatService derivedStatService, IPlayerResourceService resourceService, PlayerPrefab prefab, IGameEventRegistryService eventFactory ) {
			_derivedStatService = derivedStatService ?? throw new ArgumentNullException( nameof( derivedStatService ) );
			_resourceService = resourceService ?? throw new ArgumentNullException( nameof( resourceService ) );
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );

			eventFactory
				.GetEvent<SaveBeginEventArgs>( Save.Data.EventNames.SAVE_BEGIN_EVENT, Save.Data.EventNames.NAMESPACE )
				.Subscribe( OnSaveBegin );
		}

		/*
		===============
		OnSaveBegin
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnSaveBegin( in SaveBeginEventArgs args ) {
			using var section = args.Writer.AddSection( "Player" );

			section.AddField( nameof( DerivedStatType.EffectiveHealthMax ), _derivedStatService.GetValue( DerivedStatType.EffectiveHealthMax ) );
			section.AddField( nameof( DerivedStatType.EffectiveRageMax ), _derivedStatService.GetValue( DerivedStatType.EffectiveRageMax ) );
			section.AddField( nameof( DerivedStatType.EffectiveSanityMax ), _derivedStatService.GetValue( DerivedStatType.EffectiveSanityMax ) );

			var position = _prefab.GlobalPosition.ToSystem();
			section.AddField( "PositionX", position.X );
			section.AddField( "PositionY", position.Y );

			section.AddField( nameof( PlayerResourceType.Health ), _resourceService.GetValue( PlayerResourceType.Health ) );
			section.AddField( nameof( PlayerResourceType.Rage ), _resourceService.GetValue( PlayerResourceType.Rage ) );
			section.AddField( nameof( PlayerResourceType.Sanity ), _resourceService.GetValue( PlayerResourceType.Sanity ) );

			var jumpKit = _prefab.GetComponent<PlayerJumpKit>();
			var module = jumpKit.Module;

			section.AddField( $"DashModule", module.Name );
		}
	};
};