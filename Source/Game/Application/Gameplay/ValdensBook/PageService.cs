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
using Nomad.Core.Events;
using Nomad.Game.Domain.Events.ValdensBook;

namespace Nomad.Game.Application.Gameplay.ValdensBook {
	/*
	===================================================================================
	
	PageService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class PageService : IDisposable {
		private readonly PageRepository _repository;

		private bool _isDisposed = false;

		public IGameEvent<PageUnlockedEventArgs> PageUnlocked => _pageUnlocked;
		private readonly IGameEvent<PageUnlockedEventArgs> _pageUnlocked;
		
		public IGameEvent<PageReadEventArgs> PageRead => _pageRead;
		private readonly IGameEvent<PageReadEventArgs> _pageRead;

		public PageService( IGameEventRegistryService eventFactory ) {
			_pageUnlocked = eventFactory.GetEvent<PageUnlockedEventArgs>( EventNames.PAGE_UNLOCKED, EventNames.NAMESPACE );
			_pageRead = eventFactory.GetEvent<PageReadEventArgs>( EventNames.PAGE_READ, EventNames.NAMESPACE );

			_repository = new PageRepository();
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			// DISPOSAL GOES HERE
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}
	};
};