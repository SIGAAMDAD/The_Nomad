extends Control

@onready var _root = $"/root/Multiplayer"

@onready var _massacre_data = $MarginContainer/PlayerList/MassacreData

func _ready() -> void:
	var startIndex := 2
	var data_table:VBoxContainer = null
	
	match _root._mode:
		MultiplayerMode.GameMode.Massacre:
			data_table = _massacre_data
	
	for player in _root._players.values():
		var data := data_table.get_child( startIndex ) as HBoxContainer
		
		data.show()
		data.get_child( 0 ).text = player._multiplayer_username
		data.get_child( 1 ).text = str( player._multiplayer_kills )
		data.get_child( 2 ).text = str( player._multiplayer_deaths )
		
		startIndex += 1
