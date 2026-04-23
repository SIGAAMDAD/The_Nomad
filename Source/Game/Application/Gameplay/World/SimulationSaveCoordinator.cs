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
using Nomad.Save.Data;
using Nomad.Save.Events;

namespace Nomad.Game.Application.Gameplay.World {
	/*
	===================================================================================
	
	SimulationSaveCoordinator
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class SimulationSaveCoordinator {
		private readonly CalendarService _calendarService;
		private readonly WeatherService _weatherService;

		private readonly object _lock = new();

		public SimulationSaveCoordinator( CalendarService calendarService, WeatherService weatherService, IGameEventRegistryService eventFactory ) {
			_calendarService = calendarService ?? throw new ArgumentNullException( nameof( calendarService ) );
			_weatherService = weatherService ?? throw new ArgumentNullException( nameof( weatherService ) );

			eventFactory
				.GetEvent<SaveBeginEventArgs>( EventNames.SAVE_BEGIN_EVENT, EventNames.NAMESPACE )
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
		private void OnSaveBegin( in SaveBeginEventArgs args ) {
			lock ( _lock ) {
				var writer = args.Writer.AddSection( "WorldData" );

				var current = _calendarService.Current;
				writer.AddField( "Time.Year", current.Year );
				writer.AddField( "Time.Month", current.Month );
				writer.AddField( "Time.Day", current.Day );
				writer.AddField( "Time.Hour", current.Hour );
				writer.AddField( "Time.Minute", current.Minute );

				writer.AddField( "WeatherId", (string)_weatherService.CurrentWeatherId );
			}
		}
	};
};