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

using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Interactables
{
    /// <summary>
	///
	/// </summary>
	public readonly struct CheckpointDefinitionId
    {
        public static readonly CheckpointDefinitionId Invalid = new CheckpointDefinitionId(InternString.Empty);

        public bool IsValid => Value != InternString.Empty;

        public readonly InternString Value;

        public CheckpointDefinitionId(InternString value)
        {
            Value = value;
        }

        public bool Equals(CheckpointDefinitionId other)
        {
            return Value == other.Value;
        }
    }
}
