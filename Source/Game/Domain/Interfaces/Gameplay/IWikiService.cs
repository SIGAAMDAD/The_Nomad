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
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.Gameplay;
using Nomad.Game.Domain.Events.Gameplay;

namespace Nomad.Game.Domain.Interfaces.Gameplay
{
	public interface IWikiService : IDisposable
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.Gameplay", PayloadName = "WikiPageFoundEventArgs" )]
		[EventPayload( "PageId", typeof( InternString ) )]
		IGameEvent<WikiPageFoundEventArgs> PageFound { get; }

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
		/// Attempts a retrieval of a Valden's Book page.
		/// </summary>
		/// <param name="pageId"></param>
		/// <param name="localizationName"></param>
		/// <param name="localizationDescription"></param>
		/// <returns><c>true</c> if the page has been unlocked, <c>false</c> if otherwise.</returns>
		bool TryTranslatePage( InternString pageId, out string localizationName, out string localizationDescription );
	};
};
