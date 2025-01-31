class_name WeaponSlot extends Node2D

var _weapon:WeaponEntity = null
var _index:int = 0
var _mode:WeaponBase.Properties = WeaponBase.Properties.None

func is_used() -> bool:
	return _weapon != null

func _ready() -> void:
	pass
