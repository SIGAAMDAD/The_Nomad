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
using Nomad.Core.CVars;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.CVars;
using Nomad.Events.Extensions;
using Nomad.Game.Domain.Data.World;
using Nomad.Game.Domain.Events.World;
using Nomad.Game.Domain.Interfaces.World;

namespace Nomad.Game.Application.Gameplay.World
{
	/*
	===================================================================================

	CalendarService

	World clock only.
	- Owns date/time progression
	- Publishes minute/hour/day/month/year changes
	- Does NOT own weather scheduling
	- Does NOT own season resolution
	- Does NOT own sunrise/sunset transitions

	===================================================================================
	*/
	internal sealed class CalendarService : ICalendarService
	{
		// 4 real minutes = 1 in-game hour
		private const int HOUR_TICRATE_MS = (60 * 1000) * 4;
		private const int MINUTE_TICRATE_MS = HOUR_TICRATE_MS / 60;

		public WorldTime Current => new WorldTime(
			_currentYear,
			_currentMonth,
			_currentDay,
			_currentHour,
			_currentMinute
		);

		/// <summary>
		/// Total number of elapsed in-game days since the calendar epoch.
		/// Useful for season/weather systems that want absolute world time.
		/// </summary>
		public long AbsoluteDay => _absoluteDay;

		/// <summary>
		/// Total number of elapsed in-game minutes since the calendar epoch.
		/// Useful for one-shot scheduled weather changes.
		/// </summary>
		public long AbsoluteMinute => _absoluteMinute;

		private int _currentYear = 0;
		private int _currentMonth = 0;
		private int _currentDay = 0;
		private int _currentHour = 0;
		private int _currentMinute = 0;

		private long _absoluteDay = 0;
		private long _absoluteMinute = 0;

		private readonly CalendarDefinition _calendarDefinition = default;

		public IGameEvent<YearChangedEventArgs> YearChanged => _yearChanged;
		private readonly IGameEvent<YearChangedEventArgs> _yearChanged = default;

		public IGameEvent<MonthChangedEventArgs> MonthChanged => _monthChanged;
		private readonly IGameEvent<MonthChangedEventArgs> _monthChanged = default;

		public IGameEvent<DayChangedEventArgs> DayChanged => _dayChanged;
		private readonly IGameEvent<DayChangedEventArgs> _dayChanged = default;

		public IGameEvent<HourChangedEventArgs> HourChanged => _hourChanged;
		private readonly IGameEvent<HourChangedEventArgs> _hourChanged = default;

		public IGameEvent<MinuteChangedEventArgs> MinuteChanged => _minuteChanged;
		private readonly IGameEvent<MinuteChangedEventArgs> _minuteChanged = default;

		/*
		===============
		CalendarService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <param name="cvarSystem"></param>
		/// <param name="calendarDefinition"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public CalendarService( IGameEventRegistryService eventFactory, ICVarSystemService cvarSystem, CalendarDefinition calendarDefinition )
		{
			ArgumentGuard.ThrowIfNull( eventFactory );
			ArgumentGuard.ThrowIfNull( cvarSystem );

			_calendarDefinition = calendarDefinition ?? throw new ArgumentNullException( nameof( calendarDefinition ) );

			_yearChanged = eventFactory.GetEvent<YearChangedEventArgs>(
				YearChangedEventArgs.Name,
				YearChangedEventArgs.NameSpace
			);

			_monthChanged = eventFactory.GetEvent<MonthChangedEventArgs>(
				MonthChangedEventArgs.Name,
				MonthChangedEventArgs.NameSpace
			);

			_dayChanged = eventFactory.GetEvent<DayChangedEventArgs>(
				DayChangedEventArgs.Name,
				DayChangedEventArgs.NameSpace
			);

			_hourChanged = eventFactory.GetEvent<HourChangedEventArgs>(
				HourChangedEventArgs.Name,
				HourChangedEventArgs.NameSpace
			);

			_minuteChanged = eventFactory
				.GetEvent<MinuteChangedEventArgs>(
					MinuteChangedEventArgs.Name,
					MinuteChangedEventArgs.NameSpace
				)
				.PublishEvery( PublishMinuteChanged, MINUTE_TICRATE_MS );
		}

		public void Dispose()
		{
			_yearChanged?.Dispose();
			_monthChanged?.Dispose();
			_dayChanged?.Dispose();
			_hourChanged?.Dispose();
			_minuteChanged?.Dispose();
		}

		/*
		========
		SetTime
		========
		*/
		/// <summary>
		/// Hard-sets the current calendar time.
		/// Useful for loading saves / debug commands.
		/// Does not publish change events.
		/// </summary>
		/// <param name="time"></param>
		public void SetTime( WorldTime time )
		{
			_currentYear = time.Year;
			_currentMonth = time.Month;
			_currentDay = time.Day;
			_currentHour = time.Hour;
			_currentMinute = time.Minute;

			RecomputeAbsoluteTime();
		}

		/*
		===================
		PublishMinuteChanged
		===================
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private MinuteChangedEventArgs PublishMinuteChanged()
		{
			AdvanceOneMinute(
				out bool hourChanged,
				out bool dayChanged,
				out bool monthChanged,
				out bool yearChanged
			);

			WorldTime current = Current;

			//
			// Publish from largest unit to smallest so subscribers always see
			// the fully finalized timestamp for the new tick.
			//
			if ( yearChanged ) {
				_yearChanged.Publish( new YearChangedEventArgs( current ) );
			}
			if ( monthChanged ) {
				_monthChanged.Publish( new MonthChangedEventArgs( _calendarDefinition.Months[_currentMonth].Id, current ) );
			}
			if ( dayChanged ) {
				_dayChanged.Publish( new DayChangedEventArgs( current ) );
			}
			if ( hourChanged ) {
				_hourChanged.Publish( new HourChangedEventArgs( current ) );
			}
			return new MinuteChangedEventArgs( current );
		}

		/*
		================
		AdvanceOneMinute
		================
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="hourChanged"></param>
		/// <param name="dayChanged"></param>
		/// <param name="monthChanged"></param>
		/// <param name="yearChanged"></param>
		private void AdvanceOneMinute(
			out bool hourChanged,
			out bool dayChanged,
			out bool monthChanged,
			out bool yearChanged
		)
		{
			hourChanged = false;
			dayChanged = false;
			monthChanged = false;
			yearChanged = false;

			_currentMinute++;
			_absoluteMinute++;

			if ( _currentMinute < 60 ) {
				return;
			}

			_currentMinute = 0;
			_currentHour++;
			hourChanged = true;

			if ( _currentHour < 24 ) {
				return;
			}

			_currentHour = 0;
			_currentDay++;
			_absoluteDay++;
			dayChanged = true;

			if ( _currentDay < GetDaysInMonth( _currentYear, _currentMonth ) ) {
				return;
			}

			_currentDay = 0;
			_currentMonth++;
			monthChanged = true;

			if ( _currentMonth < GetMonthCountForYear( _currentYear ) ) {
				return;
			}

			_currentMonth = 0;
			_currentYear++;
			yearChanged = true;
		}

		/*
		==============
		GetDaysInMonth
		==============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <returns></returns>
		private int GetDaysInMonth( int year, int month )
		{
			ArgumentOutOfRangeException.ThrowIfNegative( year );
			ArgumentOutOfRangeException.ThrowIfNegative( month );

			if ( month >= _calendarDefinition.Months.Count ) {
				throw new ArgumentOutOfRangeException(
					nameof( month ),
					month,
					"Month index exceeds calendar definition."
				);
			}

			return _calendarDefinition.Months[month].DayCount;
		}

		/*
		====================
		GetMonthCountForYear
		====================
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="year"></param>
		/// <returns></returns>
		private int GetMonthCountForYear( int year )
		{
			ArgumentOutOfRangeException.ThrowIfNegative( year );
			return _calendarDefinition.Months.Count;
		}

		/*
		=====================
		RecomputeAbsoluteTime
		=====================
		*/
		/// <summary>
		/// Rebuilds absolute day/minute counters from the current calendar fields.
		/// Only needed when externally setting the time.
		/// </summary>
		private void RecomputeAbsoluteTime()
		{
			long totalDays = 0;

			for ( int year = 0; year < _currentYear; year++ ) {
				totalDays += GetMonthCountForYear( year ) * _calendarDefinition.Months[0].DayCount; // same-length months for now
			}
			for ( int month = 0; month < _currentMonth; month++ ) {
				totalDays += GetDaysInMonth( _currentYear, month );
			}

			totalDays += _currentDay;

			_absoluteDay = totalDays;
			_absoluteMinute = (totalDays * 24L * 60L)
				+ (_currentHour * 60L)
				+ _currentMinute;
		}
	};
};
