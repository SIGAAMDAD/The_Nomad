extends FSMState

@export var _animation:StringName

func _on_enter( actor: Node, _blackboard: Blackboard ) -> void:
	actor.HeadAnimations.play( _animation )
	actor.BodyAnimations.play( _animation )
	if _animation == "idle":
		actor.ArmAnimations.play( "idle" )

func _on_update( _delta: float, actor: Node, _blackboard: Blackboard ) -> void:
	if actor.SightTarget != null:
		actor.LookAtTarget()
	
	actor.HeadAnimations.global_rotation = actor.LookAngle
	actor.ArmAnimations.global_rotation = actor.AimAngle
	
	if actor.BodyAnimations.flip_h:
		actor.ArmAnimations.sprite_frames = actor.Weapon.AnimationsLeft
	else:
		actor.ArmAnimations.sprite_frames = actor.Weapon.AnimationsRight
	
	if actor.velocity.x < 0.0:
		actor.BodyAnimations.flip_h = true
	elif actor.velocity.x > 0.0:
		actor.BodyAnimations.flip_h = false

func _on_exit( _actor: Node, _blackboard: Blackboard ) -> void:
	pass
