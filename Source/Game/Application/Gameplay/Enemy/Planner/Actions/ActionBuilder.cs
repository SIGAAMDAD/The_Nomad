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

namespace Nomad.Game.Application.Gameplay.Enemy.Planner.Actions
{
	public sealed class ActionBuilder
	{
		private readonly List<WorldCondition> _preconditions = new List<WorldCondition>();
		private readonly List<WorldEffect> _effects = new List<WorldEffect>();

		public int BaseCost { get; set; }

		public void Requires( WorldKey key, bool value )
		{
			_preconditions.Add( new WorldCondition( key, value ) );
		}

		public void Effect( WorldKey key, bool value )
		{
			_effects.Add( new WorldEffect( key, value ) );
		}

		public WorldCondition[] GetPreconditions() => _preconditions.ToArray();
		public WorldEffect[] GetEffects() => _effects.ToArray();
	};
};
