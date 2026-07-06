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
using Nomad.Game.Sdk.World;
using Nomad.Game.Sdk.Events.World;

namespace Nomad.Game.Gameplay.World.Simulation
{
	/*
	===================================================================================

	WeatherService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class WeatherService : IWeatherService, IDisposable
	{
		private readonly ICalendarService _calendarService;
		private readonly SeasonService _seasonService;

		private long _nextWeatherChangeAtAbsoluteMinute = 0;

		public WeatherType CurrentWeather { get; private set; }

		private readonly IDisposable _minuteChanged;
		private readonly IDisposable _seasonChanged;

		private bool _isDisposed = false;

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
		/// <exception cref="ArgumentNullException"></exception>
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

			_minuteChanged = eventFactory
				.GetEvent<MinuteChangedEventArgs>(
					MinuteChangedEventArgs.Name,
					MinuteChangedEventArgs.NameSpace
				)
				.Subscribe( OnMinuteChanged );

			_seasonChanged = eventFactory
				.GetEvent<SeasonChangedEventArgs>(
					SeasonChangedEventArgs.Name,
					SeasonChangedEventArgs.NameSpace
				)
				.Subscribe( OnSeasonChanged );

			CurrentSetup();
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

			_minuteChanged?.Dispose();
			_seasonChanged?.Dispose();
			_weatherChanged?.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public bool TrySetWeather( WeatherType weatherType )
		{
			return true;
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
			CurrentWeather = RollWeather( _seasonService.CurrentSeason, null );
			ScheduleNextWeatherChange( _seasonService.CurrentSeason.Id );
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
			WeatherType previous = CurrentWeather;
			CurrentWeather = RollWeather( season, previous );

			ScheduleNextWeatherChange( season.Id );

			_weatherChanged.Publish(
				new WeatherChangedEventArgs(
					args.Time,
					previous,
					CurrentWeather
				)
			);
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
		/// <param name="seasonId"></param>
		private void ScheduleNextWeatherChange( SeasonDefinitionId seasonId )
		{
			if ( !_seasonService.TryGetSeason( seasonId, out var season ) ) {
				return;
			}

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
		private static WeatherType RollWeather( SeasonDefinition season, WeatherType? currentWeather )
		{
			float totalWeight = 0.0f;
			for ( int i = 0; i < season.WeatherTable.Count; i++ ) {
				totalWeight += season.WeatherTable[i].Weight;
			}

			if ( totalWeight <= 0.0f ) {
				return WeatherType.Clear;
			}

			float roll = RNJesus.NextFloat() * totalWeight;
			float accum = 0.0f;

			for ( int i = 0; i < season.WeatherTable.Count; i++ ) {
				accum += season.WeatherTable[i].Weight;
				if ( roll <= accum ) {
					return season.WeatherTable[i].Type;
				}
			}

			return season.WeatherTable[^1].Type;
		}
	};
};
