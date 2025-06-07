@tool
@icon("res://addons/behaviour_toolkit/icons/BTDecoratorNot.svg")
class_name BTInverter extends BTDecorator
## The result of the leaf is inverted.


func tick(delta: float, actor: Node, blackboard: Blackboard) -> BTStatus:
	match leaf.tick( delta, actor, blackboard ):
		BTStatus.SUCCESS:
			return BTStatus.FAILURE
		BTStatus.FAILURE:
			return BTStatus.SUCCESS
		_:
			return BTStatus.RUNNING
