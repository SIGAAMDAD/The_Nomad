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

using Game.Application.Configuration.Enums;
using Godot;
using Nomad.Core.Abstractions;
using System;
using System.Runtime.CompilerServices;

namespace Game.Application.Common.Models.ValueObjects {
	/// <summary>
	/// Represents the size of a game window.
	/// </summary>
	/// <param name="Width"></param>
	/// <param name="Height"></param>
	public readonly record struct WindowSize(
		int Width, int Height
	) : IValueObject<WindowSize> {
		/*
		===============
		operator WindowSize
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator WindowSize( WindowResolution value ) => value switch {
			WindowResolution.Res_640x480 => new WindowSize { Width = 640, Height = 480 },
			WindowResolution.Res_800x600 => new WindowSize { Width = 800, Height = 600 },
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
			_ => throw new ArgumentOutOfRangeException( nameof( value ) )
		};

		/*
		===============
		operator Vector2I
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static implicit operator Vector2I( WindowSize size ) => new Vector2I( size.Width, size.Height );

		/*
		===============
		operator WindowSize
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static implicit operator WindowSize( Vector2I size ) => new WindowSize( size.X, size.Y );

		/*
		===============
		operator WindowResolution
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="size"></param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static implicit operator WindowResolution( WindowSize size ) {
			switch ( size.Width ) {
				case 640:
					return WindowResolution.Res_640x480;
				case 800:
					return WindowResolution.Res_800x600;
				case 1024:
					return WindowResolution.Res_1024x768;
				case 1360:
					return WindowResolution.Res_1360x768;
				case 1366:
					return WindowResolution.Res_1366x768;
				case 1440:
					return WindowResolution.Res_1440x900;
				case 1536:
					return WindowResolution.Res_1536x864;
				case 1680:
					return WindowResolution.Res_1680x1050;
				case 3440:
					return WindowResolution.Res_3440x1440;
				case 3840:
					return WindowResolution.Res_3840x2160;
				case 1280:
					switch ( size.Height ) {
						case 720:
							return WindowResolution.Res_1280x720;
						case 768:
							return WindowResolution.Res_1280x768;
						case 800:
							return WindowResolution.Res_1280x800;
						case 1024:
							return WindowResolution.Res_1280x1024;
					}
					break;
				case 2560:
					switch ( size.Height ) {
						case 1080:
							return WindowResolution.Res_2560x1080;
						case 1440:
							return WindowResolution.Res_2560x1440;
						case 1600:
							return WindowResolution.Res_2560x1600;
					}
					break;
				case 2048:
					switch ( size.Height ) {
						case 1152:
							return WindowResolution.Res_2048x1152;
						case 1536:
							return WindowResolution.Res_2048x1536;
					}
					break;
				case 1920:
					switch ( size.Height ) {
						case 1080:
							return WindowResolution.Res_1920x1080;
						case 1200:
							return WindowResolution.Res_1920x1200;
					}
					break;
				case 1600:
					switch ( size.Height ) {
						case 900:
							return WindowResolution.Res_1600x900;
						case 1200:
							return WindowResolution.Res_1600x1200;
					}
					break;
			}
			return WindowResolution.Default;
		}

		public static bool operator <( WindowSize left, WindowSize right ) => left.Width < right.Width && left.Height < right.Height;
		public static bool operator >( WindowSize left, WindowSize right ) => left.Width > right.Width && left.Height > right.Height;
		public static bool operator <=( WindowSize left, WindowSize right ) => left.Width <= right.Width && left.Height <= right.Height;
		public static bool operator >=( WindowSize left, WindowSize right ) => left.Width >= right.Width && left.Height >= right.Height;
	};
};