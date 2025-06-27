extends BTLeaf

@export var _lose_interest_time:float = 10.0

var _timer:Timer = Timer.new()

func _ready() -> void:
	_timer.wait_time = _lose_interest_time
	_timer.one_shot = true
	add_child( _timer )

func tick( _delta: float, actor: Node, _blackboard: Blackboard ) -> BTStatus:
	if _timer.is_stopped():
		_timer.start()
	elif _timer.time_left == 0.0:
		return BTStatus.FAILURE
	
	actor.CheckSight()
	if actor.IsAlert() || actor.IsSuspicious():
		return BTStatus.SUCCESS
	
	actor.OnChangeInvestigationAngleTimerTimeout()
	return BTStatus.RUNNING
