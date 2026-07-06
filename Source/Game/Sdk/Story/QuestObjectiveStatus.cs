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
    public enum QuestObjectiveStatus : byte
    {
        /// <summary>
        /// The current primary objective.
        /// </summary>
        Main,

        /// <summary>
        /// Optional objective that hasn't been completed yet.
        /// </summary>
        SideIncomplete,

        /// <summary>
        /// Optional objective that was failed.
        /// </summary>
        SideFailed,

        /// <summary>
        /// Optional objective that has been completed.
        /// </summary>
        SideCompleted,

        /// <summary>
        /// Queued but not current objective.
        /// </summary>
        Inactive,

        /// <summary>
        /// A reqiured objective that was failed.
        /// </summary>
        Failed,

        /// <summary>
        /// A completed main objective.
        /// </summary>
        Completed
    }
}
