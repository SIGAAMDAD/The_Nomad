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
using Nomad.Core.Util;
using Nomad.Game.Sdk.Gameplay;

namespace Nomad.Game.Sdk.Player
{
	/// <summary>
	///
	/// </summary>
	public interface IWikiService
	{
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		IReadOnlyList<WikiEntry> GetWikiEntries();

		/// <summary>
		///
		/// </summary>
		/// <param name="pageId"></param>
		/// <returns></returns>
		bool IsPageUnlocked( InternString pageId );

		/// <summary>
		///
		/// </summary>
		/// <param name="pageId"></param>
		/// <returns></returns>
		bool TryUnlockPage( InternString pageId );

		/// <summary>
		/// Attemps a retrieval of a wiki page.
		/// </summary>
		/// <param name="pageId"></param>
		/// <param name="translatedName"></param>
		/// <param name="translatedDescription"></param>
		/// <returns></returns>
		bool TryTranslatePage( InternString pageId, out string translatedName, out string translatedDescription );
	}
}
