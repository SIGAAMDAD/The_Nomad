@tool
@icon("res://addons/behaviour_toolkit/icons/BTCompositeSequence.svg")
class_name BTSequence extends BTComposite
## A sequence node will return success if all of its children return success.


var current_leaf: int = 0


func tick(delta: float, actor: Node, blackboard: Blackboard) -> BTStatus:
	if current_leaf > leaves.size() - 1:
		current_leaf = 0
		return BTStatus.SUCCESS

	match leaves[current_leaf].tick(delta, actor, blackboard):
		BTStatus.RUNNING:
			return BTStatus.RUNNING
		BTStatus.FAILURE:
			current_leaf = 0
			return BTStatus.FAILURE
	
	current_leaf += 1
	return BTStatus.RUNNING
