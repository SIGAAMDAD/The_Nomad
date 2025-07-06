extends BTLeaf

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	var _angle: float
	if randi_range( 0, 99 ) > 49:
		_angle = randf_range( -50, 50.0 )
	else:
		_angle = randf_range( 130.0, 230.0 )
	
	_angle = deg_to_rad( _angle )
	actor.LookAngle = _angle
	actor.AimAngle = _angle
	return BTStatus.SUCCESS
