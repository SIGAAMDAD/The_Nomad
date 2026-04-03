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
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using System.Collections.Concurrent;
using Nomad.Game.Domain.Interfaces.Player;

namespace Nomad.Game.Application.Gameplay.Player.JumpKit {
	/*
	===================================================================================
	
	DashModuleRepository

	===================================================================================
	*/
	/// <summary>
	/// Handles dash module upgrades, which modules we have, which modules are unlocked.
	/// </summary>
	
	internal sealed class DashModuleRepository : IDashModuleRepository {
		private readonly ISubscriptionHandle _dashModuleUnlocked;
		private readonly DashModuleFactory _factory;

		/// <summary>
		/// The currently unlocked dash modules.
		/// </summary>
		private readonly ConcurrentDictionary<Type, IDashModule> _availableModules = new();

		/*
		===============
		DashModuleRepository
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		public DashModuleRepository( IGameEventRegistryService eventFactory, DashModuleFactory factory ) {
			var dashModuleUnlocked = eventFactory.GetEvent<PlayerDashModuleUnlockedEventArgs>( EventNames.PLAYER_DASH_MODULE_CHANGED, EventNames.NAMESPACE );
			_dashModuleUnlocked = dashModuleUnlocked.Subscribe( OnDashModuleUnlocked );

			_factory = factory ?? throw new ArgumentNullException( nameof( factory ) );
		}

		/*
		===============
		HasModuleBlueprint
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool HasModuleBlueprint<T>()
			where T : IDashModule
		{
			return _availableModules.ContainsKey( typeof( T ) );
		}

		/*
		===============
		OnDashModuleUnlocked
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnDashModuleUnlocked( in PlayerDashModuleUnlockedEventArgs args ) {
			if ( _availableModules.ContainsKey( args.ModuleType ) ) {
				return;
			}
			_availableModules[ args.ModuleType ] = _factory.CreateModule( args.ModuleType );
		}
	};
};