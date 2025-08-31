class_name FindThreatsGoal extends GoapGoal

func get_goal_name() -> String:
	return "FindThreats"

func is_valid() -> bool:
	return _actor._world_state.get_state( "has_target", false ) == false

func priority() -> int:
	return 10 if _actor._world_state.get_state( "has_target", false ) == false else 1

func get_desired_state() -> Dictionary:
	return {
		"has_target": true
	}
