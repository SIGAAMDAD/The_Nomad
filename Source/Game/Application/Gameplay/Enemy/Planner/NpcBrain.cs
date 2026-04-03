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

using Nomad.Game.Application.Gameplay.Enemy.Planner.Goals;

namespace Nomad.Game.Application.Gameplay.Enemy.Planner {
	/*
	===================================================================================
	
	NpcBrain
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class NpcBrain {
		private readonly ReplanController _replan;
		private readonly PlannerAction[] _actions;
		private readonly GoalDef[] _goals;
		private readonly WorkingMemory _memory;

		private Plan _currentPlan = Plan.Empty;
		private PlannerAction _runningAction;

		public NpcBrain( ReplanController replan, PlannerAction[] actions, GoalDef[] goals, WorkingMemory memory ) {
			_replan = replan;
			_actions = actions;
			_goals = goals;
			_memory = memory;
		}

		public void Tick( float dt, WorldState currentState ) {
			_replan.Tick( dt );

			GoalDef goal = SelectBestGoal( _goals, new PlanningContext( _memory ) );

			bool hasUsablePlan = _currentPlan != null && !_currentPlan.IsFinished;
			bool currentStepStillValid = true;

			if ( hasUsablePlan ) {
				PlannerAction step = _currentPlan.CurrentStep.Action;
				if ( step.ValidateContext != null ) {
					currentStepStillValid = step.ValidateContext( new PlanningContext( _memory ) );
				}
			}

			if ( _replan.ShouldReplan( hasUsablePlan, currentStepStillValid ) ) {
				_replan.ConsumeReasons();

				if ( AStarPlanner.TryPlan( currentState, goal, new PlanningContext( _memory ), _actions, maxExpansions: 64, out Plan newPlan ) ) {
					_currentPlan = newPlan;
					_replan.OnPlanBuilt();
				}
			}

			ExecuteCurrentStep( dt );
		}

		private static GoalDef SelectBestGoal( GoalDef[] goals, PlanningContext context ) {
			GoalDef best = goals[0];
			int bestScore = best.GetPriority( context );

			for ( int i = 1; i < goals.Length; i++ ) {
				int score = goals[i].GetPriority( context );
				if ( score > bestScore ) {
					best = goals[i];
					bestScore = score;
				}
			}

			return best;
		}

		private void ExecuteCurrentStep( float dt ) {
			if ( _currentPlan == null || _currentPlan.IsFinished ) {
				return;
			}

			PlannerAction step = _currentPlan.CurrentStep.Action;

			if ( _runningAction != step ) {
				_runningAction = step;
				ActionRunStatus started = step.Runner.Start( this );

				if ( started == ActionRunStatus.Failed ) {
					_runningAction = null;
					_replan.MarkDirty( ReplanReason.ActionFailed );
					return;
				}

				if ( started == ActionRunStatus.Succeeded ) {
					_runningAction = null;
					_currentPlan.CurrentIndex++;
					return;
				}
			}

			ActionRunStatus status = step.Runner.Tick( this, dt );

			if ( status == ActionRunStatus.Succeeded ) {
				_runningAction = null;
				_currentPlan.CurrentIndex++;
			} else if ( status == ActionRunStatus.Failed ) {
				_runningAction = null;
				_replan.MarkDirty( ReplanReason.ActionFailed );
			}
		}

		public void NotifyTargetLost() {
			_replan.MarkDirty( ReplanReason.TargetLost );
		}

		public void NotifyHeavyDamage() {
			_replan.MarkDirty( ReplanReason.TookHeavyDamage );
		}
	};
};