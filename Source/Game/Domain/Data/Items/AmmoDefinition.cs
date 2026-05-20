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

using System.Text.Json;
using Nomad.Core.Util;

namespace Nomad.Game.Domain.Data.Items
{
	public sealed record AmmoDefinition : ItemDefinition
	{
		public override ItemType BaseType => ItemType.Ammunition;
		public AmmoType Type { get; init; }
		public AmmoModifier Modifier { get; init; }
		public float Range { get; init; }
		public float Velocity { get; init; }
		public float Damage { get; init; }

		public static AmmoDefinition Load( JsonElement json )
		{
			return new AmmoDefinition {
				Type = JsonLoader.GetRequired<AmmoType>( json, nameof( Type ) ),
				Modifier = JsonLoader.GetRequired<AmmoModifier>( json, nameof( Modifier ) ),
				Damage = JsonLoader.GetRequired<float>( json, nameof( Damage ) ),
				Velocity = JsonLoader.GetRequired<float>( json, nameof( Velocity ) ),
				Range = JsonLoader.GetRequired<float>( json, nameof( Range ) )
			};
		}
	};
};
