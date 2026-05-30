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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nomad.Game.Sdk.Mods
{
    public sealed record ModuleManifest
    {
        [JsonIgnore]
        public string DirectoryPath { get; set; } = string.Empty;

        public ModCapabilities Capabilities { get; init; }

        public string Id { get; init; }
        public string Name { get; init; }
        public string Version { get; init; }
        public string ApiVersion { get; init; }
        public string Author { get; init; }

        public string? Pck { get; init; }
        public int LoadPriority { get; init; } = 0;
        public bool Official { get; init; } = false;

        public string Assembly { get; init; }
        public string EntryType { get; init; }

        public List<ModuleDependency> Dependencies { get; init; } = new();
        public List<string> Incompatibilities { get; init; } = new();
    }
}
