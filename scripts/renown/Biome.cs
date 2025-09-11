/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
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
	
	===================================================================================
	*/
	/// <summary>
	/// contains information and relevant data for an in-game biome, including weather and
	/// player detection
	/// </summary>
	
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
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
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

			WeatherChangeTimer = new Timer() {
				Name = nameof( WeatherChangeTimer ),
				WaitTime = WeatherChangeInterval,
				Autostart = true
			};
			GameEventBus.ConnectSignal( WeatherChangeTimer, Timer.SignalName.Timeout, this, OnWeatherChangeTimerTimeout );
			AddChild( WeatherChangeTimer );

			Hide();
		}
	};
};
