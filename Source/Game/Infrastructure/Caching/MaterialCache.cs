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

using System.Collections.Generic;
using Godot;

namespace Nomad.Game.Infrastructure.Caching
{
	internal sealed partial class MaterialCache : Resource
	{
		[Export]
		private global::Godot.Collections.Dictionary<StringName, OrmMaterial3D> _materials = new();

		private static readonly MaterialCache _instance = new MaterialCache();

		public static OrmMaterial3D GetMaterial( StringName material )
		{
			if ( !_instance._materials.TryGetValue( material, out var mat ) ) {
				return null;
			}

			return mat;
		}

		public static ICollection<OrmMaterial3D> GetMaterials()
		{
			return _instance._materials.Values;
		}
	};
};
