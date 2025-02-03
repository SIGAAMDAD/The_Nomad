class_name DuelMode extends MultiplayerMode

var _player1_score:int = 0
var _player2_score:int = 0
var _max_rounds:int = 0
var _round_index:int = 0

func new_round() -> void:
	_round_index += 1

func _on_player_2_score( attacker: EntityBase, target: EntityBase ) -> void:
	_player2_score += 1
	new_round()

func _on_player_1_score( attacker: EntityBase, target: EntityBase ) -> void:
	_player1_score += 1
	new_round()

func _init( player1: Player, player2: Player, numRounds: int ) -> void:
	player1.on_player_death.connect( _on_player_2_score )
	player2.on_player_death.connect( _on_player_1_score )
	
	_max_rounds = numRounds
	_round_index = 0
	_player1_score = 0
	_player2_score = 0
