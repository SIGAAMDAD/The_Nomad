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

namespace Nomad.Modding.CVars
{
    public sealed record ModCVarCreateInfo<T>
    {
        public string LocalName { get; init; }
        public T DefaultValue { get; init; }

        public string Description { get; init; }

        public bool Saved { get; init; }
        public bool Hidden { get; init; }
        public bool ReadOnly { get; init; }
        public bool DeveloperOnly { get; init; }

        public Func<T, bool>? Validator { get; init; }
    }
}
