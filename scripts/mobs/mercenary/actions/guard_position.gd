class_name GuardAction extends GoapAction

const PRECONDITIONS:Dictionary = {
	"has_target": false
};
const EFFECTS:Dictionary = {
	"has_target": true
};

func get_action_name():
	return "Guard"

func get_cost( _blackboard ) -> int:
	return 4

func get_preconditions() -> Dictionary:
	return PRECONDITIONS

func get_effects() -> Dictionary:
	return EFFECTS

func perform( actor: CharacterBody2D, delta: float ) -> bool:
	if actor._target != null:
		actor._world_state.set_state( "has_target", true )
		actor.play_sfx( actor._target_spotted[ randi_range( 0, actor._target_spotted.size() - 1 ) ] )
	
	return actor._target == null
