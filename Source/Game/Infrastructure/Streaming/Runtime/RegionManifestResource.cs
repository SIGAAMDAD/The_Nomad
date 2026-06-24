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

namespace Nomad.Game.Infrastructure.Streaming
{
	[GlobalClass]
	internal sealed partial class RegionManifestResource : Resource
	{
		[Export]
		public int X { get; set; }

		[Export]
		public int Z { get; set; }

		[Export]
		public string ScenePath { get; set; } = string.Empty;

		[Export]
		public string ProxyScenePath { get; set; } = string.Empty;

		[Export]
		public string LightScenePath { get; set; } = string.Empty;

		[Export]
		public string TraversalDatabasePath { get; set; } = string.Empty;

		[Export]
		public Aabb LocalBounds { get; set; }

		[Export]
		public int EstimatedCPUBytes { get; set; }

		[Export]
		public int EstimatedGPUBytes { get; set; }

		[Export]
		public bool EnableHotLighting { get; set; } = true;

		[Export]
		public bool EnableHotAudio { get; set; } = true;

		public RegionManifest ToManifest()
		{
			return new RegionManifest(
				new RegionId( X, Z ),
				ScenePath,
				ProxyScenePath,
				LightScenePath,
				TraversalDatabasePath,
				LocalBounds,
				EstimatedCPUBytes,
				EstimatedGPUBytes,
				EnableHotLighting,
				EnableHotAudio
			);
		}
	};
};
