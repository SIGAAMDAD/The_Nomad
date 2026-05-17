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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;
using Nomad.Game.Domain.Interfaces.Gameplay;
using Nomad.Save.Services;

namespace Nomad.Game.Application.Gameplay.ValdensBook
{
	/*
	===================================================================================

	WikiService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>
	/// <remarks>
	/// Owns: wiki page unlock event, wiki page entry cache. But does not own the actual unlocking of wiki pages that itself is handled by <see cref="WikiEntry"/>.
	/// </remarks>

	internal sealed class WikiService : IWikiService
	{
		private readonly IDisposable _saveBegin;

		private readonly ConcurrentDictionary<InternString, WikiEntry> _entryCache = new();
		private readonly ILocalizationService _localizationService;

		public IGameEvent<WikiPageFoundEventArgs> PageFound => _pageFound;
		private readonly IGameEvent<WikiPageFoundEventArgs> _pageFound = null;

		public WikiService( ILocalizationService localizationService, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_localizationService = localizationService ?? throw new ArgumentNullException( nameof( localizationService ) );

			_saveBegin = eventFactory
				.GetEvent<SaveBeginEventArgs>(
					SaveBeginEventArgs.Name,
					SaveBeginEventArgs.NameSpace
				)
				.Subscribe( OnSaveBegin );

			_pageFound = eventFactory.GetEvent<WikiPageFoundEventArgs>(
				WikiPageFoundEventArgs.Name,
				WikiPageFoundEventArgs.NameSpace
			);
		}

		public void Dispose()
		{
			_saveBegin?.Dispose();
			_pageFound?.Dispose();
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
		public bool IsPageUnlocked( InternString pageId )
		{
			return _entryCache.ContainsKey( pageId );
		}

		/*
		===============
		TryGetPage
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="pageId"></param>
		/// <param name="page"></param>
		/// <returns></returns>
		public bool TryGetPage( InternString pageId, out WikiEntry page )
		{
			return _entryCache.TryGetValue( pageId, out page );
		}

		public bool TryUnlockPage( InternString pageId )
		{
			return true;
		}

		private void OnSaveBegin( in SaveBeginEventArgs args )
		{
			lock ( this ) {
				var writer = args.Writer.AddSection( "WikiData" );
			}
		}
	};
};
