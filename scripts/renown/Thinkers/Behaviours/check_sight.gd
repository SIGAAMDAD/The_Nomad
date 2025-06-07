extends BTLeaf

func tick( delta: float, actor: Node, blackboard: Blackboard ) -> BTBehaviour.BTStatus:
	actor.CheckSight()
	if actor.IsAlert():
		blackboard.set_value( "Awareness", 2 )
		return BTStatus.SUCCESS
	elif actor.IsSuspicious():
		blackboard.set_value( "Awareness", 1 )
		return BTStatus.SUCCESS
	
	return BTBehaviour.BTStatus.RUNNING
