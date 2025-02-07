extends Area2D

@export var _loot_table:ItemDefinition = null

@onready var _t

var _is_open:bool = false

func _on_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if _is_open || body != Player:
		return
