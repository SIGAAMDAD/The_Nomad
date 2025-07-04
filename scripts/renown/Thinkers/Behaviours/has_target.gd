extends BTLeaf

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	if actor.Target != null:
		return
	
	return BTStatus.RUNNING
