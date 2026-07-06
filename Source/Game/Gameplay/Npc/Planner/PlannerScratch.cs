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
using System.Buffers;
using System.Collections.Generic;
using Nomad.Game.Sdk.Npc.Planner;

namespace Nomad.Game.Gameplay.Npc.Planner
{
	internal sealed class PlannerScratch
	{
		public readonly List<PlannerNode> Nodes = new List<PlannerNode>( 128 );
		public readonly List<int> Open = new List<int>( 128 );
		public readonly Dictionary<WorldState, int> BestG = new Dictionary<WorldState, int>( 128 );

		public readonly List<PlanStep> ReversedSteps = new List<PlanStep>( 16 );

		public int[] ActionCosts = Array.Empty<int>();
		public bool[] ActionContextValid = Array.Empty<bool>();

		public void Prepare( int actionCount )
		{
			Nodes.Clear();
			Open.Clear();
			BestG.Clear();
			ReversedSteps.Clear();

			if ( ActionCosts.Length < actionCount ) {
				if ( ActionCosts.Length > 0 ) {
					ArrayPool<int>.Shared.Return( ActionCosts );
				}
				ActionCosts = ArrayPool<int>.Shared.Rent( actionCount );
			}

			if ( ActionContextValid.Length < actionCount ) {
				if ( ActionContextValid.Length > 0 ) {
					ArrayPool<bool>.Shared.Return( ActionContextValid );
				}
				ActionContextValid = ArrayPool<bool>.Shared.Rent( actionCount );
			}
		}
	}
}
