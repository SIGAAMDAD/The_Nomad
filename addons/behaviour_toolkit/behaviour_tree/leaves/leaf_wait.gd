@tool
@icon("res://addons/behaviour_toolkit/icons/BTLeafWait.svg")
class_name LeafWait extends BTLeaf
## Leaf that waits set ammount of calls of [code]tick()[/code] before returning
## SUCCESS.


@export var wait_time:float = 10.0

var timer:Timer = Timer.new()

func _ready() -> void:
	timer.name = "leaf_timer"
	timer.wait_time = wait_time
	timer.one_shot = true
	add_child( timer )

func tick(_delta: float, _actor: Node, _blackboard: Blackboard):
	if timer.time_left > 0.0:
		if timer.is_stopped():
			timer.start()
		return BTStatus.RUNNING
	else:
		return BTStatus.SUCCESS
