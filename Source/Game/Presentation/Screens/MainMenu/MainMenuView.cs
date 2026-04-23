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

using Nomad.Audio.Interfaces;
using Nomad.Core.Engine.Globals;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using System;
using Godot;

namespace Nomad.Game.Presentation.Screens.MainMenu {
	/*
	===================================================================================
	
	MainMenuView
	
	===================================================================================
	*/
	/// <summary>
	/// Handles the main menu's creation.
	/// </summary>
	
	internal sealed partial class MainMenuView : Control {
		public event Action NewGame;
		public event Action LoadGame;
		public event Action Extras;
		public event Action Settings;
		public event Action QuitGame;

		private readonly MainMenuPresenter _presenter;

		public MainMenuView() {
			_presenter = new MainMenuPresenter(
				this,
				ServiceLocator.GetService<IEngineService>(),
				ServiceLocator.GetService<IGameEventRegistryService>()
			);
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			var audioDevice = ServiceLocator.GetService<IAudioDevice>();
			audioDevice.LoadBank( EngineService.GetStoragePath( "Audio/Banks/Desktop/ui.bank", StorageScope.StreamingAssets ) );
			audioDevice.LoadBank( EngineService.GetStoragePath( "Audio/Banks/Desktop/music.bank", StorageScope.StreamingAssets ) );

			var musicService = ServiceLocator.GetService<IMusicService>();
			musicService.PlayTheme( "event:/Music/UserInterface/MainMenuTheme" );

			GetNode<Button>( "OptionsContainer/NewGameButton" ).Pressed += () => NewGame?.Invoke();
			GetNode<Button>( "OptionsContainer/LoadGameButton" ).Pressed += () => LoadGame?.Invoke();
			GetNode<Button>( "OptionsContainer/ExtrasButton" ).Pressed += () => Extras?.Invoke();
			GetNode<Button>( "OptionsContainer/SettingsButton" ).Pressed += () => Settings?.Invoke();
			GetNode<Button>( "OptionsContainer/QuitGameButton" ).Pressed += () => QuitGame?.Invoke();
		}
	};
};
