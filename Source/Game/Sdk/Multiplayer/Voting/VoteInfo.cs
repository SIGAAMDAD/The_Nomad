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
using Nomad.Core.OnlineServices;

namespace Nomad.Game.Sdk.Multiplayer.Voting
{
    public sealed record VoteInfo
    {
        public VoteId Id { get; init; }
        public VoteKind Kind { get; init; }
        public PeerId StartedBy { get; init; }
        public PeerId TargetPeer { get; init; }
        public int RequiredVotes { get; init; }
        public int EligibleVoters { get; init; }
        public int VotesCast { get; init; }
        public uint Version { get; init; }
        public DateTime StartedUtc { get; init; }
        public DateTime EndsUtc { get; init; }
    }
}
