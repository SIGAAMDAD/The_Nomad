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

namespace Nomad.Game.Sdk.Story
{
    /// <summary>
    ///
    /// </summary>
    public readonly struct QuestInstanceId
    {
        public readonly uint Value;

        public QuestInstanceId(uint value)
        {
            Value = value;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is QuestInstanceId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(QuestInstanceId other)
        {
            return other.Value == Value;
        }

        public static implicit operator uint(QuestInstanceId value)
        {
            return value.Value;
        }

        public static bool operator ==(QuestInstanceId left, QuestInstanceId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(QuestInstanceId left, QuestInstanceId right)
        {
            return !left.Equals(right);
        }
    }
}
