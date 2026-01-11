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

namespace Game.Application.Configuration.Enums {
	/// <summary>
	/// Anti aliasing mode.
	/// </summary>
	public enum AntiAliasing : uint {
		None,
		FXAA,
		TAA,
		SMAA,
		MSAA_2x,
		MSAA_4x,
		MSAA_8x,

		Count,

		Default = FXAA
	};

	public enum AntiAliasingBasic : uint {
		None = AntiAliasing.None,
		EdgeAA = AntiAliasing.FXAA,
		ScreenSpace = AntiAliasing.MSAA_4x,

		Count = 3,

		Default = EdgeAA
	};

	public static class AntiAliasingExtensions {
		/*
		===============
		AsString
		===============
		*/
		/// <summary>
		/// Converts a <see cref="AntiAliasingBasic"/> enum to a human-readable string.
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static string AsString( this AntiAliasingBasic mode ) => mode switch {
			AntiAliasingBasic.None => "None",
			AntiAliasingBasic.EdgeAA => "Edge AA",
			AntiAliasingBasic.ScreenSpace => "ScreenSpace",
			_ => throw new ArgumentOutOfRangeException( nameof( mode ) )
		};

		/*
		===============
		AsString
		===============
		*/
		/// <summary>
		/// Converts a <see cref="AntiAliasing"/> enum to a human-readable string.
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static string AsString( this AntiAliasing mode ) => mode switch {
			AntiAliasing.None => "None",
			AntiAliasing.FXAA => "FXAA",
			AntiAliasing.TAA => "TAA",
			AntiAliasing.SMAA => "SMAA",
			AntiAliasing.MSAA_2x => "MSAA 2x",
			AntiAliasing.MSAA_4x => "MSAA 4x",
			AntiAliasing.MSAA_8x => "MSAA 8x",
			_ => throw new ArgumentOutOfRangeException( nameof( mode ) )
		};
	};
};