/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using System.Collections.Generic;

namespace Renown.World {
	public enum WeatherType : uint {
		Clear,
		Humid,
		Snowing,
		British,
		Raining,
		ThunderStorm,
		Windy,
		Blazing,
		
		Count
	};

	/*
	===================================================================================
	
	Biome
	
	contains information and relevant data for an in-game biome, including weather and
	player detection
	
	===================================================================================
	*/
	
	public partial class Biome : WorldArea {
		/// <summary>
		/// The current weather of the biome
		/// </summary>
		[Export]
		private WeatherType CurrentWeather = WeatherType.Clear;

		/// <summary>
		/// Chance of biome clear & sunny weather
		/// </summary>
		[Export]
		private float WeatherChanceClear = 0.0f;

		/// <summary>
		/// Chance of biome hot & humid weather
		/// </summary>
		[Export]
		private float WeatherChanceHumid = 0.0f;

		/// <summary>
		/// Chance of biome snow
		/// </summary>
		[Export]
		private float WeatherChanceSnowing = 0.0f;

		/// <summary>
		/// Chance of biome british weather
		/// </summary>
		[Export]
		private float WeatherChanceBritish = 0.0f;

		/// <summary>
		/// Chance of biome rain
		/// </summary>
		[Export]
		private float WeatherChanceRaining = 0.0f;

		/// <summary>
		/// Chance of biome thunderstorm
		/// </summary>
		[Export]
		private float WeatherChanceThunderStorm = 0.0f;

		/// <summary>
		/// Chance of biome wind
		/// </summary>
		[Export]
		private float WeatherChanceWindy = 0.0f;

		/// <summary>
		/// Chance of biome heatstroke weather
		/// </summary>
		[Export]
		private float WeatherChanceBlazing = 0.0f;

		/// <summary>
		/// How often weather changes are calculated in this biome
		/// </summmary>
		[Export]
		private float WeatherChangeInterval = 0.0f;

		private Timer WeatherChangeTimer;
		private Dictionary<WeatherType, float> WeatherChances;

		private float CheckDelta = 0.0f;

		[Signal]
		public delegate void AgentEnteredAreaEventHandler( Entity agent );
		[Signal]
		public delegate void AgentExitedAreaEventHandler( Entity agent );

		/*
		===============
		OnWeatherChangeTimerTimeout
		===============
		*/
		private void OnWeatherChangeTimerTimeout() {
			float chance = 0.0f;
			WeatherType weather = WeatherType.Clear;

			// TODO: have the current season have an impact on the weather
			for ( int i = 0; i < WeatherChances.Count; i++ ) {
				float other = WeatherChances[ weather ];
				if ( other > chance ) {
					chance = other;
					weather = (WeatherType)i;
				}
			}
			CurrentWeather = weather;
		}

		/*
		===============
		_Ready

		godot initialization override
		===============
		*/
		public override void _Ready() {
			base._Ready();

			WeatherChances = new Dictionary<WeatherType, float>{
				{ WeatherType.Clear, WeatherChanceClear },
				{ WeatherType.Humid, WeatherChanceHumid },
				{ WeatherType.Snowing, WeatherChanceSnowing },
				{ WeatherType.British, WeatherChanceBritish },
				{ WeatherType.Raining, WeatherChanceRaining },
				{ WeatherType.ThunderStorm, WeatherChanceThunderStorm },
				{ WeatherType.Windy, WeatherChanceWindy },
				{ WeatherType.Blazing, WeatherChanceBlazing },
			};

			/*
			WeatherChangeTimer = new Timer();
			WeatherChangeTimer.Name = "WeatherChangeTimer";
			WeatherChangeTimer.WaitTime = WeatherChangeInterval;
			WeatherChangeTimer.Connect( "timeout", Callable.From( OnWeatherChangeTimerTimeout ) );
			AddChild( WeatherChangeTimer );
			*/

			Hide();
		}

		/*
		===============
		_Process
		===============
		*/
		public override void _Process( double delta ) {
			base._Process( delta );
		}
	};
};
