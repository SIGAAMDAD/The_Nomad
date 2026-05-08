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
using Nomad.Core.CVars;
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.FileSystem;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.EngineUtils.Settings.Services;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================
	
	SettingsMenuFactory
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal static class SettingsMenuFactory
	{
		/*
		===============
		Create
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		/// <returns></returns>
		public static SettingsMenuPresenter Create( SettingsMenuView view )
		{
			var serviceLocator = ServiceLocator.Instance;
			var cvarSystem = serviceLocator.GetService<ICVarSystemService>();
			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();
			var fileSystem = serviceLocator.GetService<IFileSystem>();
			var audioDevice = serviceLocator.GetService<IAudioDevice>();
			var windowService = serviceLocator.GetService<IWindowService>();
			var displayService = serviceLocator.GetService<IDisplayService>();

			var sections = new ISettingsSectionPresenter[] {
				new AudioSettingsContainerPresenter(
					view.AudioView,
					new AudioSettingsContainerModel(
						audioDevice,
						new AudioSettingsService( audioDevice, cvarSystem ),
						cvarSystem
					)
				),
				new DisplaySettingsContainerPresenter(
					view.DisplayView,
					new DisplaySettingsContainerModel(
						windowService,
						new DisplaySettingsService(
							displayService,
							cvarSystem
						)
					)
				)
			};

			return new SettingsMenuPresenter(
				view, cvarSystem, eventFactory, fileSystem, sections
			);
		}
	};
};
