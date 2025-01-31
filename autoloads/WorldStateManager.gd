class_name WorldStateManager extends Node2D

@onready var _state:Dictionary = {
}

func get_state( name: String, default: Variant = null ) -> Variant:
	return _state.get( name, default )

func set_state( name: String, value: Variant ) -> void:
	_state[ name ] = value

func clear() -> void:
	_state.clear()
