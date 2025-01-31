class_name GotoNodeAction extends GoapAction

const PRECONDITIONS:Dictionary = {
	"is_patrolling": true,
#	"has_destination": true
};
const EFFECTS:Dictionary = {
	"has_destination": false,
	"is_patrolling": true
};

func get_action_name():
	return "GotoNode"

func get_cost( _blackboard ) -> int:
	return _blackboard[ "goto_node_distance" ]

func get_preconditions() -> Dictionary:
	return PRECONDITIONS

func get_effects() -> Dictionary:
	return EFFECTS

func perform( actor: CharacterBody2D, delta: float ) -> bool:
	actor.move_along_path( delta )
	return actor._navigation.is_target_reached()
