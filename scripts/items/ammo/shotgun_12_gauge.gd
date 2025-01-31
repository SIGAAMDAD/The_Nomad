extends Node2D

@onready var _data:AmmoBase = ResourceLoader.load( "res://resources/ammo/shotgun_12gauge.tres" )
@onready var _pickup_sfx:AudioStreamPlayer2D = $PickupSfx
@onready var _icon_sprite:Sprite2D = $Icon

func play_sfx( sfx: AudioStreamPlayer2D ) -> void:
	sfx.global_position = global_position
	sfx.play()

func _ready() -> void:
	_pickup_sfx.stream = _data._pickup_sfx
	_icon_sprite.texture = _data._icon

func _on_pickup_area_2d_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		return
	
	play_sfx( _pickup_sfx )
	_icon_sprite.hide()
