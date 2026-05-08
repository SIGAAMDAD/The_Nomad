/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is" ), without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Core.CVars;
using Nomad.Core.Events;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Core.Util;
using Nomad.Game.Application.Gameplay.World;
using Nomad.Game.Domain.Data.World;
using System.Collections.Generic;
using Godot;

namespace Nomad.Game.Prefabs {
	/*
	===================================================================================
	
	SingleWorld
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class SingleWorld : WorldBase {
		private readonly SimulationCoordinator _coordinator;

		private readonly CalendarDefinition _calender;
		private readonly WorldDefinition _worldDefinition;
		private readonly Dictionary<InternString, SeasonDefinition> _seasons;

		public static readonly SeasonDefinition EarlyLongSummer = new() {
			Id = new( "season.early_longsummer.id" ),

			DaylightHours = 13.75f,
			TwilightHours = 0.80f,

			MinWeatherDurationHours = 5,
			MaxWeatherDurationHours = 9,

			WeatherTable = new[] {
				new WeatherWeight( new( "weather.clear" ),        38.0f ),
				new WeatherWeight( new( "weather.dry_heat" ),     22.0f ),
				new WeatherWeight( new( "weather.dust_wind" ),    16.0f ),
				new WeatherWeight( new( "weather.sandstorm" ),     9.0f ),
				new WeatherWeight( new( "weather.heat_haze" ),    10.0f ),
				new WeatherWeight( new( "weather.ashfall_light" ), 3.0f ),
				new WeatherWeight( new( "weather.rain" ),          2.0f )
			},

			NightAmbient = new Color( 0.16f, 0.12f, 0.19f, 1.0f ),
			DawnAmbient = new Color( 0.63f, 0.41f, 0.29f, 1.0f ),
			DayAmbient = new Color( 0.95f, 0.88f, 0.76f, 1.0f ),
			DuskAmbient = new Color( 0.72f, 0.37f, 0.25f, 1.0f )
		};

		public static readonly SeasonDefinition LateLongSummer = new() {
			Id = new( "season.late_longsummer.id" ),

			DaylightHours = 14.50f,
			TwilightHours = 0.70f,

			MinWeatherDurationHours = 4,
			MaxWeatherDurationHours = 8,

			WeatherTable = new[] {
				new WeatherWeight( new( "weather.clear" ),        34.0f ),
				new WeatherWeight( new( "weather.dry_heat" ),     27.0f ),
				new WeatherWeight( new( "weather.dust_wind" ),    16.0f ),
				new WeatherWeight( new( "weather.sandstorm" ),    12.0f ),
				new WeatherWeight( new( "weather.heat_haze" ),     8.0f ),
				new WeatherWeight( new( "weather.ashfall_light" ), 2.0f ),
				new WeatherWeight( new( "weather.rain" ),          1.0f )
			},

			NightAmbient = new Color( 0.15f, 0.11f, 0.18f, 1.0f ),
			DawnAmbient = new Color( 0.68f, 0.43f, 0.27f, 1.0f ),
			DayAmbient = new Color( 0.99f, 0.91f, 0.75f, 1.0f ),
			DuskAmbient = new Color( 0.76f, 0.33f, 0.23f, 1.0f )
		};

		public static readonly SeasonDefinition EarlyLongWinter = new() {
			Id = new( "season.early_longwinter.id" ),

			DaylightHours = 10.25f,
			TwilightHours = 0.95f,

			MinWeatherDurationHours = 6,
			MaxWeatherDurationHours = 11,

			WeatherTable = new[] {
				new WeatherWeight( new( "weather.clear_cold" ),    30.0f ),
				new WeatherWeight( new( "weather.cold_wind" ),     22.0f ),
				new WeatherWeight( new( "weather.overcast" ),      18.0f ),
				new WeatherWeight( new( "weather.fog" ),           12.0f ),
				new WeatherWeight( new( "weather.frost" ),         10.0f ),
				new WeatherWeight( new( "weather.freezing_rain" ),  5.0f ),
				new WeatherWeight( new( "weather.sandstorm_cold" ), 3.0f )
			},

			NightAmbient = new Color( 0.08f, 0.10f, 0.15f, 1.0f ),
			DawnAmbient = new Color( 0.36f, 0.40f, 0.49f, 1.0f ),
			DayAmbient = new Color( 0.73f, 0.78f, 0.85f, 1.0f ),
			DuskAmbient = new Color( 0.28f, 0.32f, 0.41f, 1.0f )
		};

		public static readonly SeasonDefinition LateLongWinter = new() {
			Id = new( "season.late_longwinter.id" ),

			DaylightHours = 9.00f,
			TwilightHours = 1.05f,

			MinWeatherDurationHours = 7,
			MaxWeatherDurationHours = 12,

			WeatherTable = new[] {
				new WeatherWeight( new( "weather.clear_cold" ),    25.0f ),
				new WeatherWeight( new( "weather.cold_wind" ),     25.0f ),
				new WeatherWeight( new( "weather.overcast" ),      20.0f ),
				new WeatherWeight( new( "weather.fog" ),           14.0f ),
				new WeatherWeight( new( "weather.frost" ),         10.0f ),
				new WeatherWeight( new( "weather.freezing_rain" ),  4.0f ),
				new WeatherWeight( new( "weather.sandstorm_cold" ), 2.0f )
			},

			NightAmbient = new Color( 0.07f, 0.09f, 0.14f, 1.0f ),
			DawnAmbient = new Color( 0.33f, 0.37f, 0.47f, 1.0f ),
			DayAmbient = new Color( 0.69f, 0.75f, 0.83f, 1.0f ),
			DuskAmbient = new Color( 0.24f, 0.29f, 0.39f, 1.0f )
		};

		public SingleWorld() {
			_worldDefinition = new WorldDefinition {
				Name = new InternString( "Bellatum Terrae" ),
				Calendar = new CalendarDefinition {
					Months = new MonthDefinition[] {
						new MonthDefinition {
							Id = new InternString( "month.early_longsummer.id" ),
							DisplayName = new InternString( "month.early_longsummer.displayname" ),
							DayCount = 69,
							SeasonId = new InternString( "season.early_longsummer.id" ),
							TransitionSeasonId = new InternString( "season.late_longsummer.id" )
						},
						new MonthDefinition {
							Id = new InternString( "month.late_longsummer.id" ),
							DisplayName = new InternString( "month.late_longsummer.displayname" ),
							DayCount = 89,
							SeasonId = new InternString( "season.late_longsummer.id" ),
							TransitionSeasonId = new InternString( "season.early_longwinter.id" ),
							TransitionStartNormalized = 0.5f
						},
						new MonthDefinition {
							Id = new InternString( "month.early_longwinter.id" ),
							DisplayName = new InternString( "month.early_longwinter.displayname" ),
							DayCount = 69,
							SeasonId = new InternString( "season.early_longwinter.id" ),
							TransitionSeasonId = new InternString( "season.late_longwinter.id" )
						},
						new MonthDefinition {
							Id = new InternString( "month.late_longwinter.id" ),
							DisplayName = new InternString( "month.late_longwinter.displayname" ),
							DayCount = 89,
							SeasonId = new InternString( "season.late_longwinter.id" ),
							TransitionSeasonId = new InternString( "season.early_longsummer.id" ),
							TransitionStartNormalized = 0.5f
						},
					}
				},
				Seasons = new Dictionary<InternString, SeasonDefinition> {
					[ new( "season.early_longsummer.id" ) ] = EarlyLongSummer,
					[ new( "season.late_longsummer.id" ) ] = LateLongSummer,
					[ new( "season.early_longwinter.id" ) ] = EarlyLongWinter,
					[ new( "season.late_longwinter.id" ) ] = LateLongWinter,
				}
			};

			var serviceLocator = ServiceLocator.Instance;
			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();
			var cvarSystem = serviceLocator.GetService<ICVarSystemService>();

			var startTime = new WorldTime(
				year: 897852,
				month: 0,
				day: 23,
				hour: 12,
				minute: 0
			);

			_coordinator = new SimulationCoordinator( eventFactory, cvarSystem, startTime, _worldDefinition );
		}
	};
};