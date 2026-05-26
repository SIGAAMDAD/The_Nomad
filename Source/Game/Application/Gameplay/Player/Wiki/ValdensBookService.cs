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
using Nomad.Game.Sdk.Gameplay;
using Nomad.Game.Sdk.Events.Gameplay;
using Nomad.Game.Sdk.Player;
using Nomad.Save.Services;
using Nomad.Game.Application.Gameplay.Player;

namespace Nomad.Game.Application.Gameplay.Player.Wiki
{
	/*
	===================================================================================

	ValdensBookService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>
	/// <remarks>
	/// Owns: wiki page unlock event, wiki page entry cache. But does not own the actual unlocking of wiki pages that itself is handled by <see cref="WikiEntry"/>.
	/// </remarks>

	internal sealed class ValdensBookService : WikiService, IValdensBookService
	{
		private readonly IDisposable _saveBegin;

		public IGameEvent<ValdensBookPageFoundEventArgs> PageFound => _pageFound;
		private readonly IGameEvent<ValdensBookPageFoundEventArgs> _pageFound = null;

		/*
		===============
		ValdensBookService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="localizationService"></param>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public ValdensBookService( ILocalizationService localizationService, IGameEventRegistryService eventFactory )
			: base( localizationService )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_saveBegin = eventFactory
				.GetEvent<SaveBeginEventArgs>(
					SaveBeginEventArgs.Name,
					SaveBeginEventArgs.NameSpace
				)
				.Subscribe( OnSaveBegin );

			_pageFound = eventFactory.GetEvent<ValdensBookPageFoundEventArgs>(
				ValdensBookPageFoundEventArgs.Name,
				ValdensBookPageFoundEventArgs.NameSpace
			);
		}

		public void Dispose()
		{
			_saveBegin?.Dispose();
			_pageFound?.Dispose();
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
		public override bool TryUnlockPage( InternString pageId )
		{
			if ( base.TryUnlockPage( pageId ) ) {
				_pageFound.Publish(
					new ValdensBookPageFoundEventArgs(
						pageId
					)
				);
				return true;
			}
			return false;
		}

		/*
		===============
		OnSaveBegin
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnSaveBegin( in SaveBeginEventArgs args )
		{
			lock ( this ) {
				var writer = args.Writer.AddSection( "ValdensBookData" );

				var entries = GetWikiEntries();
				writer.AddField( "EntryCount", entries.Count );
				for ( int i = 0; i < entries.Count; i++ ) {
					writer.AddField( $"UnlockedEntry{i}", (string)entries[i].Name );
				}
			}
		}
	};
};
