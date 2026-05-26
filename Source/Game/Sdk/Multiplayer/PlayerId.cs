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
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Entities;

namespace Nomad.Game.Sdk.Multiplayer
{
    /// <summary>
	///
	/// </summary>
	public readonly struct PlayerId
    {
        public static PlayerId Invalid => new PlayerId(PeerId.Invalid);

        public bool IsValid => this != Invalid;

        public readonly Guid Value;

        public PlayerId(PeerId value)
        {
            Value = value.Id;
        }

        public PlayerId(EntityId value)
        {
            Value = value.Value;
        }

        public void ThrowIfInvalid(string callingMethod)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException($"{callingMethod} given an invalid PlayerId!");
            }
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is PlayerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(PlayerId other)
        {
            return Value == other.Value;
        }

        public static bool operator ==(PlayerId left, PlayerId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlayerId left, PlayerId right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(PlayerId left, PeerId right)
        {
            return left.Value.Equals(right);
        }

        public static bool operator !=(PlayerId left, PeerId right)
        {
            return !left.Value.Equals(right);
        }

        public static bool operator ==(PlayerId left, EntityId right)
        {
            return left.Value.Equals(right);
        }

        public static bool operator !=(PlayerId left, EntityId right)
        {
            return !left.Value.Equals(right);
        }

        public static implicit operator Guid(PlayerId value)
        {
            return value.Value;
        }
    }
}
