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

using Nomad.Game.Domain.Interfaces.Mods;
using Nomad.Game.Infrastructure.Mods;
using System.Reflection;

namespace Nomad.Game.Domain.Data.Mods {
	public sealed record LoadedModule {
		public string Directory { get; init; }
		public ModuleManifest Manifest { get; init; }
		public ModuleLoadContext LoadContext { get; init; }
		public Assembly Assembly { get; init; }
		public IGameModule Instance { get; init; }

		public ModuleState State { get; set; }
	};
};