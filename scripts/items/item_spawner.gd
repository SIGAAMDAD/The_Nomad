extends Node2D

@export var _data:ItemDefinition = null

var _script = null

func _ready() -> void:
	_script = load( "res://scenes/Items/Weapons/weapon_base.tscn" ).instantiate()
	_script.set( "Data", _data )
	add_child( _script )
