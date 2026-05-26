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
using Nomad.Game.Application.Gameplay.Enemy.Planner;
using Nomad.Game.Application.Gameplay.Enemy.Planner.Goals;

namespace Nomad.Game.Application.Gameplay.Enemy.Planner.Goals
{
	public sealed class GoalBuilder
	{
		private readonly List<WorldCondition> _desired = new List<WorldCondition>();

		public void Wants( WorldKey key, bool value )
		{
			_desired.Add( new WorldCondition( key, value ) );
		}

		public WorldCondition[] GetDesiredState() => _desired.ToArray();
	};
};
