/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Game.Domain.Events.UI;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System.Collections.Generic;

namespace Game.Application.UI.Menus {
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

	public class MenuStateMachine<TState>( InternString menuId, TState currentMenu, IGameEventRegistryService eventFactory, IReadOnlyDictionary<TState, Control?> states )
		where TState : unmanaged
	{
		public TState CurrentState => currentMenu;
		public InternString OwnerId => menuId;

		private readonly IReadOnlyDictionary<TState, Control?> _states = states;

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
			if ( !_states.TryGetValue( stateId, out Control? newState ) ) {
				// NOTE: this might need... extra verification
				return;
			}

			TState oldMenu = currentMenu;
			_states[ oldMenu ]?.CallDeferred( Control.MethodName.Hide );
			currentMenu = stateId;
			newState?.CallDeferred( Control.MethodName.Show );

			UIEventHelper.PublishUIEvent( eventFactory, UIConstants.MENU_STATE_CHANGED_EVENT, new MenuStateChangedEventArgs<TState>( menuId, oldMenu, currentMenu ) );
		}
	};
};