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

namespace Nomad.Game.Sdk.Multiplayer.Profile
{
    public sealed record ObjectiveStatsRecord
    {
        public uint ObjectivesCompleted { get; init; }
        public uint ObjectivesDenied { get; init; }

        public uint FlagsCaptured { get; init; }
        public uint FlagsReturned { get; init; }

        public uint ExtractionCompletions { get; init; }
        public uint ExtractionDenials { get; init; }

        public uint VipKills { get; init; }
        public uint VipSaves { get; init; }

        public uint BountiesClaimed { get; init; }
        public uint BountyEscapes { get; init; }

        public uint HoldLineSeconds { get; init; }
        public uint AlliesRevived { get; init; }
    }
}
