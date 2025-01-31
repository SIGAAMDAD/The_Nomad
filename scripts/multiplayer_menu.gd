extends Control

func _on_refresh_pressed() -> void:
	if $LobbyContainer.get_child_count() > 0:
		for lobby in $LobbyContainer.get_children():
			lobby.queue_free()

func _on_host_pressed() -> void:
	SteamLobby.create_lobby()
	$HostButton.hide()
	$LobbyContainer/Lobbies.hide()

func join_lobby( lobbyId: int ) -> void:
	Steam.joinLobby( lobbyId )
	$HostButton.hide()
	$LobbyContainer/Lobbies.hide()

var _lobby_list:Array

func open_lobby_list() -> void:
	Steam.addRequestLobbyListDistanceFilter( Steam.LobbyDistanceFilter.LOBBY_DISTANCE_FILTER_WORLDWIDE )
	Steam.requestLobbyList()

func _on_lobby_match_list( lobbies ) -> void:
	print( "Loading lobby list..." )
	for lobby in lobbies:
		var lobbyName := Steam.getLobbyData( lobby, "name" )
		var lobbyMemberCount := Steam.getNumLobbyMembers( lobby )
		
		var button = Button.new()
		button.set_text( str( lobbyName, "| Player(s): ", lobbyMemberCount ) )
		button.set_size( Vector2( 100, 5 ) )
		button.connect( "pressed", Callable( self, "join_lobby" ).bind( lobby ) )
		
		$LobbyContainer/Lobbies.add_child( button )

func _ready() -> void:
	Steam.lobby_match_list.connect( _on_lobby_match_list )
	open_lobby_list()
