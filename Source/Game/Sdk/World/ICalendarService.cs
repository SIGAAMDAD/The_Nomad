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
using Nomad.Game.Sdk.Events.World;

namespace Nomad.Game.Sdk.World
{
    /// <summary>
    ///
    /// </summary>
    public interface ICalendarService : IDisposable
    {
        [Event(nameSpace: "Nomad.Game.Sdk.Events.World")]
        [EventPayload("Time", typeof(WorldTime))]
        IGameEvent<MinuteChangedEventArgs> MinuteChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.World")]
        [EventPayload("Time", typeof(WorldTime))]
        IGameEvent<HourChangedEventArgs> HourChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.World")]
        [EventPayload("Time", typeof(WorldTime))]
        IGameEvent<DayChangedEventArgs> DayChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.World")]
        [EventPayload("Current", typeof(InternString), Order = 1)]
        [EventPayload("Time", typeof(WorldTime), Order = 2)]
        IGameEvent<MonthChangedEventArgs> MonthChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.World")]
        [EventPayload("Time", typeof(WorldTime))]
        IGameEvent<YearChangedEventArgs> YearChanged { get; }

        WorldTime Current { get; }

        long AbsoluteDay { get; }
        long AbsoluteMinute { get; }

        void SetTime(WorldTime time);
    }
}
