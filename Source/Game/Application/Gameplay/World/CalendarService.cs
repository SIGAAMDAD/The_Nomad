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

using Nomad.Core.Events;
using Nomad.Game.Domain.Events.World;

namespace Nomad.Game.Application.Gameplay.World {
	/*
	===================================================================================
	
	CalendarService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class CalendarService {
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
		public CalendarService( IGameEventRegistryService eventFactory ) {
			_yearChanged = eventFactory.GetEvent<YearChangedEventArgs>( EventNames.YEAR_CHANGED, EventNames.NAMESPACE );
			_monthChanged = eventFactory.GetEvent<MonthChangedEventArgs>( EventNames.MONTH_CHANGED, EventNames.NAMESPACE );
			_dayChanged = eventFactory.GetEvent<DayChangedEventArgs>( EventNames.DAY_CHANGED, EventNames.NAMESPACE );
			_hourChanged = eventFactory.GetEvent<HourChangedEventArgs>( EventNames.HOUR_CHANGED, EventNames.NAMESPACE );
			_minuteChanged = eventFactory.GetEvent<MinuteChangedEventArgs>( EventNames.MINUTE_CHANGED, EventNames.NAMESPACE );
		}
	};
};