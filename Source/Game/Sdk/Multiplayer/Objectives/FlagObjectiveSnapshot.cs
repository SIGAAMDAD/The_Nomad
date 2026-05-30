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

using System.Numerics;
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Multiplayer.Team;

namespace Nomad.Game.Sdk.Multiplayer.Objectives
{
    public readonly struct FlagObjectiveSnapshot
    {
        public TeamId TeamId { get; init; }
        public FlagStatus Status { get; init; }

        public PeerId Carrier { get; init; }

        public Vector2 HomePosition { get; init; }
        public Vector2 CurrentPosition { get; init; }
        public Vector2 DroppedPosition { get; init; }

        public uint Version { get; init; }
    }
}
