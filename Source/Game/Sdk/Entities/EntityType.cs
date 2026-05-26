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

namespace Nomad.Game.Sdk.Entities
{
    /// <summary>
    /// Broad runtime category of an entity.
    ///
    /// This should be used for coarse filtering, interaction routing, save/load routing,
    /// and broad gameplay checks. Specific behavior should come from capability interfaces.
    /// </summary>
    public enum EntityType : byte
    {
        None = 0,

        Player,
        Enemy,
        Npc,

        Item,
        Consumable,
        Weapon,
        Ammo,
        Armor,

        Container,
        Door,
        Checkpoint,
        Interactable,
        LoreObject,

        Projectile,
        ThrownObject,
        Trap,
        Hazard,

        WorldObject,
        Destructible,
        Trigger,
        Volume,

        Corpse,
        Effect
    }
}
