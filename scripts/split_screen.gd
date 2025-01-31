extends Node2D

@onready var _players:Dictionary = {
	"1": {
		subviewport = $ScreenData/Player1/SubViewport,
		camera = $ScreenData/Player1/SubViewport/Camera2D,
		player = $ScreenData/Player1/SubViewport/Network/Player1
	},
	"2": {
		subviewport = $ScreenData/Player2/SubViewport,
		camera = $ScreenData/Player2/SubViewport/Camera2D,
		player = $ScreenData/Player1/SubViewport/Network/Player2
	}
}

#TODO: test this
@onready var _spawner = $ScreenData/Player1/SubViewport/Network

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var playerList = Input.get_connected_joypads()
	
	_players[ "2" ].subviewport.world_2d = _players[ "1" ].subviewport.world_2d
	_players[ "1" ].player.camera.hide()
	
	_players[ "2" ].player = load( "res://scenes/Player.tscn" ).instantiate()
	_spawner.add_child( _players[ "2" ].player )
	
	_players[ "1" ].player.setup_split_screen( playerList[ 0 ] )
	_players[ "2" ].player.setup_split_screen( playerList[ 1 ] )
	
	for node in _players.values():
		var remote_transform := RemoteTransform2D.new()
		remote_transform.remote_path = node.camera.get_path()
		node.player.add_child( remote_transform )
