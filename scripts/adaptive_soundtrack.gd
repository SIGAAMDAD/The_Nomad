class_name AdaptiveSoundtrack extends Node2D

@export var _soundtrack:AudioStreamPlayer = AudioStreamPlayer.new()

@onready var _player:Player = null

func init( player: Player ) -> void:
	_player = player
	_soundtrack.play()

func _process( delta: float ) -> void:
	if _soundtrack[ "parameters/switch_to_clip" ] != "Ambience":
		_soundtrack[ "parameters/switch_to_clip" ] = "Ambience"
	pass
