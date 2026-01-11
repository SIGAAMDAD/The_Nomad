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

namespace Game.Infrastructure.Configuration.Godot.ValueObjects {
	internal readonly record struct WindowModeData(
		DisplayServer.WindowMode Mode,
		bool Borderless,
		bool Exclusive
	) : IValueObject<WindowModeData> {
		public static implicit operator WindowModeData( WindowMode value ) => value switch {
			WindowMode.Windowed => new WindowModeData { Mode = DisplayServer.WindowMode.Windowed, Borderless = false, Exclusive = false },
			WindowMode.BorderlessWindowed => new WindowModeData { Mode = DisplayServer.WindowMode.Windowed, Borderless = true, Exclusive = false },
//			WindowMode.Fullscreen => new WindowModeData { Mode = DisplayServer.WindowMode.Fullscreen, Borderless = false, Exclusive = false },
//			WindowMode.BorderlessFullscreen => new WindowModeData { Mode = DisplayServer.WindowMode.Fullscreen, Borderless = true, Exclusive = false },
			WindowMode.ExclusiveFullscreen => new WindowModeData { Mode = DisplayServer.WindowMode.ExclusiveFullscreen, Borderless = true, Exclusive = true },
			_ => throw new ArgumentOutOfRangeException( nameof( value ) )
		};
	};
};