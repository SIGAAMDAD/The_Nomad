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
using Nomad.Core.Engine.Services;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Game.Domain.Interfaces.Gameplay;

namespace Nomad.Game.Presentation.Screens.PauseMenu
{
	/*
	===================================================================================
	
	PauseMenuFactory
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal static class PauseMenuFactory
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
		public static PauseMenuPresenter Create( PauseMenuView view )
		{
			var serviceLocator = ServiceLocator.Instance;
			var emitterFactory = serviceLocator.GetService<IEmitterFactory>();
			var engineService = serviceLocator.GetService<IEngineService>();
			var gameStateService = serviceLocator.GetService<IGameStateService>();
			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();
			var pauseService = serviceLocator.GetService<IGamePauseService>();

			return new PauseMenuPresenter(
				view,
				new PauseMenuModel( emitterFactory ),
				engineService,
				eventFactory,
				gameStateService,
				pauseService
			);
		}
	};
};
