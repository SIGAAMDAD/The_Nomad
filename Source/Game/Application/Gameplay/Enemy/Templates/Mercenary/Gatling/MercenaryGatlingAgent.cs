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

using Nomad.Game.Application.Gameplay.Enemy.Planner;
using Nomad.Game.Application.Gameplay.Enemy.Planner.Goals;

namespace Nomad.Game.Application.Gameplay.Enemy.Templates.Mercenary.Gatling {
	/*
	===================================================================================
	
	MercenaryGatlingAgent
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class MercenaryGatlingAgent : NpcAgent {
		public override WorkingMemory Memory => GatlingMemory;
		public MercenaryGatlingMemory GatlingMemory { get; }

		public MercenaryGatlingAgent( EnemyBase owner, ReplanController replanController, IStateCompiler stateCompiler, IGoalSelector goalSelector, ISensor[] sensors, PlannerAction[] actions, GoalDef[] goals )
			: base( owner, replanController, stateCompiler, goalSelector, sensors, actions, goals )
		{
			GatlingMemory = new MercenaryGatlingMemory();
		}

		protected override int GetPlanningBudget() {
			return 96;
		}
	};
};