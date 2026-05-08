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
using System.Collections.Generic;
using Nomad.Save.Services;
using Nomad.Save.ValueObjects;

namespace Nomad.Game.Presentation.Screens.LoadGameMenu
{
	/*
	===================================================================================
	
	LoadGameMenuModel
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class LoadGameMenuModel
	{
		private readonly ISaveDataProvider _saveDataProvider;

		/*
		===============
		LoadGameMenuModel
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="saveDataProvider"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public LoadGameMenuModel( ISaveDataProvider saveDataProvider )
		{
			_saveDataProvider = saveDataProvider ?? throw new ArgumentNullException( nameof( saveDataProvider ) );
		}

		/*
		===============
		GetSlots
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IReadOnlyList<SaveFileMetadata> GetSlots()
		{
			return _saveDataProvider.ListSaveFiles();
		}
	};
};
