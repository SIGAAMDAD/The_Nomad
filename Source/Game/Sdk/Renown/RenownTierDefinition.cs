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

namespace Nomad.Game.Sdk.Renown
{
    public sealed record RenownTierDefinition
    {
        public readonly InternString Id;
        public readonly RenownTier Tier;
        public readonly float MinimumRenownScore;
        public readonly float MaximumRenownScore;

        public float RequiredRenown => MinimumRenownScore;

        public RenownTierDefinition(InternString id, RenownTier tier, float minimumRenownScore, float maximumRenownScore)
        {
            Id = id;
            Tier = tier;
            MinimumRenownScore = minimumRenownScore;
            MaximumRenownScore = maximumRenownScore;
        }
    }
}
