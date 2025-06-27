extends FSMState

@export var _animation:StringName

func _on_enter( actor: Node, _blackboard: Blackboard ) -> void:
	actor.HeadAnimations.call_deferred( "play", _animation )
	actor.BodyAnimations.call_deferred( "play", _animation )
	if _animation == "idle":
		actor.ArmAnimations.call_deferred( "play", "idle" )

func _on_update( _delta: float, actor: Node, _blackboard: Blackboard ) -> void:
	if actor.SightTarget != null:
		actor.LookAtTarget()
	
	actor.HeadAnimations.set_deferred( "global_rotation", actor.LookAngle )
	actor.ArmAnimations.set_deferred( "global_rotation", actor.AimAngle )
	
	if actor.BodyAnimations.flip_h:
		actor.ArmAnimations.set_deferred( "sprite_frames", actor.Weapon.AnimationsLeft )
	else:
		actor.ArmAnimations.set_deferred( "sprite_frames", actor.Weapon.AnimationsRight )
	
	if actor.velocity.x < 0.0:
		actor.BodyAnimations.set_deferred( "flip_h", true )
	elif actor.velocity.x > 0.0:
		actor.BodyAnimations.set_deferred( "flip_h", false )

func _on_exit( _actor: Node, _blackboard: Blackboard ) -> void:
	pass
