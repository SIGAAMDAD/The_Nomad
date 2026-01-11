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

using Game.Application.Common.Models.ValueObjects;
using Nomad.Core.Util;
using System;

namespace Game.Application.Configuration.Enums {
	public enum WindowResolution : uint {
		Res_640x480,
		Res_800x600,
		Res_1024x768,
		Res_1280x720,
		Res_1280x768,
		Res_1280x800,
		Res_1280x1024,
		Res_1360x768,
		Res_1366x768,
		Res_1440x900,
		Res_1536x864,
		Res_1600x900,
		Res_1600x1200,
		Res_1680x1050,
		Res_1920x1080,
		Res_1920x1200,
		Res_2048x1152,
		Res_2048x1536,
		Res_2560x1080,
		Res_2560x1440,
		Res_2560x1600,
		Res_3440x1440,
		Res_3840x2160,

		Res_Native,

		Count,

		Min = Res_640x480,
		Max = Res_3840x2160,

		Default = Res_1920x1080
	};

	public static class WindowResolutionExtensions {
		private const string WINDOW_SIZE_640X480 = "640x480";
		private const string WINDOW_SIZE_800X600 = "800x600";
		private const string WINDOW_SIZE_1024X768 = "1024x768";
		private const string WINDOW_SIZE_1280X720 = "1280x720";
		private const string WINDOW_SIZE_1280X768 = "1280x768";
		private const string WINDOW_SIZE_1280X800 = "1280x800";
		private const string WINDOW_SIZE_1280X1024 = "1280x1024";
		private const string WINDOW_SIZE_1360X768 = "1360x768";
		private const string WINDOW_SIZE_1366X768 = "1366x768";
		private const string WINDOW_SIZE_1440X900 = "1440x900";
		private const string WINDOW_SIZE_1536X864 = "1536x864";
		private const string WINDOW_SIZE_1600X900 = "1600x900";
		private const string WINDOW_SIZE_1600X1200 = "1600x1200";
		private const string WINDOW_SIZE_1680X1050 = "1680x1050";
		private const string WINDOW_SIZE_1920X1080 = "1920x1080";
		private const string WINDOW_SIZE_1920X1200 = "1920x1200";
		private const string WINDOW_SIZE_2048X1152 = "2048x1152";
		private const string WINDOW_SIZE_2048X1536 = "2048x1536";
		private const string WINDOW_SIZE_2560X1080 = "2560x1080";
		private const string WINDOW_SIZE_2560X1440 = "2560x1440";
		private const string WINDOW_SIZE_2560X1600 = "2560x1600";
		private const string WINDOW_SIZE_3440X1440 = "3440x1440";
		private const string WINDOW_SIZE_3840X2160 = "3840x2160";
		private const string WINDOW_SIZE_NATIVE = "Native Resolution";

		/*
		===============
		ToDisplayString
		===============
		*/
		public static InternString ToDisplayString( this WindowResolution resolution ) => resolution switch {
			WindowResolution.Res_640x480 => new( WINDOW_SIZE_640X480 ),
			WindowResolution.Res_800x600 => new( WINDOW_SIZE_800X600 ),
			WindowResolution.Res_1024x768 => new( WINDOW_SIZE_1024X768 ),
			WindowResolution.Res_1280x720 => new( WINDOW_SIZE_1280X720 ),
			WindowResolution.Res_1280x768 => new( WINDOW_SIZE_1280X768 ),
			WindowResolution.Res_1280x800 => new( WINDOW_SIZE_1280X800 ),
			WindowResolution.Res_1280x1024 => new( WINDOW_SIZE_1280X1024 ),
			WindowResolution.Res_1360x768 => new( WINDOW_SIZE_1360X768 ),
			WindowResolution.Res_1366x768 => new( WINDOW_SIZE_1366X768 ),
			WindowResolution.Res_1440x900 => new( WINDOW_SIZE_1440X900 ),
			WindowResolution.Res_1536x864 => new( WINDOW_SIZE_1536X864 ),
			WindowResolution.Res_1600x900 => new( WINDOW_SIZE_1600X900 ),
			WindowResolution.Res_1600x1200 => new( WINDOW_SIZE_1600X1200 ),
			WindowResolution.Res_1680x1050 => new( WINDOW_SIZE_1680X1050 ),
			WindowResolution.Res_1920x1080 => new( WINDOW_SIZE_1920X1080 ),
			WindowResolution.Res_1920x1200 => new( WINDOW_SIZE_1920X1200 ),
			WindowResolution.Res_2048x1152 => new( WINDOW_SIZE_2048X1152 ),
			WindowResolution.Res_2048x1536 => new( WINDOW_SIZE_2048X1536 ),
			WindowResolution.Res_2560x1080 => new( WINDOW_SIZE_2560X1080 ),
			WindowResolution.Res_2560x1440 => new( WINDOW_SIZE_2560X1440 ),
			WindowResolution.Res_2560x1600 => new( WINDOW_SIZE_2560X1600 ),
			WindowResolution.Res_3440x1440 => new( WINDOW_SIZE_3440X1440 ),
			WindowResolution.Res_3840x2160 => new( WINDOW_SIZE_3840X2160 ),
			WindowResolution.Res_Native => new( WINDOW_SIZE_NATIVE ),
			_ => throw new ArgumentOutOfRangeException( nameof( resolution ) )
		};
		
		/*
		===============
		GetSize
		===============
		*/
		public static WindowSize GetSize( this WindowResolution resolution ) => resolution switch {
			WindowResolution.Res_640x480 => new WindowSize { Width = 640, Height = 480 },
			WindowResolution.Res_800x600 => new WindowSize { Width = 800, Height = 800 },
			WindowResolution.Res_1024x768 => new WindowSize { Width = 1024, Height = 768 },
			WindowResolution.Res_1280x720 => new WindowSize { Width = 1280, Height = 720 },
			WindowResolution.Res_1280x768 => new WindowSize { Width = 1280, Height = 768 },
			WindowResolution.Res_1280x800 => new WindowSize { Width = 1280, Height = 800 },
			WindowResolution.Res_1280x1024 => new WindowSize { Width = 1280, Height = 1024 },
			WindowResolution.Res_1360x768 => new WindowSize { Width = 1360, Height = 768 },
			WindowResolution.Res_1366x768 => new WindowSize { Width = 1366, Height = 768 },
			WindowResolution.Res_1440x900 => new WindowSize { Width = 1440, Height = 900 },
			WindowResolution.Res_1536x864 => new WindowSize { Width = 1536, Height = 864 },
			WindowResolution.Res_1600x900 => new WindowSize { Width = 1600, Height = 900 },
			WindowResolution.Res_1600x1200 => new WindowSize { Width = 1600, Height = 1200 },
			WindowResolution.Res_1680x1050 => new WindowSize { Width = 1680, Height = 1050 },
			WindowResolution.Res_1920x1080 => new WindowSize { Width = 1920, Height = 1080 },
			WindowResolution.Res_1920x1200 => new WindowSize { Width = 1920, Height = 1200 },
			WindowResolution.Res_2048x1152 => new WindowSize { Width = 2048, Height = 1152 },
			WindowResolution.Res_2048x1536 => new WindowSize { Width = 2048, Height = 1536 },
			WindowResolution.Res_2560x1080 => new WindowSize { Width = 2560, Height = 1080 },
			WindowResolution.Res_2560x1440 => new WindowSize { Width = 2560, Height = 1440 },
			WindowResolution.Res_2560x1600 => new WindowSize { Width = 2560, Height = 1600 },
			WindowResolution.Res_3440x1440 => new WindowSize { Width = 3440, Height = 1440 },
			WindowResolution.Res_3840x2160 => new WindowSize { Width = 3840, Height = 2160 },
			_ => throw new ArgumentOutOfRangeException( nameof( resolution ) )
		};
		
		/*
		===============
		TryParse
		===============
		*/
		public static bool TryParse( InternString resolutionString, out WindowResolution resolution ) {
			switch ( (string)resolutionString ) {
				case WINDOW_SIZE_640X480:
					resolution = WindowResolution.Res_640x480;
					break;
				case WINDOW_SIZE_800X600:
					resolution = WindowResolution.Res_800x600;
					break;
				case WINDOW_SIZE_1024X768:
					resolution = WindowResolution.Res_1024x768;
					break;
				case WINDOW_SIZE_1280X720:
					resolution = WindowResolution.Res_1280x720;
					break;
				case WINDOW_SIZE_1280X768:
					resolution = WindowResolution.Res_1280x768;
					break;
				case WINDOW_SIZE_1280X800:
					resolution = WindowResolution.Res_1280x800;
					break;
				case WINDOW_SIZE_1280X1024:
					resolution = WindowResolution.Res_1280x1024;
					break;
				case WINDOW_SIZE_1360X768:
					resolution = WindowResolution.Res_1360x768;
					break;
				case WINDOW_SIZE_1366X768:
					resolution = WindowResolution.Res_1366x768;
					break;
				case WINDOW_SIZE_1440X900:
					resolution = WindowResolution.Res_1440x900;
					break;
				case WINDOW_SIZE_1536X864:
					resolution = WindowResolution.Res_1536x864;
					break;
				case WINDOW_SIZE_1600X900:
					resolution = WindowResolution.Res_1600x900;
					break;
				case WINDOW_SIZE_1600X1200:
					resolution = WindowResolution.Res_1600x1200;
					break;
				case WINDOW_SIZE_1680X1050:
					resolution = WindowResolution.Res_1680x1050;
					break;
				case WINDOW_SIZE_1920X1080:
					resolution = WindowResolution.Res_1920x1080;
					break;
				case WINDOW_SIZE_1920X1200:
					resolution = WindowResolution.Res_1920x1200;
					break;
				case WINDOW_SIZE_2048X1152:
					resolution = WindowResolution.Res_2048x1152;
					break;
				case WINDOW_SIZE_2048X1536:
					resolution = WindowResolution.Res_2048x1536;
					break;
				case WINDOW_SIZE_2560X1080:
					resolution = WindowResolution.Res_2560x1080;
					break;
				case WINDOW_SIZE_2560X1440:
					resolution = WindowResolution.Res_2560x1440;
					break;
				case WINDOW_SIZE_2560X1600:
					resolution = WindowResolution.Res_2560x1600;
					break;
				case WINDOW_SIZE_3440X1440:
					resolution = WindowResolution.Res_3440x1440;
					break;
				case WINDOW_SIZE_3840X2160:
					resolution = WindowResolution.Res_3840x2160;
					break;
				case WINDOW_SIZE_NATIVE:
					resolution = WindowResolution.Res_Native;
					break;
				default:
					resolution = WindowResolution.Default;
					return false;
			}
			return true;
		}
	};
};