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

using Nomad.Save.Services;

namespace Nomad.Game.Presentation.Screens.LoadGameMenu
{
	/*
	===================================================================================
	
	LoadGameMenuPresenter
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class LoadGameMenuPresenter
	{
		public LoadGameMenuPresenter( LoadGameMenuView view, LoadGameMenuModel model, ISaveDataProvider dataProvider )
		{
			view.SlotSelected += OnSlotSelected;
			view.Back += OnBackPressed;

			var files = dataProvider.ListSaveFiles();
			foreach ( var file in files ) {
				view.AddSlot( file );
			}
		}

		private void OnSlotSelected( int slot )
		{
		}

		private void OnBackPressed()
		{
		}
	};
};
