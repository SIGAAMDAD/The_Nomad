extends BTLeaf

func tick( delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	if actor.global_position.distance_to( actor.LastTargetPosition ) > 1024:
		actor.Aiming = false
		actor.Awareness = 1
		actor.AimTimer.stop()
		
		const _interp: float = 0.5
		
		NavigationServer2D.agent_set_
		
		return BTStatus.RUNNING
	
	return BTStatus.SUCCESS
