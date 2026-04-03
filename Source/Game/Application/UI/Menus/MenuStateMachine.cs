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

using Nomad.Game.Domain.Events.UI;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Application.UI;
using Nomad.UI;
using System.Collections.Generic;

namespace Nomad.Game.Application.UI.Menus {
	/*
	===================================================================================
	
	MenuStateMachine
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	/// <param name="currentMenu"></param>
	/// <param name="states"></param>

	public class MenuStateMachine<TState>
		where TState : unmanaged
	{
		public TState CurrentState => _currentMenu;
		private TState _currentMenu;

		public InternString OwnerId => _menuId;
		private readonly InternString _menuId;

		private readonly IReadOnlyDictionary<TState, EnginePanel?> _states;
		private readonly IGameEventRegistryService _eventFactory;

		/*
		===============
		MenuStateMachine
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="menuId"></param>
		/// <param name="currentMenu"></param>
		/// <param name="eventFactory"></param>
		/// <param name="states"></param>
		public MenuStateMachine( InternString menuId, TState currentMenu, IGameEventRegistryService eventFactory, IReadOnlyDictionary<TState, EnginePanel?> states ) {
			_menuId = menuId;
			_currentMenu = currentMenu;
			_eventFactory = eventFactory;
			_states = states;
		}

		/*
		===============
		SetState
		===============
		*/
		/// <summary>
		/// Changes the state to the requested index.
		/// </summary>
		/// <param name="stateId"></param>
		public void SetState( TState stateId ) {
			if ( !_states.TryGetValue( stateId, out EnginePanel? newState ) ) {
				// NOTE: this might need... extra verification
				return;
			}

			TState oldMenu = _currentMenu;
			_states[ oldMenu ]?.Visible = false;
			_currentMenu = stateId;
			newState?.Visible = true;

			var menuStateChanged = _eventFactory.GetEvent<MenuStateChangedEventArgs<TState>>( UIConstants.MENU_STATE_CHANGED_EVENT, UIConstants.NAMESPACE );
			menuStateChanged.Publish( new MenuStateChangedEventArgs<TState>( _menuId, oldMenu, _currentMenu ) );
		}
	};
};
