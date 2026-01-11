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

using System;
using System.Runtime.CompilerServices;

namespace Game.Application.Configuration.Enums {
	/// <summary>
	/// 
	/// </summary>
	public enum WindowMode : uint {
		Windowed,
		BorderlessWindowed,
		Fullscreen,
		BorderlessFullscreen,
		ExclusiveFullscreen,

		Count,

		Default = ExclusiveFullscreen
	};
	
	public enum WindowModeBasic : uint {
		Windowed,
		BorderlessWindowed,
		Fullscreen,

		Count,

		Default = Fullscreen
	};

	public static class WindowModeExtensions {
		private const string WINDOW_MODE_WINDOWED = "Windowed";
		private const string WINDOW_MODE_BORDERLESS_WINDOWED = "Borderless Windowed";
		private const string WINDOW_MODE_FULLSCREEN = "Fullscreen";
		private const string WINDOW_MODE_BORDERLESS_FULLSCREEN = "Borderless Fullscreen";
		private const string WINDOW_MODE_EXCLUSIVE_FULLSCREEN = "Fullscreen";

		/*
		===============
		AsString
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static string AsString( this WindowModeBasic mode ) => mode switch {
			WindowModeBasic.Windowed => WINDOW_MODE_WINDOWED,
			WindowModeBasic.BorderlessWindowed => WINDOW_MODE_BORDERLESS_WINDOWED,
			WindowModeBasic.Fullscreen => WINDOW_MODE_FULLSCREEN,
			_ => throw new ArgumentOutOfRangeException( nameof( mode  ) )
		};

		/*
		===============
		AsRealMode
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static WindowMode AsRealMode( this WindowModeBasic mode ) => mode switch {
			WindowModeBasic.Windowed => WindowMode.Windowed,
			WindowModeBasic.BorderlessWindowed => WindowMode.BorderlessWindowed,
			WindowModeBasic.Fullscreen => WindowMode.ExclusiveFullscreen,
			_ => throw new ArgumentOutOfRangeException( nameof( mode ) )
		};

		/*
		===============
		AsString
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static string AsString( this WindowMode mode ) => mode switch {
			WindowMode.Windowed => WINDOW_MODE_WINDOWED,
			WindowMode.BorderlessWindowed => WINDOW_MODE_BORDERLESS_WINDOWED,
			WindowMode.Fullscreen => WINDOW_MODE_FULLSCREEN,
			WindowMode.BorderlessFullscreen => WINDOW_MODE_BORDERLESS_FULLSCREEN,
			WindowMode.ExclusiveFullscreen => WINDOW_MODE_EXCLUSIVE_FULLSCREEN,
			_ => throw new ArgumentOutOfRangeException( nameof( mode ) )
		};

		/*
		===============
		AsUIMode
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static WindowModeBasic AsUIMode( this WindowMode mode ) => mode switch {
			WindowMode.Windowed => WindowModeBasic.Windowed,
			WindowMode.BorderlessWindowed => WindowModeBasic.BorderlessWindowed,
			WindowMode.BorderlessFullscreen or WindowMode.Fullscreen or WindowMode.ExclusiveFullscreen => WindowModeBasic.Fullscreen,
			_ => throw new ArgumentOutOfRangeException( nameof( mode ) )
		};
	};
};