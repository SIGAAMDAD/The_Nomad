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

namespace Nomad.Game.Domain.Events.World {
	/// <summary>
	/// 
	/// </summary>
	public static class EventNames {
		/// <summary>
		/// 
		/// </summary>
		public const string NAMESPACE = "Nomad.Game.Domain.Events.World";

		/// <summary>
		/// 
		/// </summary>
		public const string YEAR_CHANGED = NAMESPACE + ".YearChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string MONTH_CHANGED = NAMESPACE + ".MonthChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string DAY_CHANGED = NAMESPACE + ".DayChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string HOUR_CHANGED = NAMESPACE + ".HourChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string MINUTE_CHANGED = NAMESPACE + ".MinuteChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string DAYTIME_START = NAMESPACE + ".DayTimeStart";

		/// <summary>
		/// 
		/// </summary>
		public const string NIGHTTIME_START = NAMESPACE + ".NightTimeStart";

		/// <summary>
		/// 
		/// </summary>
		public const string WEATHER_CHANGED = NAMESPACE + ".WeatherChanged";

		/// <summary>
		/// 
		/// </summary>
		public const string SEASON_CHANGED = NAMESPACE + ".SeasonChanged";
	};
};