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

namespace Nomad.Game.Sdk.Entities
{
    [Flags]
    public enum StatusEffectFlags : uint
    {
        None = 0,

        Harmful = 1 << 0,
        Beneficial = 1 << 1,
        Neutral = 1 << 2,

        RemovedOnDeath = 1 << 3,
        RemovedOnRespawn = 1 << 4,
        RemovedOnRoundEnd = 1 << 5,
        PersistsThroughDeath = 1 << 6,

        Hidden = 1 << 7,
        Replicated = 1 << 8
    }
}
