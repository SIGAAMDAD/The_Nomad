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

namespace Nomad.Game.Sdk.Story
{
    /// <summary>
    /// The relationship between two quests.
    /// </summary>
    public enum QuestRelationship : byte
    {
        /// <summary>
        /// They are completely unrelated.
        /// </summary>
        None = 0,

        /// <summary>
        /// The quest being queried is the child of another quest.
        /// </summary>
        Child,

        /// <summary>
        /// The quest being queried is the parent of another quest.
        /// </summary>
        Parent,

        /// <summary>
        /// The quest being queried is a sibling, or alternate path, of another quest.
        /// </summary>
        Sibling,

        /// <summary>
        /// The quest being queried is within the same storyline, but not in the immediate family tree.
        /// </summary>
        Storyline,

        /// <summary>
        /// Ideally should never be returned, but the same quest as the one being queried.
        /// </summary>
        IsSame
    }
}
