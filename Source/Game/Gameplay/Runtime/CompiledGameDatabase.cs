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

using Nomad.Game.Gameplay.Runtime.Areas;
using Nomad.Game.Gameplay.Runtime.Renown;
using Nomad.Game.Gameplay.Runtime.Story;
using Nomad.Game.Gameplay.Runtime.World;

namespace Nomad.Game.Gameplay.Runtime
{
	internal sealed class CompiledGameDatabase
	{
		public static readonly CompiledGameDatabase Empty = new CompiledGameDatabase(
			CompiledWorldDatabase.Empty,
			CompiledAreaDatabase.Empty,
			CompiledStoryDatabase.Empty,
			CompiledRenownDatabase.Empty
		);

		public readonly CompiledWorldDatabase World;
		public readonly CompiledAreaDatabase Areas;
		public readonly CompiledStoryDatabase Story;
		public readonly CompiledRenownDatabase Renown;

		public CompiledGameDatabase(
			CompiledWorldDatabase world,
			CompiledAreaDatabase areas,
			CompiledStoryDatabase story,
			CompiledRenownDatabase renown
		)
		{
			World = world ?? CompiledWorldDatabase.Empty;
			Areas = areas ?? CompiledAreaDatabase.Empty;
			Story = story ?? CompiledStoryDatabase.Empty;
			Renown = renown ?? CompiledRenownDatabase.Empty;
		}
	};
};
