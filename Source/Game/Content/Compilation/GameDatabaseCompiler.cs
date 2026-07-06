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
using Nomad.Core.Util;
using Nomad.Game.Gameplay.Runtime.Areas;
using Nomad.Game.Content.Compilation.Areas;
using Nomad.Game.Content.Compilation.Renown;
using Nomad.Game.Content.Compilation.Story;
using Nomad.Game.Content.Compilation.World;
using Nomad.Game.Gameplay.Runtime.Renown;
using Nomad.Game.Gameplay.Runtime.Story;
using Nomad.Game.Gameplay.Runtime.World;
using Nomad.Game.Sdk.Areas;
using Nomad.Game.Sdk.Renown.Regional;
using Nomad.Game.Sdk.Story;
using Nomad.Game.Sdk.World;
using Nomad.Game.Gameplay.Runtime;

namespace Nomad.Game.Content.Compilation
{
	internal sealed class GameDatabaseCompiler
	{
		private readonly WorldCompiler _worldCompiler = new WorldCompiler();
		private readonly AreaCompiler _areaCompiler = new AreaCompiler();
		private readonly StoryCompiler _storyCompiler = new StoryCompiler();
		private readonly RenownCompiler _renownCompiler = new RenownCompiler();

		public CompiledGameDatabase Compile(
			WorldDefinition world,
			IEnumerable<AreaDefinition> areas,
			IEnumerable<StorylineDefinition> storylines,
			IEnumerable<RegionalRenownDefinition>? regionalRenown = null,
			IEnumerable<InternString>? facts = null
		)
		{
			CompiledWorldDatabase compiledWorld = _worldCompiler.Compile( world );
			CompiledAreaDatabase compiledAreas = _areaCompiler.Compile( areas, compiledWorld.Biomes );
			CompiledStoryDatabase compiledStory = _storyCompiler.Compile( storylines, facts );
			CompiledRenownDatabase compiledRenown = _renownCompiler.Compile( compiledAreas, regionalRenown );

			return new CompiledGameDatabase(
				compiledWorld,
				compiledAreas,
				compiledStory,
				compiledRenown
			);
		}
	};
};
