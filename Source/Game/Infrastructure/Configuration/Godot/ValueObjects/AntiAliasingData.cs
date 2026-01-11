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
	internal readonly record struct AntiAliasingData(
		RenderingServer.ViewportMsaa Msaa,
		RenderingServer.ViewportScreenSpaceAA ScreenSpaceAA,
		bool UseTaa
	) : IValueObject<AntiAliasingData> {
		public static implicit operator AntiAliasingData( AntiAliasing mode ) => mode switch {
			AntiAliasing.None => new AntiAliasingData { Msaa = RenderingServer.ViewportMsaa.Disabled, ScreenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled, UseTaa = false },
			AntiAliasing.FXAA => new AntiAliasingData { Msaa = RenderingServer.ViewportMsaa.Disabled, ScreenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Fxaa, UseTaa = false },
			AntiAliasing.MSAA_2x => new AntiAliasingData { Msaa = RenderingServer.ViewportMsaa.Msaa2X, ScreenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled, UseTaa = false },
			AntiAliasing.MSAA_4x => new AntiAliasingData { Msaa = RenderingServer.ViewportMsaa.Msaa4X, ScreenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled, UseTaa = false },
			AntiAliasing.MSAA_8x => new AntiAliasingData { Msaa = RenderingServer.ViewportMsaa.Msaa8X, ScreenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled, UseTaa = false },
			AntiAliasing.SMAA => new AntiAliasingData { Msaa = RenderingServer.ViewportMsaa.Disabled, ScreenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Smaa, UseTaa = false },
			_ => throw new ArgumentOutOfRangeException( nameof( mode ) )
		};
	};
};