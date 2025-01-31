class_name EliminateThreatsGoal extends GoapGoal

func get_goal_name() -> String:
	return "EliminateThreats"

func is_valid() -> bool:
	return _actor._world_state.get_state( "has_target", false ) == true

func priority() -> int:
	return 50 if _actor._world_state.get_state( "has_target", false ) else 1

func get_desired_state() -> Dictionary:
	return {
		"has_target": false
	}
