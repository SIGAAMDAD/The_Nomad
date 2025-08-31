class_name PatrolGoal extends GoapGoal

const DESIRED_STATE:Dictionary = {
	"is_patrolling": true
};

func get_goal_name() -> String:
	return "Patrol"

func is_valid() -> bool:
	return true
#	return _actor._world_state.get_state( "has_target", false ) == false and _actor._world_state.get_state( "suspicion", 0.0 ) < 60.0 and _actor._world_state.get_state( "is_patrolling", false ) == true

func priority() -> int:
	# if we don't have a target and we aren't too suspicious, we just patrol
	var hasTarget:bool = _actor._world_state.get_state( "has_target", false ) == false
	var hasPatrolRoute:bool = _actor._world_state.get_state( "is_patrolling", false ) == true
	var isCalm:bool = _actor._world_state.get_state( "suspicion", 0.0 ) < 60.0
	return 80 if hasTarget and hasPatrolRoute and isCalm else 10

func get_desired_state() -> Dictionary:
	return DESIRED_STATE
