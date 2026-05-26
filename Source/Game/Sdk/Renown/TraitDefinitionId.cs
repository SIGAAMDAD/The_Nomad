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
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Renown
{
	/// <summary>
	///
	/// </summary>
	public readonly struct TraitDefinitionId : IEquatable<TraitDefinitionId>
	{
		public static readonly TraitDefinitionId Invalid = new TraitDefinitionId( InternString.Empty );

		public readonly InternString Value;
		public bool IsValid => Value != InternString.Empty;

		public TraitDefinitionId( InternString value )
		{
			Value = value;
		}

		public bool Equals( TraitDefinitionId other )
		{
			return Value == other.Value;
		}
	}
}
