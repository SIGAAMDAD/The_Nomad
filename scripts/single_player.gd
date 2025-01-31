extends Node2D

@onready var _player1:Player = $Network/Player1
@onready var _adaptive_soundtrack:Node2D = $AdaptiveSoundtrack
@onready var _spawner:MultiplayerSpawner = $MultiplayerSpawner

func _ready() -> void:
	_adaptive_soundtrack.init( _player1 )
	if Input.get_connected_joypads().size() > 0:
		_player1.setup_split_screen( 0 )
