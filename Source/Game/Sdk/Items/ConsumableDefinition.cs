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
using Nomad.Core.Util;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Sdk.Items
{
    public sealed record ConsumableDefinition : ItemDefinition
    {
        public override ItemType BaseType => ItemType.Consumable;

        public PlayerResourceType Resource { get; init; }
        public float Amount { get; init; }

        public static ConsumableDefinition Load(JsonElement json)
        {
            return new ConsumableDefinition
            {
                Resource = JsonLoader.GetRequired<PlayerResourceType>(json, nameof(Resource)),
                Amount = JsonLoader.GetRequired<float>(json, nameof(Amount))
            };
        }
    }
}
