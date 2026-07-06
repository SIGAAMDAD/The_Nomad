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

using System.Linq;
using Nomad.Game.Gameplay.Runtime.Biomes;
using Nomad.Game.Content.Compilation.Biomes;
using Nomad.Game.Gameplay.Runtime.World;
using Nomad.Game.Sdk.World;

namespace Nomad.Game.Content.Compilation.World
{
	internal sealed class WorldCompiler
	{
		private readonly BiomeCompiler _biomeCompiler = new BiomeCompiler();

		public CompiledWorldDatabase Compile( WorldDefinition definition )
		{
			if ( definition == null ) {
				return CompiledWorldDatabase.Empty;
			}

			CompiledBiomeDatabase biomes = _biomeCompiler.Compile( definition.Biomes );

			return new CompiledWorldDatabase(
				definition.Name,
				definition.StartTime,
				definition.Calendar,
				definition.Seasons.ToArray(),
				biomes
			);
		}
	};
};
