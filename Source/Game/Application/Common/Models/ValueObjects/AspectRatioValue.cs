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
using Nomad.Core.Abstractions;
using System;

namespace Game.Application.Common.Models.ValueObjects {
	internal readonly record struct AspectRatioValue(
		float Ratio
	) : IValueObject<AspectRatioValue> {
		public static implicit operator AspectRatioValue( AspectRatio value ) => value switch {
			AspectRatio.Aspect_Automatic => new( 1.0f ),
			AspectRatio.Aspect_4_3 => new( 4.0f / 3.0f ),
			AspectRatio.Aspect_16_10 => new( 16.0f / 10.0f ),
			AspectRatio.Aspect_16_9 => new( 16.0f / 9.0f ),
			AspectRatio.Aspect_21_9 => new( 21.0f / 9.0f ),
			_ => throw new ArgumentOutOfRangeException( nameof( value ) )
		};
	};
};