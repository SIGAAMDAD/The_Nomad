class_name ShootTargetAction extends GoapAction

func get_action_name() -> String:
	return "ShootTarget"

func get_cost( _blackboard ) -> int:
	return _blackboard[ "target" ].global_position.distance_to( _blackboard[ "position" ] ) / 10.0

func get_preconditions() -> Dictionary:
	return { "has_target": true }

func get_effects() -> Dictionary:
	return { "has_target": false }

func perform( actor: CharacterBody2D, delta: float ) -> bool:
	if actor._world_state.get_state( "target" ).global_position.distance_to( actor.global_position ) < actor._data._max_attack_range:
		return false
	else:
		actor.move_along_path( delta )
	
	return actor._target != null
