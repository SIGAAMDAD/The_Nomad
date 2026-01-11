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
	public enum AspectRatio : uint {
		Aspect_Automatic,
		Aspect_4_3,
		Aspect_16_10,
		Aspect_16_9,
		Aspect_21_9,

		Count,

		Default = Aspect_Automatic
	};

	public static class AspectRatioExtensions {
		private const string ASPECT_RATIO_AUTOMATIC = "Automatic";
		private const string ASPECT_RATIO_4_3 = "4:3";
		private const string ASPECT_RATIO_16_10 = "16:10";
		private const string ASPECT_RATIO_16_9 = "16:9";
		private const string ASPECT_RATIO_21_9 = "21:9";

		/*
		===============
		ToDisplayString
		===============
		*/
		public static InternString ToDisplayString( this AspectRatio aspectRatio ) => aspectRatio switch {
			AspectRatio.Aspect_Automatic => new( ASPECT_RATIO_AUTOMATIC ),
			AspectRatio.Aspect_4_3 => new( ASPECT_RATIO_4_3 ),
			AspectRatio.Aspect_16_10 => new( ASPECT_RATIO_16_10 ),
			AspectRatio.Aspect_16_9 => new( ASPECT_RATIO_16_9 ),
			AspectRatio.Aspect_21_9 => new( ASPECT_RATIO_21_9 ),
			_ => throw new ArgumentOutOfRangeException( nameof( aspectRatio ) )
		};
		
		/*
		===============
		TryParse
		===============
		*/
		public static bool TryParse( InternString aspectRatioString, out AspectRatio aspectRatio ) {
			switch ( (string)aspectRatioString ) {
				case ASPECT_RATIO_AUTOMATIC:
					aspectRatio = AspectRatio.Aspect_Automatic;
					break;
				case ASPECT_RATIO_4_3:
					aspectRatio = AspectRatio.Aspect_4_3;
					break;
				case ASPECT_RATIO_16_10:
					aspectRatio = AspectRatio.Aspect_16_10;
					break;
				case ASPECT_RATIO_16_9:
					aspectRatio = AspectRatio.Aspect_16_9;
					break;
				case ASPECT_RATIO_21_9:
					aspectRatio = AspectRatio.Aspect_21_9;
					break;
				default:
					aspectRatio = AspectRatio.Default;
					return false;
			}
			return true;
		}
	};
};