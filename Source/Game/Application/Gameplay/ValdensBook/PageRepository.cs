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
using Nomad.Core.Events;
using Nomad.Core.FileSystem;

namespace Nomad.Game.Application.Gameplay.ValdensBook {
	/*
	===================================================================================
	
	PageRepository
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PageRepository {
		private readonly ConcurrentDictionary<Guid, (string Title, string Text)> _pages = new();

		public PageRepository() {
		}

		/*
		===============
		GetPageText
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pageId"></param>
		/// <returns></returns>
		public string? GetPageText( Guid pageId ) {
			if ( _pages.TryGetValue( pageId, out var page ) ) {
				return page.Text;
			}
			return null;
		}
	};
};