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
	/*
	===================================================================================
	
	VSyncData
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal readonly record struct VSyncData(
		DisplayServer.VSyncMode Mode,
		int SwapChainImageCount
	) : IValueObject<VSyncData> {
		public static implicit operator VSyncData( VSyncMode mode ) => mode switch {
			VSyncMode.Off => new VSyncData { Mode = DisplayServer.VSyncMode.Disabled, SwapChainImageCount = 2 },
			VSyncMode.On => new VSyncData { Mode = DisplayServer.VSyncMode.Enabled, SwapChainImageCount = 2 },
			VSyncMode.Adaptive => new VSyncData { Mode = DisplayServer.VSyncMode.Adaptive, SwapChainImageCount = 2 },
			VSyncMode.TripleBuffered => new VSyncData { Mode = DisplayServer.VSyncMode.Adaptive, SwapChainImageCount = 3 },
			_ => throw new ArgumentOutOfRangeException( nameof( mode ) )
		};
	};
};