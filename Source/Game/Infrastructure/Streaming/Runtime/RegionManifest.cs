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
	internal readonly struct RegionManifest
	{
		public readonly bool IsDefined;
		public readonly RegionId Id;
		public readonly string ScenePath;
		public readonly string ProxyScenePath;
		public readonly string LightScenePath;
		public readonly string TraversalDatabasePath;
		public readonly Aabb LocalBounds;
		public readonly int EstimatedCPUBytes;
		public readonly int EstimatedGPUBytes;
		public readonly bool EnableHotLighting;
		public readonly bool EnableHotAudio;

		public bool HasScene => IsDefined && !string.IsNullOrEmpty( ScenePath );
		public bool HasProxyScene => IsDefined && !string.IsNullOrEmpty( ProxyScenePath );
		public bool HasLightScene => IsDefined && !string.IsNullOrEmpty( LightScenePath );
		public bool HasTraversalDatabase => IsDefined && !string.IsNullOrEmpty( TraversalDatabasePath );

		public RegionManifest(
			RegionId id,
			string scenePath,
			string proxyScenePath = "",
			string lightScenePath = "",
			string traversalDatabasePath = "",
			Aabb localBounds = default,
			int estimatedCPUBytes = 0,
			int estimatedGPUBytes = 0,
			bool enableHotLighting = true,
			bool enableHotAudio = true
		)
		{
			IsDefined = true;
			Id = id;
			ScenePath = scenePath ?? string.Empty;
			ProxyScenePath = proxyScenePath ?? string.Empty;
			LightScenePath = lightScenePath ?? string.Empty;
			TraversalDatabasePath = traversalDatabasePath ?? string.Empty;
			LocalBounds = localBounds;
			EstimatedCPUBytes = estimatedCPUBytes;
			EstimatedGPUBytes = estimatedGPUBytes;
			EnableHotLighting = enableHotLighting;
			EnableHotAudio = enableHotAudio;
		}
	};
};
