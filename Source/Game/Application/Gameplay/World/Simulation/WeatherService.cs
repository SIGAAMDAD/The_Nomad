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
using Nomad.Core.Util;
using Nomad.Game.Domain.Data.World;
using Nomad.Game.Domain.Events.World;
using Nomad.Game.Domain.Interfaces.World;

namespace Nomad.Game.Application.Gameplay.World
{
	/*
	===================================================================================
	
	WeatherService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class WeatherService : IWeather
	{
		private readonly CalendarService _calendarService;
		private readonly SeasonService _seasonService;

		private long _nextWeatherChangeAtAbsoluteMinute = 0;

		public InternString CurrentWeatherId { get; private set; } = new InternString( "weather.clear" );

		public IGameEvent<WeatherChangedEventArgs> WeatherChanged => _weatherChanged;
		private readonly IGameEvent<WeatherChangedEventArgs> _weatherChanged = default;

		/*
		===============
		WeatherService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="seasonService"></param>
		/// <param name="calendarService"></param>
		public WeatherService(
			IGameEventRegistryService eventFactory,
			SeasonService seasonService,
			CalendarService calendarService
		)
		{
			_seasonService = seasonService ?? throw new ArgumentNullException( nameof( seasonService ) );
			_calendarService = calendarService ?? throw new ArgumentNullException( nameof( calendarService ) );

			_weatherChanged = eventFactory.GetEvent<WeatherChangedEventArgs>(
				WeatherChangedEventArgs.Name,
				WeatherChangedEventArgs.NameSpace
			);

			eventFactory
				.GetEvent<MinuteChangedEventArgs>( MinuteChangedEventArgs.Name, MinuteChangedEventArgs.NameSpace )
				.Subscribe( OnMinuteChanged );

			eventFactory
				.GetEvent<SeasonChangedEventArgs>( SeasonChangedEventArgs.Name, SeasonChangedEventArgs.NameSpace )
				.Subscribe( OnSeasonChanged );

			CurrentSetup();
		}

		/*
		===============
		CurrentSetup
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void CurrentSetup()
		{
			CurrentWeatherId = RollWeather( _seasonService.CurrentSeason, null );
			ScheduleNextWeatherChange( _seasonService.CurrentSeason );
		}

		/*
		===============
		OnMinuteChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnMinuteChanged( in MinuteChangedEventArgs args )
		{
			if ( _calendarService.AbsoluteMinute < _nextWeatherChangeAtAbsoluteMinute ) {
				return;
			}

			SeasonDefinition season = _seasonService.CurrentSeason;
			InternString previous = CurrentWeatherId;
			CurrentWeatherId = RollWeather( season, previous );

			ScheduleNextWeatherChange( season );

			_weatherChanged.Publish( new WeatherChangedEventArgs(
				args.Time,
				previous,
				CurrentWeatherId
			) );
		}

		/*
		===============
		OnSeasonChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnSeasonChanged( in SeasonChangedEventArgs args )
		{
			// Optional:
			// you can either keep current weather until its timer ends,
			// or force a reroll immediately when the season changes.

			// I recommend: keep current weather, but reschedule the next roll
			// so the new season takes over naturally.
			ScheduleNextWeatherChange( args.Current );
		}

		/*
		===============
		ScheduleNextWeatherChange
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="season"></param>
		private void ScheduleNextWeatherChange( SeasonDefinition season )
		{
			int durationHours = RNJesus.IntRange(
				season.MinWeatherDurationHours,
				season.MaxWeatherDurationHours + 1
			);
			_nextWeatherChangeAtAbsoluteMinute = _calendarService.AbsoluteMinute + (durationHours * 60L);
		}

		/*
		===============
		RollWeather
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="season"></param>
		/// <param name="currentWeatherId"></param>
		/// <returns></returns>
		private static InternString RollWeather( SeasonDefinition season, InternString? currentWeatherId )
		{
			float totalWeight = 0.0f;
			for ( int i = 0; i < season.WeatherTable.Count; i++ ) {
				totalWeight += season.WeatherTable[i].Weight;
			}

			if ( totalWeight <= 0.0f ) {
				return new InternString( "weather.clear" );
			}

			float roll = RNJesus.NextFloat() * totalWeight;
			float accum = 0.0f;

			for ( int i = 0; i < season.WeatherTable.Count; i++ ) {
				accum += season.WeatherTable[i].Weight;
				if ( roll <= accum ) {
					return season.WeatherTable[i].Id;
				}
			}

			return season.WeatherTable[^1].Id;
		}
	};
};
