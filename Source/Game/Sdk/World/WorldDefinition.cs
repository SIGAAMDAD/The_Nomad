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

using System;
using System.Collections.Generic;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Sdk.World
{
    /// <summary>
    ///
    /// </summary>
    public sealed record WorldDefinition
    {
        public InternString Name { get; init; }

        public WorldTime StartTime { get; init; }

        public CalendarDefinition Calendar { get; init; }

        public IReadOnlyList<SeasonDefinition> Seasons { get; init; }
            = Array.Empty<SeasonDefinition>();

        public IReadOnlyList<Nomad.Game.Sdk.Biomes.BiomeDefinition> Biomes { get; init; }
            = Array.Empty<Nomad.Game.Sdk.Biomes.BiomeDefinition>();
    }
}
