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

namespace Nomad.Game.Sdk.Story
{
    /// <summary>
    /// An objective for a quest.
    /// </summary>
    public sealed record QuestObjectiveDefinition
    {
        public QuestObjectiveId Id { get; init; }
        public InternString DisplayName { get; init; }

        /// <summary>
        /// Special designator if the objective has a quantity required to complete it.
        /// </summary>
        public int? Quantity { get; init; }

        /// <summary>
        /// Special designator for optional/side objectives.
        /// </summary>
        public bool Optional { get; init; }

        public static QuestObjectiveDefinition Load(JsonElement json)
        {
            return new QuestObjectiveDefinition
            {
                Id = new QuestObjectiveId(new InternString(json.GetRequired<string>(nameof(Id)))),
                DisplayName = new InternString(json.GetRequired<string>(nameof(DisplayName))),
                Quantity = json.TryGet(nameof(Quantity), out int quantity) ? quantity : null,
                Optional = json.GetRequired<bool>(nameof(Optional))
            };
        }
    }
}
