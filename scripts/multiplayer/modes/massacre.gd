class_name MassacreMode extends MultiplayerMode

class Score:
	var _player:Player = null
	var _kills:int = 0
	var _deaths:int = 0
	var _points:int = 0

var _scores:Array[ Score ]

func _init( numPlayers: int ) -> void:
	print( "Initializing Massacre mode..." )
	print( "PlayerCount: ", numPlayers )
	
	_scores.resize( numPlayers )
