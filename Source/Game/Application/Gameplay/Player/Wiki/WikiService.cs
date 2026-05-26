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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Nomad.Core.Engine.Services;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Player;

namespace Nomad.Game.Application.Gameplay.Player.Wiki
{
	/*
	===================================================================================

	WikiService

	===================================================================================
	*/
	/// <summary>
	/// A cache entry table of localizations stored and indexed with <see cref="InternString"/>. Contains both
	/// Valden's Book and Journal entries.
	/// </summary>
	/// <remarks>
	/// Owns the translation and the generation of the cache entries but not the events themselves.
	/// </remarks>

	internal abstract class WikiService : IWikiService
	{
		private readonly ConcurrentDictionary<InternString, WikiEntry> _entryCache = new();
		private readonly ILocalizationService _localizationService;

		/*
		===============
		WikiService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="localizationService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public WikiService( ILocalizationService localizationService )
		{
			_localizationService = localizationService ?? throw new ArgumentNullException( nameof( localizationService ) );
		}

		/*
		===============
		GetWikiEntries
		===============
		*/
		/// <summary>
		/// Retrieves a snapshot of the currently unlocked wiki entries.
		/// </summary>
		/// <returns></returns>
		public virtual IReadOnlyList<WikiEntry> GetWikiEntries()
		{
			return _entryCache.Values.ToArray();
		}

		/*
		===============
		IsPageUnlocked
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="pageId"></param>
		/// <returns></returns>
		public virtual bool IsPageUnlocked( InternString pageId )
		{
			return _entryCache.ContainsKey( pageId );
		}

		/*
		===============
		TryTranslatePage
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="pageId"></param>
		/// <param name="translatedName"></param>
		/// <param name="translatedDescription"></param>
		/// <returns></returns>
		public virtual bool TryTranslatePage( InternString pageId, out string translatedName, out string translatedDescription )
		{
			if ( !_entryCache.TryGetValue( pageId, out var entry ) ) {
				translatedName = string.Empty;
				translatedDescription = string.Empty;
				return false;
			}

			translatedName = _localizationService.Translate( entry.Name );
			translatedDescription = _localizationService.Translate( entry.Description );

			return true;
		}

		/*
		===============
		TryUnlockPage
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="pageId"></param>
		/// <returns></returns>
		public virtual bool TryUnlockPage( InternString pageId )
		{
			if ( !_entryCache.TryGetValue( pageId, out var entry ) ) {
				entry = new WikiEntry( pageId );
				_entryCache[pageId] = entry;
			}

			return entry.TryUnlock();
		}
	};
};
