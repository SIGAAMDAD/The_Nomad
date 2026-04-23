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

namespace Nomad.Game.Domain.Data.Mods {
	public sealed record ModuleManifest {
		public string Id { get; init; }
	    public string Version { get; init; }
	    public string EntryAssembly { get; init; }
	    public string EntryType { get; init; }
	    public string[] Dependencies { get; init; }
	    public string[] OptionalDependencies { get; init; }
	    public string[] Replaces { get; init; }
	    public string[] Capabilities { get; init; }
	    public string[] LoadAfter { get; init; }
	};
};