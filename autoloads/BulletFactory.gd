extends Node2D

@onready var _bullets:Array[ CharacterBody2D ]
@onready var _bullet_scene:Resource = preload( "res://scenes/Items/bullet.tscn" )

func add_bullet( position: Vector2 ) -> void:
	var bullet = _bullet_scene.instantiate()
	_bullets.push_back( bullet )

func _physics_process( _delta: float ) -> void:
	for bullet in _bullets:
		bullet._position = Vector2( )
