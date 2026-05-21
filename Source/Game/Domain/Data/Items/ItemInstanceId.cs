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

namespace Nomad.Game.Domain.Data.Items
{
	/// <summary>
	///
	/// </summary>
	public readonly struct ItemInstanceId : IEquatable<ItemInstanceId>
	{
		public static readonly ItemInstanceId Invalid = new ItemInstanceId( Guid.Empty );

		public readonly Guid Value;
		public bool IsValid => Value != Guid.Empty;

		public ItemInstanceId( Guid value )
		{
			Value = value;
		}

		public bool Equals( ItemInstanceId other )
		{
			return Value == other.Value;
		}
	};
};
