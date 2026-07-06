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
using System.Diagnostics.CodeAnalysis;
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Npc
{
    /// <summary>
    /// Identifies a data-authored GOAP action definition.
    /// </summary>
    public readonly struct AgentActionDefinitionId : IEquatable<AgentActionDefinitionId>
    {
        public static readonly AgentActionDefinitionId Invalid = new AgentActionDefinitionId(InternString.Empty);

        public readonly InternString Value;

        public bool IsValid => Value != InternString.Empty;

        public AgentActionDefinitionId(InternString value)
        {
            Value = value;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is AgentActionDefinitionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(AgentActionDefinitionId other)
        {
            return other.Value == Value;
        }

        public static implicit operator InternString(AgentActionDefinitionId value)
        {
            return value.Value;
        }

        public static bool operator ==(AgentActionDefinitionId left, AgentActionDefinitionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AgentActionDefinitionId left, AgentActionDefinitionId right)
        {
            return !left.Equals(right);
        }
    }
}
