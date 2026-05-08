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
	public sealed record FirearmDefinition : WeaponDefinition
	{
		public override ItemType BaseType => ItemType.Firearm;
		public FirearmFlags Flags { get; init; }
		public float FireRate { get; init; }
		public int MagazineSize { get; init; }

		public static FirearmDefinition Load( JsonElement json )
		{
			return new FirearmDefinition {
				Type = JsonLoader.TryGet( json, nameof( Type ), out WeaponType type ) ? type : WeaponType.Firearm,
				BaseDurability = JsonLoader.TryGet( json, nameof( BaseDurability ), out float baseDurability ) ? baseDurability : 0.0f,
				Flags = JsonLoader.TryGet( json, nameof( Flags ), out FirearmFlags flags ) ? flags : FirearmFlags.None,
				FireRate = JsonLoader.TryGet( json, nameof( FireRate ), out float fireRate ) ? fireRate : 0.0f,
				MagazineSize = JsonLoader.TryGet( json, nameof( MagazineSize ), out int magazineSize ) ? magazineSize : 0,
			};
		}
	};
};
