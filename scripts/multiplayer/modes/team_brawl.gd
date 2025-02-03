class_name TeamBrawlMode extends MultiplayerMode

var _team_1_score:int = 0
var _team_2_score:int = 0

var _team_1_players:Array[ Player ]
var _team_2_players:Array[ Player ]

func _on_player_death( attacker: EntityBase, target: EntityBase ) -> void:
	if attacker != null and attacker is not Player:
		push_error( "_on_player_death(team_brawl): attacker is not a player object!" )
		return
	if target is not Player:
		push_error( "_on_player_death(team_brawl): target is not a player object!" )
		return
	
	if attacker != null:
		match attacker._multiplayer_team:
			1:
				_team_1_score += 1
			2:
				_team_2_score += 1

func add_player_to_team1( player: Player ) -> void:
	_team_1_players.push_back( player )
	player.on_player_death.connect( _on_player_death )
