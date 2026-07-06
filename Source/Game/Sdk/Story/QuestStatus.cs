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
    public enum QuestStatus : byte
    {
        /// <summary>
        /// The quest hasn't yet been discovered.
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// The quest has been discovered but hasn't yet been completed.
        /// </summary>
        Available,

        /// <summary>
        /// The quest is the current one marked for completion.
        /// </summary>
        Current,

        /// <summary>
        /// The quest's primary objective was failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The quest's primary objective was completed.
        /// </summary>
        Completed
    }
}
