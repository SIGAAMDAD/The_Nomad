class_name AmmoEntity extends Node2D

@export var _data:ItemDefinition = null

@onready var _pickup_area:Area2D = $PickupArea2D
@onready var _pickup_sfx:AudioStreamPlayer2D = $PickupSfx

var _icon_sprite:Sprite2D = Sprite2D.new()

func play_sfx( sfx: AudioStreamPlayer2D ) -> void:
	sfx.global_position = global_position
	sfx.play()

func _ready() -> void:
	if !_data:
		push_error( "Cannot initialize AmmoEntity without a valid AmmoBase (null)" )
		queue_free()
		return
	
	_icon_sprite.texture = _data.icon
	_pickup_sfx.stream = _data.properties.pickup_sfx
	
	add_child( _icon_sprite )

func _on_pickup_area_2d_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not CharacterBody2D:
		return
	
	play_sfx( _pickup_sfx )
	_pickup_area.queue_free()
	_pickup_sfx.queue_free()
	_icon_sprite.queue_free()
	reparent( body )
	body.on_pickup_ammo( self )
