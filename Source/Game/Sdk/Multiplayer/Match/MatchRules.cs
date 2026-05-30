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

namespace Nomad.Game.Sdk.Multiplayer.Match
{
    public sealed record MatchRules
    {
        public MultiplayerMode Mode { get; set; }
        public string MapId { get; set; } = string.Empty;

        public byte MaxPlayers { get; set; } = 8;
        public byte MaxTeams { get; set; } = 0;

        public byte ScoreLimit { get; set; } = 20;
        public byte RoundLimit { get; set; } = 1;

        public uint MatchDurationTicks { get; set; } = 0;
        public uint RoundDurationTicks { get; set; } = 0;
        public uint WarmupTicks { get; set; } = 0;
        public uint CountdownTicks { get; set; } = 180;
        public uint PostMatchTicks { get; set; } = 300;

        public bool FriendlyFire { get; set; }
        public bool AllowRespawns { get; set; } = true;
        public bool AllowSpectators { get; set; } = true;
        public bool AllowLateJoin { get; set; }
        public bool LockLoadoutsOnStart { get; set; } = true;
        public bool Ranked { get; set; }
    }
}
