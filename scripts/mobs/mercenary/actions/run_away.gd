class_name RunAwayAction extends GoapAction

func get_action_name() -> String:
	return "RunAway"

func get_cost( _blackboard ) -> int:
	return 0

func get_preconditions() -> Dictionary:
	return {
		"fear": 75.0
	}

func get_effects() -> Dictionary:
	return {
		"has_target": true,
	}

func perform( actor: CharacterBody2D, delta: float ) -> bool:
	if actor._astar_path.is_empty():
		actor._astar_path = actor._pathfinder.get_valid_path( actor.global_position / 16, actor._patrol_goal.global_position / 16 )
	
	if actor._target != null:
		actor._world_state.set_state( "has_target", true )
		actor._world_state.set_state( "is_patrolling", false )
	
	actor.move_along_path( delta )
	return actor._target != null
