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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Numerics;
using Nomad.Core.Util;
using Nomad.Game.Sdk.World;
using Nomad.Game.Sdk.Events.World;

namespace Nomad.Game.Application.Gameplay.World
{
	/*
	===================================================================================

	SeasonService

	===================================================================================
	*/
	/// <summary>
	/// Month-authoritative season resolver.
	/// The month defines the season, and optionally blends toward another season as the
	/// month progresses.
	/// </summary>

	internal sealed class SeasonService : ISeasonService
	{
		public SeasonDefinition CurrentSeason { get; private set; }

		private readonly CalendarService _calendar;
		private readonly IReadOnlyDictionary<SeasonDefinitionId, SeasonDefinition> _seasons;
		private readonly IReadOnlyList<MonthDefinition> _months;

		public IGameEvent<SeasonChangedEventArgs> SeasonChanged => _seasonChanged;
		private readonly IGameEvent<SeasonChangedEventArgs> _seasonChanged = default;

		/*
		=============
		SeasonService
		=============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="calendar"></param>
		/// <param name="months"></param>
		/// <param name="seasons"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public SeasonService(
			IGameEventRegistryService eventFactory,
			CalendarService calendar,
			IReadOnlyList<MonthDefinition> months,
			IReadOnlyDictionary<SeasonDefinitionId, SeasonDefinition> seasons
		)
		{
			ArgumentGuard.ThrowIfNull( eventFactory );

			_calendar = calendar ?? throw new ArgumentNullException( nameof( calendar ) );
			_months = months ?? throw new ArgumentNullException( nameof( months ) );
			_seasons = seasons ?? throw new ArgumentNullException( nameof( seasons ) );

			_seasonChanged = eventFactory
				.GetEvent<SeasonChangedEventArgs>(
					SeasonChangedEventArgs.Name,
					SeasonChangedEventArgs.NameSpace
				);

			CurrentSeason = ResolveSeason( calendar.Current );

			//
			// DayChanged is enough here because:
			// 1) a month rollover also triggers a day rollover
			// 2) transition alpha only changes as the month progresses
			//
			calendar.DayChanged.Subscribe( OnDayChanged );
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
			_seasonChanged.Dispose();
		}

		public bool TryGetSeason( SeasonDefinitionId id, out SeasonDefinition definition )
		{
			return _seasons.TryGetValue( id, out definition );
		}

		/*
		============
		OnDayChanged
		============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnDayChanged( in DayChangedEventArgs args )
		{
			SeasonDefinition next = ResolveSeason( args.Time );

			if ( next.Equals( CurrentSeason ) ) {
				return;
			}

			SeasonDefinition previous = CurrentSeason;
			CurrentSeason = next;

			_seasonChanged.Publish( new SeasonChangedEventArgs(
				args.Time,
				previous.Id,
				next.Id
			) );
		}

		/*
		=============
		ResolveSeason
		=============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		private SeasonDefinition ResolveSeason( in WorldTime time )
		{
			if ( time.Month < 0 || time.Month >= _months.Count ) {
				throw new ArgumentOutOfRangeException(
					nameof( time ),
					$"Month index '{time.Month}' is outside the month definition table."
				);
			}

			MonthDefinition month = _months[time.Month];

			if ( !_seasons.TryGetValue( month.SeasonId, out SeasonDefinition primary ) ) {
				throw new KeyNotFoundException(
					$"Primary season '{month.SeasonId}' was not found in the season table."
				);
			}

			if ( !_seasons.TryGetValue( month.TransitionSeasonId, out SeasonDefinition secondary ) ) {
				throw new KeyNotFoundException(
					$"Transition season '{month.TransitionSeasonId}' was not found in the season table."
				);
			}

			float dayProgress01 = GetMonthProgress01( time.Day, month.DayCount );

			if ( dayProgress01 <= month.TransitionStartNormalized ) {
				return primary;
			}

			float alpha = (dayProgress01 - month.TransitionStartNormalized) / (1.0f - month.TransitionStartNormalized);
			alpha = Smooth01( Math.Clamp( alpha, 0.0f, 1.0f ) );

			return LerpSeason( primary, secondary, alpha );
		}

		/*
		==================
		GetMonthProgress01
		==================
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="day"></param>
		/// <param name="daysInMonth"></param>
		/// <returns></returns>
		private static float GetMonthProgress01( int day, int daysInMonth )
		{
			if ( daysInMonth <= 1 ) {
				return 1.0f;
			}
			return Math.Clamp( (float)day / (daysInMonth - 1), 0.0f, 1.0f );
		}

		/*
		==========
		LerpSeason
		==========
		*/
		/// <summary>
		/// Blends two prototype SeasonDefinitions together.
		///
		/// NOTE:
		/// If your SeasonDefinition grows more fields later, update this method.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		private static SeasonDefinition LerpSeason( SeasonDefinition a, SeasonDefinition b, float t )
		{
			t = Math.Clamp( t, 0.0f, 1.0f );

			return new SeasonDefinition {
				Id = t < 0.5f ? a.Id : b.Id,

				DaylightHours = Interpolation.Lerp( a.DaylightHours, b.DaylightHours, t ),
				TwilightHours = Interpolation.Lerp( a.TwilightHours, b.TwilightHours, t ),

				MinWeatherDurationHours = (int)Math.Round(
					Interpolation.Lerp( a.MinWeatherDurationHours, b.MinWeatherDurationHours, t )
				),

				MaxWeatherDurationHours = (int)Math.Round(
					Interpolation.Lerp( a.MaxWeatherDurationHours, b.MaxWeatherDurationHours, t )
				),

				//
				// Prototype behavior:
				// keep the dominant weather table based on whichever season is currently closer.
				// Later, if you want, this can be replaced with a merged weighted table.
				//
				WeatherTable = t < 0.5f ? a.WeatherTable : b.WeatherTable,

				NightAmbient = a.NightAmbient.Lerp( b.NightAmbient, t ),
				DawnAmbient = a.DawnAmbient.Lerp( b.DawnAmbient, t ),
				DayAmbient = a.DayAmbient.Lerp( b.DayAmbient, t ),
				DuskAmbient = a.DuskAmbient.Lerp( b.DuskAmbient, t )
			};
		}

		/*
		========
		Smooth01
		========
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		private static float Smooth01( float t )
		{
			t = Math.Clamp( t, 0.0f, 1.0f );
			return t * t * (3.0f - (2.0f * t));
		}
	}
}
