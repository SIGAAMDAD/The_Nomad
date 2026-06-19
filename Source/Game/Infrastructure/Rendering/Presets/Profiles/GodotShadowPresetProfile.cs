/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Godot;

namespace Nomad.Game.Infrastructure.Rendering.Presets.Profiles
{
	internal sealed record GodotShadowPresetProfile
	{
		public int DirectionalShadowSize { get; init; } = 4096;
		public int DirectionalSoftShadowFilterQuality { get; init; } = 2;
		public bool DirectionalSoftShadow16Bits { get; init; } = true;

		public int PositionalSoftShadowFilterQuality { get; init; } = 2;
		public bool PositionalShadowAtlas16Bits { get; init; } = true;
		public int PositionalShadowAtlasSize { get; init; } = 4096;
		public int PositionalShadowAtlasQuadrant0Subdiv { get; init; } = 4;
		public int PositionalShadowAtlasQuadrant1Subdiv { get; init; } = 4;
		public int PositionalShadowAtlasQuadrant2Subdiv { get; init; } = 16;
		public int PositionalShadowAtlasQuadrant3Subdiv { get; init; } = 64;
	};
};
