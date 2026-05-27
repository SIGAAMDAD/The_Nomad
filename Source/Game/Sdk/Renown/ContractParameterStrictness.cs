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

namespace Nomad.Game.Sdk.Renown
{
    public enum ContractParameterStrictness
    {
        /// <summary>
        /// Failing this fails the contract.
        /// </summary>
        Required,

        /// <summary>
        /// Failing this only removes bonus pay.
        /// </summary>
        Bonus,

        /// <summary>
        /// Affects client/faction relationship more than payout.
        /// </summary>
        ClientPref,

        /// <summary>
        /// Player may not know this condition at first.
        /// </summary>
        Hidden
    }
}
