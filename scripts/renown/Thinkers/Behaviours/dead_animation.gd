extends FSMState

func _on_enter( actor: Node, _blackboard: Blackboard ) -> void:
	actor.BodyAnimations.call_deferred( "play", "die_high" if actor.HitHead else "die_low" )
	actor.ArmAnimations.call_deferred( "hide" )
	actor.HeadAnimations.call_deferred( "hide" )

func _on_update( _delta: float, actor: Node, _blackboard: Blackboard ) -> void:
	pass

func _on_exit( _actor: Node, _blackboard: Blackboard ) -> void:
	pass
