class_name LookAtDisturbanceAction extends GoapAction

func get_action_name():
	return "LookAtDisturbance"

func get_cost( _blackboard ) -> int:
	return 1

func get_preconditions() -> Dictionary:
	return {
		"has_target": false
	}

func get_effects() -> Dictionary:
	return {
		"has_target": true,
	}

func perform( actor: CharacterBody2D, delta: float ) -> bool:
	return actor._target != null
