extends BTLeaf

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	if actor.NavAgent.is_navigation_finished():
		actor.AnimationStateMachine.fire_event( "stop_moving" )
		return BTStatus.SUCCESS
	
	NavigationServer2D.agent_set_velocity( actor.NavAgent.get_rid(), actor.LookDir * actor.MovementSpeed )
	
	actor.CheckSight()
	if actor.IsAlert() || actor.IsSuspicious():
		return BTStatus.SUCCESS
	
	return BTStatus.RUNNING
