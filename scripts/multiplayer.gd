extends MultiplayerSpawner

@export var _player_scene:PackedScene = null

@onready var _spawner:MultiplayerSpawner = $MultiplayerSpawner

var _players:Dictionary = {}

func spawn_player( data ) -> Player:
	var player := _player_scene.instantiate()
	player.set_multiplayer_authority( data )
	_players[ data ] = player
	return player

func remove_player( data ) -> void:
	_players[ data ].queue_free()
	_players[ data ].erase( data )

func _ready() -> void:
	spawn_function = spawn_player
	if is_multiplayer_authority():
		spawn( 1 )
		multiplayer.peer_connected.connect( spawn )
		multiplayer.peer_disconnected.connect( remove_player )

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
