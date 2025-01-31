class_name InvestigateDisturbanceAction extends GoapAction

const PRECONDITIONS:Dictionary = {
	"alert": true
};

const EFFECTS:Dictionary = {
	"has_target": true
};

func get_action_name() -> String:
	return "InvestigateDisturbance"

func get_cost( _blackboard ) -> int:
	return 10

func get_preconditions() -> Dictionary:
	return PRECONDITIONS

func get_effects() -> Dictionary:
	return EFFECTS

func perform( actor: CharacterBody2D, delta: float ) -> bool:
	
	
	return actor._target != null
