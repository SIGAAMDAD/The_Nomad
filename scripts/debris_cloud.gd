extends Node2D

@onready var _timer:Timer = $Timer
@onready var _texture:Texture = preload( "res://textures/env/dustcloud.png" )

func _on_timer_timeout() -> void:
	_timer.queue_free()
	queue_free()

func _ready() -> void:
	_timer.timeout.connect( _on_timer_timeout )

func create( position: Vector2 ) -> void:
#	var velocity := range - from.distance_to( to )
	var numSmokeClouds := 64
	
	for i in range( numSmokeClouds ):
		var cloud := DebrisSmoke.new()
		cloud.texture = _texture
		cloud.global_position = position
		add_child( cloud )
