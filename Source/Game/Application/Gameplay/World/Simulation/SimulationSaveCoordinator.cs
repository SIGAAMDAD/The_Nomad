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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Save.Services;

namespace Nomad.Game.Application.Gameplay.World
{
	/*
	===================================================================================

	SimulationSaveCoordinator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class SimulationSaveCoordinator
	{
		private readonly CalendarService _calendarService;
		private readonly WeatherService _weatherService;

		private readonly object _lock = new();

		/*
		===============
		SimulationSaveCoordinator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="calendarService"></param>
		/// <param name="weatherService"></param>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public SimulationSaveCoordinator( CalendarService calendarService, WeatherService weatherService, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_calendarService = calendarService ?? throw new ArgumentNullException( nameof( calendarService ) );
			_weatherService = weatherService ?? throw new ArgumentNullException( nameof( weatherService ) );

			eventFactory
				.GetEvent<SaveBeginEventArgs>( SaveBeginEventArgs.Name, SaveBeginEventArgs.NameSpace )
				.Subscribe( OnSaveBegin );
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
			lock ( _lock ) {
				var writer = args.Writer.AddSection( "WorldData" );

				//
				// write the calendar data
				//
				var current = _calendarService.Current;
				writer.AddField( "Time.Year", current.Year );
				writer.AddField( "Time.Month", current.Month );
				writer.AddField( "Time.Day", current.Day );
				writer.AddField( "Time.Hour", current.Hour );
				writer.AddField( "Time.Minute", current.Minute );

				//
				// write the weather data
				//
				writer.AddField( "WeatherId", (string)_weatherService.CurrentWeatherId );

			}
		}
	};
};
