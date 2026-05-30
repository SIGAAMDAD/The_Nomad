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

using Nomad.Core.Util;

namespace Nomad.Game.Sdk.World
{
    /// <summary>
    ///
    /// </summary>
    public readonly struct MonthDefinition
    {
        public InternString Id { get; init; }
        public InternString DisplayName { get; init; }
        public SeasonDefinitionId SeasonId { get; init; }
        public SeasonDefinitionId TransitionSeasonId { get; init; }
        public float TransitionStartNormalized { get; init; }
        public int DayCount { get; init; }
    }
}
