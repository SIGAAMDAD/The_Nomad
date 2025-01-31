class_name SearchGoal extends GoapGoal

func get_goal_name() -> String:
	return "Search"

func is_valid() -> bool:
	return _actor._world_state.get_state( "alert" ) == true

func priority() -> int:
	return 80

func get_desired_state() -> Dictionary:
	return {
		"has_target": true
	}
