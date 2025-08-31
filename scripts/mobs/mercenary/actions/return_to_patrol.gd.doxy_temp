class_name ReturnToPatrolAction extends GoapAction

func get_action_name():
	return "ReturnToPatrol"

func get_cost( _blackboard ) -> int:
	return 1

func get_preconditions() -> Dictionary:
	return {
		"is_patrolling": false,
	}

func get_effects() -> Dictionary:
	return {
		"is_patrolling": true
	}

# return to our starting position to continue the patrol
func perform( actor: CharacterBody2D, delta: float ) -> bool:
#	if !actor._astar_path.is_empty():
#		actor._astar_path.clear()
#		actor._astar_path = actor._pathfinder.get_valid_path( actor.global_position / 16 , actor._starting_position / 16 )
	actor._navigation.target_position = actor._starting_position
	
	return actor.global_position != actor._starting_position
