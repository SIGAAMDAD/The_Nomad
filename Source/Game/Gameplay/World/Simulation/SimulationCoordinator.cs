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

using Nomad.Core.CVars;
using Nomad.Core.Events;
using Nomad.Game.Presentation.World;
using Nomad.Game.Sdk.World;

namespace Nomad.Game.Gameplay.World.Simulation
{
	/*
	===================================================================================

	SimulationCoordinator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class SimulationCoordinator
	{
		private readonly CalendarService _calendarService;
		private readonly WeatherService _weatherService;
		private readonly SeasonService _seasonService;
		private readonly WorldSunLightController _sunLightController;

		private readonly SimulationSaveCoordinator _saveCoordinator;

		/*
		===============
		SimulationCoordinator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="cvarSystem"></param>
		/// <param name="startDate"></param>
		/// <param name="definition"></param>
		/// <param name="sunLightPrefab"></param>
		public SimulationCoordinator(
			IGameEventRegistryService eventFactory,
			ICVarSystemService cvarSystem,
			WorldTime startDate,
			WorldDefinition definition
		)
		{
			_calendarService = new CalendarService( eventFactory, cvarSystem, definition.Calendar );
			_seasonService = new SeasonService( eventFactory, _calendarService, definition.Calendar.Months, definition.Seasons );
			_weatherService = new WeatherService( eventFactory, _seasonService, _calendarService );

			_saveCoordinator = new SimulationSaveCoordinator( _calendarService, _weatherService, eventFactory );

			_calendarService.SetTime( startDate );
		}
	};
};
