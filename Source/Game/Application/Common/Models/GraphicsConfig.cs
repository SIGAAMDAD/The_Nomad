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

namespace Game.Application.Common.Models {
	public readonly record struct GraphicsConfig {
		public readonly ShadowFilterQuality ShadowFilterType { get; init; }
		public readonly int ShadowAtlasSize { get; init; }
		public readonly float ShadowFilterSmooth { get; init; }
		public readonly bool BakedLights { get; init; }
		public readonly bool PhysicallyBasedRendering { get; init; }
		public readonly bool BloomEnabled { get; init; }
		public readonly bool ForceVertexShading { get; init; }
		public readonly bool FootstepsEnabled { get; init; }
		public readonly QualitySetting Preset { get; init; }
		public readonly ParticleQuality ParticleQuality { get; init; }
		public readonly AnimationQuality AnimationQuality { get; init; }
	};
};