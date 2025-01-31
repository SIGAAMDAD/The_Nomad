class_name IdleGoal extends GoapGoal

func get_goal_name() -> String:
	return "Idle"

func is_valid() -> bool:
	return false

func priority() -> int:
	return 90

func get_desired_state() -> Dictionary:
	return {}
