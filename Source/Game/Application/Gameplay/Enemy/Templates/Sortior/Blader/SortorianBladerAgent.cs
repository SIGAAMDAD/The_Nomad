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

namespace Nomad.Game.Application.Gameplay.Enemy.Templates.Sortior.Blader {
	/*
	===================================================================================
	
	SortorianBladerAgent
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class SortorianBladerAgent : NpcAgent {
		public override WorkingMemory Memory => BladerMemory;
		public SortorianBladerMemory BladerMemory { get; }

		public SortorianBladerAgent( EnemyBase owner, ReplanController replanController, IStateCompiler stateCompiler, IGoalSelector goalSelector, ISensor[] sensors, PlannerAction[] actions, GoalDef[] goals )
			: base( owner, replanController, stateCompiler, goalSelector, sensors, actions, goals )
		{
			BladerMemory = new SortorianBladerMemory();
		}

		protected override int GetPlanningBudget() {
			return 96;
		}

		protected override void OnActionFailed( PlannerAction action ) {
			if ( action.Name == "HeavyGuardBreak" || action.Name == "AdvanceToDuelRange" ) {
				ForceReplan( ReplanReason.ActionFailed );
			}
		}
	};
};