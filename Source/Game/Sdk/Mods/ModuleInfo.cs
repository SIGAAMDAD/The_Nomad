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

namespace Nomad.Game.Sdk.Mods
{
	public sealed record ModuleInfo
	{
		public string Id { get; init; } = string.Empty;
		public string Name { get; init; } = string.Empty;
		public string Version { get; init; } = string.Empty;
		public string ApiVersion { get; init; } = string.Empty;
		public string Author { get; init; } = string.Empty;
		public bool Official { get; init; }
		public int LoadPriority { get; init; }
		public ModCapabilities Capabilities { get; init; }

		public static ModuleInfo FromManifest( ModuleManifest manifest )
		{
			return new ModuleInfo {
				Id = manifest.Id,
				Name = manifest.Name,
				Version = manifest.Version,
				ApiVersion = manifest.ApiVersion,
				Author = manifest.Author,
				Official = manifest.Official,
				LoadPriority = manifest.LoadPriority,
				Capabilities = manifest.Capabilities
			};
		}
	}
}
