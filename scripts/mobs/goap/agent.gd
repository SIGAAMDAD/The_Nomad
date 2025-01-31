class_name GoapAgent extends Node

@onready var _goals:Array[GoapGoal]
@onready var _current_goal:GoapGoal = null
@onready var _current_plan = null
@onready var _current_plan_step:int = 0
@onready var _action_planner:GoapActionPlanner

@onready var _actor

func _process( delta: float ) -> void:
	var goal = _get_best_goal()
	if _current_goal == null or goal != _current_goal:
		var blackboard = {
			"position": _actor.position,
		}
		
		for state in _actor._world_state._state:
			blackboard[ state ] = _actor._world_state._state[ state ]
		
		_current_goal = goal
		_current_plan = _action_planner.get_plan( _current_goal, blackboard )
		_current_plan_step = 0
	else:
		_follow_plan( _current_plan, delta )

func _get_best_goal() -> GoapGoal:
	var highest:GoapGoal
	
	for goal in _goals:
		if goal.is_valid() and ( highest == null or goal.priority() > highest.priority() ):
			highest = goal
	
	return highest

func init( actor, goals: Array[GoapGoal], action_planner: GoapActionPlanner ):
	_actor = actor
	_goals = goals
	_action_planner = action_planner

func _follow_plan( plan, delta: float ):
	if plan.size() == 0:
		return
	
	var isStepComplete = plan[ _current_plan_step ].perform( _actor, delta )
	if isStepComplete and _current_plan_step < plan.size() - 1:
		_current_plan_step += 1
