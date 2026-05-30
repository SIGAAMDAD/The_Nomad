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

namespace Nomad.Game.Sdk.Multiplayer.Voting
{
    public readonly struct VoteOptionId : IEquatable<VoteOptionId>
    {
        public readonly ushort Value;

        public static readonly VoteOptionId None = new VoteOptionId(0);
        public static readonly VoteOptionId Yes = new VoteOptionId(1);
        public static readonly VoteOptionId No = new VoteOptionId(2);
        public static readonly VoteOptionId Abstain = new VoteOptionId(3);

        public bool IsValid => Value != 0;

        public VoteOptionId(ushort value)
        {
            Value = value;
        }

        public bool Equals(VoteOptionId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is VoteOptionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(VoteOptionId left, VoteOptionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VoteOptionId left, VoteOptionId right)
        {
            return !left.Equals(right);
        }
    }
}
