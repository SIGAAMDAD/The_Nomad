class_name GuardGoal extends GoapGoal

func get_goal_name() -> String:
	return "Guard"

func is_valid() -> bool:
	return _actor._world_state.get_state( "has_target", false ) == false

func priority() -> int:
	return 30 if _actor._world_state.get_state( "has_target", false ) == false else 1

func get_desired_state() -> Dictionary:
	return {
		"has_target": true
	}
