@export var _resource:QuestResource = null

var _instance:QuestResource = null

func initialize( path: String ) -> void:
	_resource = load( path )

func start() -> void:
	_instance = _resource.instantiate()
	Questify.start_quest( _instance )

func is_completed() -> bool:
	return _instance.completed

func add_objective() -> void:
	pass
