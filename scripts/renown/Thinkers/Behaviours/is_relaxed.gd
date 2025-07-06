extends BTLeaf

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	if actor.SightTarget != null:
		return BTStatus.FAILURE
	return BTStatus.FAILURE if actor.IsAlert() || actor.IsSuspicious() else BTStatus.SUCCESS
