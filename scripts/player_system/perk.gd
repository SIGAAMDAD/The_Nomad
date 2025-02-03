class_name Perk extends Node

@onready var _parent:Player = null

func get_perk_name() -> String:
	assert( false )
	return "NULL"

func _init( parent: Player ) -> void:
	_parent = parent
