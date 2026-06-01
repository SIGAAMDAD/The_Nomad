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
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Save.Services;
using Nomad.Save.ValueObjects;

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

	internal sealed class LoadGameMenuPresenter : IDisposable
	{
		private readonly ILoadGameMenuView _view;

		private readonly IGameEventRegistryService _eventFactory;
		private readonly ISaveDataProvider _saveDataProvider;
		private readonly IGameSessionService _gameSessionService;
		private int _selectedSlotIndex = -1;

		private bool _isDisposed = false;

		/*
		===============
		LoadGameMenuPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="view"></param>
		/// <param name="dataProvider"></param>
		/// <param name="eventFactory"></param>
		public LoadGameMenuPresenter( ILoadGameMenuView view, ISaveDataProvider dataProvider, IGameSessionService gameSessionService, IGameEventRegistryService eventFactory )
		{
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_saveDataProvider = dataProvider ?? throw new ArgumentNullException( nameof( dataProvider ) );
			_gameSessionService = gameSessionService ?? throw new ArgumentNullException( nameof( gameSessionService ) );

			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_view.SlotSelected += OnSlotSelected;
			_view.Back += OnBackPressed;
			_view.LoadSlot += OnLoadSlot;
			_view.DeleteSlot += OnDeleteSlot;

			IReadOnlyList<SaveFileMetadata> files = _saveDataProvider.ListSaveFiles();
			for ( int i = 0; i < files.Count; i++ ) {
				view.AddSlot( files[i].SaveName, i );
			}
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_view.SlotSelected -= OnSlotSelected;
			_view.Back -= OnBackPressed;
			_view.LoadSlot -= OnLoadSlot;
			_view.DeleteSlot -= OnDeleteSlot;

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnDeleteSlot()
		{
		}

		private void OnLoadSlot()
		{
			if ( _selectedSlotIndex < 0 ) {
				return;
			}

			IReadOnlyList<SaveFileMetadata> files = _saveDataProvider.ListSaveFiles();
			if ( _selectedSlotIndex >= files.Count ) {
				return;
			}

			_ = _gameSessionService.LoadSinglePlayerAsync( files[_selectedSlotIndex].SaveName );
		}

		/*
		===============
		OnSlotSelected
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="slot"></param>
		private void OnSlotSelected( int slot )
		{
			IReadOnlyList<SaveFileMetadata> files = _saveDataProvider.ListSaveFiles();
			if ( slot < 0 || slot >= files.Count ) {
				return;
			}

			_selectedSlotIndex = slot;
			var file = files[slot];
			_view.SetSlotName( file.SaveName );
			_view.SetLastAccessedTime( file.LastAccessTime.ToLongDateString() );
		}

		/*
		===============
		OnBackPressed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnBackPressed()
		{
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>(
					MenuTransitionRequestedEventArgs.Name,
					MenuTransitionRequestedEventArgs.NameSpace
				)
				.Publish(
					new MenuTransitionRequestedEventArgs(
						MenuState.LoadGame,
						MenuState.Main
					)
				);
		}
	};
};
