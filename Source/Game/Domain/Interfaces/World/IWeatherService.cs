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

namespace Nomad.Game.Domain.Interfaces.World
{
	public interface IWeatherService : IDisposable
	{
		[Event( nameSpace: "Nomad.Game.Domain.Events.World" )]
		[EventPayload( "Time", typeof( WorldTime ), Order = 1 )]
		[EventPayload( "Previous", typeof( InternString ), Order = 2 )]
		[EventPayload( "Current", typeof( InternString ), Order = 3 )]
		IGameEvent<WeatherChangedEventArgs> WeatherChanged { get; }

		InternString CurrentWeatherId { get; }
	};
};
