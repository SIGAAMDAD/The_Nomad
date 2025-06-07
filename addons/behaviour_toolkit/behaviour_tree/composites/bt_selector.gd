@tool
@icon("res://addons/behaviour_toolkit/icons/BTCompositeSelector.svg")
class_name BTSelector extends BTComposite
## Selects the first child that succeeds, or fails if none do.


var current_leaf: int = 0


func tick(delta: float, actor: Node, blackboard: Blackboard) -> BTStatus:
	if current_leaf > leaves.size() - 1:
		current_leaf = 0
		return BTStatus.FAILURE
	
	match leaves[current_leaf].tick(delta, actor, blackboard):
		BTStatus.SUCCESS:
			current_leaf = 0
			return BTStatus.SUCCESS
		BTStatus.RUNNING:
			return BTStatus.RUNNING
	
	current_leaf += 1
	return BTStatus.RUNNING
