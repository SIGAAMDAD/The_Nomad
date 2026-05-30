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

using System.Text.Json;

namespace Nomad.Game.Sdk.Mods
{
    public sealed record ModFileSystemPolicy
    {
        public long MaxReadBytes { get; init; } = 8 * 1024 * 1024;
        public long MaxWriteBytes { get; init; } = 8 * 1024 * 1024;
        public int MaxEnumeratedFiles { get; init; } = 4096;

        public bool AllowRecursiveEnumeration { get; init; } = true;
        public bool CreateWriteDirectories { get; init; } = true;

        public JsonSerializerOptions JsonOptions { get; init; } = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
    }
}
