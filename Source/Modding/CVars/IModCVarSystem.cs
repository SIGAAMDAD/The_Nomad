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
using System.Collections.Generic;

namespace Nomad.Modding.CVars
{
    public interface IModCVarSystem
    {
        IModCVar<T> Register<T>(
            ModCVarCreateInfo<T> createInfo
        );

        IModCVar<T> Register<T>(
            string localName,
            T defaultValue,
            string description = "",
            bool saved = true,
            Func<T, bool>? validator = null
        );

        bool Exists(string localName);
        bool TryGet<T>(string localName, out IModCVar<T>? cvar);

        IModCVar<T> Get<T>(string localName);
        T GetValue<T>(string localName, T fallback = default);

        bool TrySet<T>(string localName, T value);

        IReadOnlyCollection<IModCVar> GetRegisteredCVars();
    }
}
