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
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Multiplayer.Team;

namespace Nomad.Game.Sdk.Multiplayer.Match
{
    public sealed record MatchResult
    {
        public MultiplayerMode Mode { get; init; }
        public string MapId { get; init; }

        public DateTimeOffset StartedAtUtc { get; init; }
        public DateTimeOffset EndedAtUtc { get; init; }

        public TeamId WinningTeamId { get; init; }
        public PeerId? WinningPlayer { get; init; }

        public IReadOnlyList<MatchPlayerStats> Players { get; init; }
    }
}
