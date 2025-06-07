extends BTLeaf

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	actor.CheckSight()
	if actor.IsAlert():
		return BTStatus.SUCCESS
	elif actor.IsSuspicious():
		return BTStatus.SUCCESS
	
	return BTStatus.FAILURE
