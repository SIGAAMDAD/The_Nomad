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
using Nomad.Game.Application.Gameplay.Npc.Planner;
using Nomad.Game.Sdk.Entities;
using Nomad.Game.Sdk.Gameplay.Npc;
using Nomad.Game.Sdk.Npc;
using Nomad.Game.Sdk.Npc.Planner;
using Nomad.Game.Sdk.Npc.Planner.Goals;
using Nomad.Game.Sdk.Npc.Planner.Sensors;

namespace Nomad.Game.Application.Gameplay.Npc
{
	/*
	===================================================================================

	NpcAgent

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class NpcAgent : NpcBase, INpcAgent
	{
		public virtual WorkingMemory Memory { get; } = new WorkingMemory();

		public NpcAgentId AgentId { get; private set; }

		public WorldState CurrentState { get; private set; }
		public GoalDef CurrentGoal => _currentGoal;
		public Plan CurrentPlan => _currentPlan;
		public PlannerAction RunningAction => _runningAction;

		public bool HasPlan => _currentPlan != null && !_currentPlan.IsFinished;
		public bool IsExecutingAction => _runningAction != null;

		private readonly ReplanController _replanController;
		private readonly IStateCompiler _stateCompiler;
		private readonly IGoalSelector _goalSelector;
		private readonly ISensor[] _sensors;
		private readonly PlannerAction[] _actions;
		private readonly GoalDef[] _goals;

		private Plan _currentPlan = Plan.Empty;
		private PlannerAction _runningAction;
		private GoalDef _currentGoal;

		private readonly INpcAgentBehavior _behavior;

		protected NpcAgent( NpcAgentId agentId, INpcAgentBehavior behavior )
			: base(
				new EntityId( agentId ),
				behavior.Archetype.Id,
				behavior.Archetype.DisplayName,
				EntityType.Npc
			)
		{
			_behavior = behavior ?? throw new ArgumentNullException( nameof( behavior ) );
			AgentId = agentId;
			_replanController = new ReplanController( 5.0f, 5.0f );
			_stateCompiler = behavior.Archetype.StateCompiler ?? throw new ArgumentNullException( nameof( behavior.Archetype.StateCompiler ) );
			_goalSelector = behavior.Archetype.GoalSelector ?? throw new ArgumentNullException( nameof( behavior.Archetype.GoalSelector ) );
			_sensors = behavior.Archetype.Sensors ?? Array.Empty<ISensor>();
			_actions = behavior.Archetype.Actions ?? Array.Empty<PlannerAction>();
			_goals = behavior.Archetype.Goals ?? Array.Empty<GoalDef>();
		}

		/*
		===============
		Tick
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="dt"></param>
		public void Tick( float dt )
		{
			TickSensors( dt );
			CurrentState = _stateCompiler.BuildState( this );

			PlanningContext context = new PlanningContext( Memory );
			GoalDef bestGoal = _goalSelector.SelectBestGoal( this, _goals );

			if ( !ReferenceEquals( bestGoal, _currentGoal ) ) {
				_currentGoal = bestGoal;
				_replanController.MarkDirty( ReplanReason.GoalChanged );
			}

			_replanController.Tick( dt );
			bool currentStepStillValid = IsCurrentStepStillValid( context );

			if ( _replanController.ShouldReplan( HasPlan, currentStepStillValid ) ) {
				BuildPlan( context );
			}

			ExecuteCurrentStep( dt );
		}

		public void ForceReplan( ReplanReason reason = ReplanReason.ContextInvalid )
		{
			_replanController.MarkDirty( reason );
		}

		public void NotifyHeavyDamage()
		{
			_replanController.MarkDirty( ReplanReason.TookHeavyDamage );
		}

		public void NotifyTargetLost()
		{
			_replanController.MarkDirty( ReplanReason.TargetLost );
		}

		protected virtual int GetPlanningBudget()
		{
			return 64;
		}

		private void TickSensors( float dt )
		{
			for ( int i = 0; i < _sensors.Length; i++ ) {
				_sensors[i].Tick( this, dt );
			}
		}

		private bool IsCurrentStepStillValid( PlanningContext context )
		{
			if ( !HasPlan ) {
				return false;
			}

			PlannerAction step = _currentPlan.CurrentStep.Action;
			if ( !CurrentState.Meets( step.Preconditions ) ) {
				return false;
			}

			if ( step.ValidateContext != null && !step.ValidateContext( context ) ) {
				return false;
			}

			return true;
		}

		private void BuildPlan( PlanningContext context )
		{
			ReplanReason reasons = _replanController.ConsumeReasons();

			if ( _currentGoal == null ) {
				ClearPlan();
				return;
			}

			if ( AStarPlanner.TryPlan( CurrentState, _currentGoal, context, _actions, GetPlanningBudget(), out Plan plan ) ) {
				CancelRunningActionIfAny();
				_currentPlan = plan;
				_replanController.OnPlanBuilt();
				_behavior.OnPlanBuilt( this, plan, reasons );
			} else {
				ClearPlan();
				_behavior.OnPlanFailed( this, _currentGoal, reasons );
			}
		}

		private void ExecuteCurrentStep( float dt )
		{
			if ( !HasPlan ) {
				return;
			}

			PlannerAction step = _currentPlan.CurrentStep.Action;
			if ( !ReferenceEquals( _runningAction, step ) ) {
				_runningAction = step;

				ActionRunStatus started = step.Runner.Start( this );
				if ( started == ActionRunStatus.Succeeded ) {
					_behavior.OnActionSucceeded( this, step );
					AdvancePlan();
					return;
				}

				if ( started == ActionRunStatus.Failed ) {
					_behavior.OnActionFailed( this, step );
					_runningAction = null;
					_replanController.MarkDirty( ReplanReason.ActionFailed );
					return;
				}

				_behavior.OnActionStarted( this, step );
			}

			ActionRunStatus tickStatus = step.Runner.Tick( this, dt );
			switch ( tickStatus ) {
				case ActionRunStatus.Running:
					break;

				case ActionRunStatus.Succeeded:
					_behavior.OnActionSucceeded( this, step );
					_runningAction = null;
					AdvancePlan();
					break;

				case ActionRunStatus.Failed:
					_behavior.OnActionFailed( this, step );
					_runningAction = null;
					_replanController.MarkDirty( ReplanReason.ActionFailed );
					break;

				default:
					throw new InvalidOperationException( "Unknown ActionRunStatus." );
			}
		}

		private void AdvancePlan()
		{
			if ( _currentPlan == null ) {
				return;
			}

			_currentPlan.CurrentIndex++;
			if ( _currentPlan.IsFinished ) {
				_behavior.OnPlanCompleted( this, _currentGoal );
			}
		}

		private void ClearPlan()
		{
			CancelRunningActionIfAny();
			_currentPlan = Plan.Empty;
		}

		private void CancelRunningActionIfAny()
		{
			if ( _runningAction != null ) {
				_runningAction.Runner.Cancel( this );
				_runningAction = null;
			}
		}
	};
};
