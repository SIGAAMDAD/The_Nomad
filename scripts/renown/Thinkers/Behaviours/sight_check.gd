extends BTLeaf

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	actor.CheckSight()
	if actor.CanSeeTarget:
		return BTStatus.SUCCESS
	
	return BTStatus.RUNNING
