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

using Nomad.Core.Engine.Globals;
using Nomad.Core.Engine.SceneManagement;
using Nomad.Core.Engine.Services;
using Nomad.Events.Globals;
using Nomad.Game.Presentation.Menus;
using Nomad.UI;

namespace Nomad.Game.Presentation.Screens.MenuHub
{
	/*
	===================================================================================

	MenuHub

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class MenuHub : EnginePanel
	{
		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void OnInit()
		{
			//			SceneManager.LoadScene( EngineService.GetStoragePath( "Prefabs/MenuBackground/MenuBackground.tscn", StorageScope.StreamingAssets ), LoadSceneMode.Additive );
			//			GameEventRegistry.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE ).Publish( new MenuTransitionRequestedEventArgs( MenuState.Splash, MenuState.Splash ) );
		}
	};
};
