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

using Nomad.Core.Util;
using System;

namespace Game.Application.Configuration.Enums {
	public enum MaxFps : uint {
		MaxFps30,
		MaxFps48,
		MaxFps60,
		MaxFps90,
		MaxFps125,
		MaxFps172,
		MaxFps225,
		MaxFps333,
		MaxFpsUnlimited,

		Count,

		Default = MaxFps60
	};

	public static class MaxFpsExtensions {
		private const string MAX_FPS_30 = "30";
		private const string MAX_FPS_48 = "48";
		private const string MAX_FPS_60 = "60";
		private const string MAX_FPS_90 = "90";
		private const string MAX_FPS_125 = "125";
		private const string MAX_FPS_172 = "172";
		private const string MAX_FPS_225 = "225";
		private const string MAX_FPS_333 = "333";
		private const string MAX_FPS_UNLIMITED = "Unlimited";

		/*
		===============
		ToDisplayString
		===============
		*/
		public static InternString ToDisplayString( this MaxFps maxFps ) => maxFps switch {
			MaxFps.MaxFps30 => new( MAX_FPS_30 ),
			MaxFps.MaxFps48 => new( MAX_FPS_48 ),
			MaxFps.MaxFps60 => new( MAX_FPS_60 ),
			MaxFps.MaxFps90 => new( MAX_FPS_90 ),
			MaxFps.MaxFps125 => new( MAX_FPS_125 ),
			MaxFps.MaxFps172 => new( MAX_FPS_172 ),
			MaxFps.MaxFps225 => new( MAX_FPS_225 ),
			MaxFps.MaxFps333 => new( MAX_FPS_333 ),
			MaxFps.MaxFpsUnlimited => new( MAX_FPS_UNLIMITED ),
			_ => throw new ArgumentOutOfRangeException( nameof( maxFps ) )
		};

		/*
		===============
		ToInt
		===============
		*/
		public static int ToInt( this MaxFps maxFps ) => maxFps switch {
			MaxFps.MaxFps30 => 30,
			MaxFps.MaxFps48 => 48,
			MaxFps.MaxFps60 => 60,
			MaxFps.MaxFps90 => 90,
			MaxFps.MaxFps125 => 125,
			MaxFps.MaxFps172 => 172,
			MaxFps.MaxFps225 => 225,
			MaxFps.MaxFps333 => 333,
			MaxFps.MaxFpsUnlimited => 0,
			_ => throw new ArgumentOutOfRangeException( nameof( maxFps ) )
		};

		/*
		===============
		TryParse
		===============
		*/
		public static bool TryParse( InternString maxFpsString, out MaxFps maxFps ) {
			switch ( (string)maxFpsString ) {
				case MAX_FPS_30:
					maxFps = MaxFps.MaxFps30;
					break;
				case MAX_FPS_48:
					maxFps = MaxFps.MaxFps48;
					break;
				case MAX_FPS_60:
					maxFps = MaxFps.MaxFps60;
					break;
				case MAX_FPS_90:
					maxFps = MaxFps.MaxFps90;
					break;
				case MAX_FPS_125:
					maxFps = MaxFps.MaxFps125;
					break;
				case MAX_FPS_172:
					maxFps = MaxFps.MaxFps172;
					break;
				case MAX_FPS_225:
					maxFps = MaxFps.MaxFps225;
					break;
				case MAX_FPS_333:
					maxFps = MaxFps.MaxFps333;
					break;
				case MAX_FPS_UNLIMITED:
					maxFps = MaxFps.MaxFpsUnlimited;
					break;
				default:
					maxFps = MaxFps.Default;
					return false;
			}
			return true;
		}
	};
};