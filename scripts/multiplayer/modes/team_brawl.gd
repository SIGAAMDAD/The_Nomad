class_name TeamBrawlMode extends MultiplayerMode

var _team1:MultiplayerData.Team = null
var _team2:MultiplayerData.Team = null
var _killfeed:KillFeed = null

func _on_player_death( attacker: CharacterBody2D, target: CharacterBody2D ) -> void:
	if attacker != null && attacker is not Player:
		push_error( "_on_player_death(team_brawl) attacker is not a player object!" )
		return
	if target is not Player:
		push_error( "_on_player_death(team_brawl): target is not a player object!" )
		return
	
	# we didn't kill ourselves...
	if attacker != null:
		attacker._multiplayer_team._score += 1
		attacker._multiplayer_kills += 1
	
	target._multiplayer_deaths += 1
	_killfeed.push( attacker, attacker.get_equipped_weapon()._weapon._data.icon, target )

func init( team1: MultiplayerData.Team, team2: MultiplayerData.Team ) -> void:
	_team1 = team1
	_team2 = team2
	
	for player in _team1._players:
		player.die.connect( _on_player_death )
	
	for player in _team2._players:
		player.die.connect( _on_player_death )
