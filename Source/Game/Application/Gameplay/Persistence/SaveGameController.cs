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
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Save.Services;
using Nomad.Save.ValueObjects;

namespace Nomad.Game.Application.Gameplay.Persistence {
	/*
	===================================================================================
	
	SaveGameController

	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class SaveGameController : IDisposable {
		private readonly IGameEventRegistryService _eventFactory;
		private readonly ISaveDataProvider _dataProvider;

		private IReadOnlyList<SaveFileMetadata> _saveFiles;

		private bool _isDisposed = false;

		/*
		===============
		SaveGameController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataProvider"></param>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public SaveGameController( ISaveDataProvider dataProvider, IGameEventRegistryService eventFactory ) {
			_dataProvider = dataProvider ?? throw new ArgumentNullException( nameof( dataProvider ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_saveFiles = _dataProvider.ListSaveFiles();

			_eventFactory
				.GetEvent<PlayerSpawnResultEventArgs>( EventNames.PLAYER_SPAWN_RESULT_READY, EventNames.NAMESPACE )
				.Subscribe( OnCheckInitialSave );
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
			if ( !_isDisposed ) {
				_eventFactory
					.GetEvent<PlayerSpawnResultEventArgs>( EventNames.PLAYER_SPAWN_RESULT_READY, EventNames.NAMESPACE )
					.Unsubscribe( OnCheckInitialSave );
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnCheckInitialSave( in PlayerSpawnResultEventArgs args ) {
			_dataProvider.Save(
				$"Data{_saveFiles.Count + 1}",
				new GameVersion( 2, 0, 1 ) // TODO: make this is an automated constant
			);
			_saveFiles = _dataProvider.ListSaveFiles();
		}
	};
};