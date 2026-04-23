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
using Nomad.Core.CVars;
using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Game.Application.UI;
using Nomad.Game.Application.UI.Menus;
using Nomad.Game.Application.UI.Menus.Events;

namespace Nomad.Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	SettingsMenuPresenter
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class SettingsMenuPresenter {
		private readonly SettingsMenuView _view;

		private readonly IGameEventRegistryService _eventFactory;
		private readonly ICVarSystemService _cvarSystem;
		private readonly IFileSystem _fileSystem;
		private readonly IReadOnlyList<ISettingsSectionPresenter> _sections;

		/*
		===============
		SettingsMenuPresenter
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		/// <param name="cvarSystem"></param>
		public SettingsMenuPresenter(
			SettingsMenuView view,
			ICVarSystemService cvarSystem,
			IGameEventRegistryService eventFactory,
			IFileSystem fileSystem,
			IReadOnlyList<ISettingsSectionPresenter> sections
		) {
			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );
			_cvarSystem = cvarSystem ?? throw new ArgumentNullException( nameof( cvarSystem ) );
			_fileSystem = fileSystem ?? throw new ArgumentNullException( nameof( fileSystem ) );
			_sections = sections ?? throw new ArgumentNullException( nameof( sections ) );

			_view.SaveRequested += OnSaveRequested;
			_view.ResetRequested += OnResetRequested;
			_view.BackRequested += OnBackRequested;
		}

		/*
		===============
		OnSaveRequested
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSaveRequested() {
			foreach ( var section in _sections ) {
				section.Save();
			}

			_cvarSystem.Save( _fileSystem, $"{_fileSystem.GetUserDataPath()}/UserConfig.ini" );
		}

		/*
		===============
		OnResetRequested
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnResetRequested() {
			foreach ( var section in _sections ) {
				section.Reset();
				section.SyncView();
			}
		}

		/*
		===============
		OnBackRequested
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnBackRequested() {
			_eventFactory
				.GetEvent<MenuTransitionRequestedEventArgs>( UIConstants.MENU_TRANSITION_REQUESTED_EVENT, UIConstants.NAMESPACE )
				.Publish( new MenuTransitionRequestedEventArgs( MenuState.Settings, MenuState.None ) );
		}
	};
};
