class_name PatrolAction extends GoapAction

const PRECONDITIONS:Dictionary = {
	"has_target": false
};
const EFFECTS:Dictionary = {
	"has_target": true,
	"is_patrolling": true
};

func get_action_name():
	return "Patrol"

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
#	if actor._astar_path.is_empty():
#		actor._astar_path = actor._pathfinder.get_valid_path( actor.global_position / 16, actor._patrol_goal.global_position / 16 )
	
	if actor._target != null:
		actor._world_state.set_state( "has_target", true )
		actor._world_state.set_state( "is_patrolling", false )
	
	actor.move_along_path( delta )
	
	return actor._target != null
