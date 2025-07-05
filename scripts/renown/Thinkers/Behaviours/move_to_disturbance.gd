extends BTLeaf

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	if actor.NavAgent.is_navigation_finished():
		return BTStatus.SUCCESS
	
	NavigationServer2D.agent_set_velocity( actor.NavAgent.get_rid(), actor.LookDir * actor.MovementSpeed )
	actor.CheckSight()
	return BTStatus.RUNNING
