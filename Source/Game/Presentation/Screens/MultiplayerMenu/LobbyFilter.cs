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
using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.Game.Application.UI;

namespace Nomad.Game.Presentation.Screens.MultiplayerMenu {
	/*
	===================================================================================
	
	LobbyFilter
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class LobbyFilter : VBoxContainer {
		public IGameEvent<bool> ShowFullLobbiesChanged => _showFullLobbiesChanged;
		private IGameEvent<bool> _showFullLobbiesChanged;

		public IGameEvent<int> MapAddedToFilter => _mapAddedToFilter;
		private IGameEvent<int> _mapAddedToFilter;

		public IGameEvent<int> MapRemovedFromFilter => _mapRemovedFromFilter;
		private IGameEvent<int> _mapRemovedFromFilter;

		public IGameEvent<int> GameModeAddedToFilter => _gameModeAddedToFilter;
		private IGameEvent<int> _gameModeAddedToFilter;

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			_showFullLobbiesChanged = GameEventRegistry.GetEvent<bool>( UIConstants.SHOW_FULL_LOBBIES_CHANGED_EVENT, UIConstants.NAMESPACE );
		}
	};
};