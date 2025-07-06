extends BTLeaf

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	if actor.IsAlert():
		return BTStatus.SUCCESS if !actor.CanSeeTarget else BTStatus.FAILURE
	elif actor.IsSuspicious():
		return BTStatus.SUCCESS
	
	return BTStatus.FAILURE
