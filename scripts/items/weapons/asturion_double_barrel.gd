class_name AsturionDoubleBarrel extends Node2D

@onready var _weapon_data:WeaponBase = null
@onready var _ammo:AmmoBase = null

func _init( weapon: WeaponBase ) -> void:
	_weapon_data = weapon
