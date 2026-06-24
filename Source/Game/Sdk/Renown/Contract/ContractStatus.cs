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

namespace Nomad.Game.Sdk.Renown.Contract
{
    public enum ContractStatus : byte
    {
        /// <summary>
        /// Currently in a faction's contract repository waiting to be claimed.
        /// </summary>
        Pending,

        /// <summary>
        /// The contract has been processed and operationally claimed.
        /// </summary>
        Active,

        /// <summary>
        /// The contract was canceled.
        /// </summary>
        Canceled,

        /// <summary>
        /// The contract's requirements have been completed, the reward is waiting to be claimed.
        /// </summary>
        Completed,

        /// <summary>
        /// The original contract's objective was failed.
        /// </summary>
        Failed
    }
}
