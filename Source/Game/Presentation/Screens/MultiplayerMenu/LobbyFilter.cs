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
using Nomad.Events.Globals;
using Nomad.Game.Application.UI;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.MultiplayerMenu {
	public sealed partial class LobbyFilter : EngineVerticalContainer {
		public IGameEvent<bool> ShowFullLobbiesChanged => _showFullLobbiesChanged;
		private IGameEvent<bool> _showFullLobbiesChanged;

		public IGameEvent<int> MapAddedToFilter => _mapAddedToFilter;
		private IGameEvent<int> _mapAddedToFilter;

		public IGameEvent<int> MapRemovedFromFilter => ;

		public IGameEvent<int> GameModeAddedToFilter => _gameModeAddedToFilter;
		private IGameEvent<int> _gameModeAddedToFilter;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			base.OnInit();

			_mapAddedToFilter = GameEventRegistry.GetEvent<int>();
			_gameModeAddedToFilter = GameEventRegistry.GetEvent<int>();
			_showFullLobbiesChanged = GameEventRegistry.GetEvent<bool>( UIConstants.SHOW_FULL_LOBBIES_CHANGED_EVENT, UIConstants.NAMESPACE );
		}

		/*
		===============
		OnShutdown
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnShutdown() {
			base.OnShutdown();
		}
	};
};