extends Node

const _PACKET_READ_LIMIT:int = 32

enum Visibility {
	Private,
	Public,
	FriendsOnly
};

signal client_joined_lobby( steamId: int )
signal client_left_lobby( steamId: int )
signal lobby_created( lobbyId: int )
signal lobby_joined( lobbyId: int )
signal lobby_join_requested( lobbyId: int )
signal lobby_owner_changed( previousOwner: int, newOwner: int )
signal lobby_data_updated( steamId: int )
signal chat_message_received( senderSteamId: int, message: String )

signal data_received( sender: int, data: Dictionary )
signal on_member_joined( steamId: int )

var _is_host:bool = false
var _lobby_owner_id:int = 0
var _lobby_id:int = 0
var _lobby_members:Array = []
var _lobby_visibility:Visibility = Visibility.Public
var _lobby_max_members:int = 4

# metadata
var _multiplayer_data:MultiplayerData = null
var _lobby_name:String = ""
var _lobby_list:Array[ int ] = []
var _lobby_mapname:String = ""
var _lobby_map:int = 0
var _lobby_gamemode:MultiplayerMode.GameMode = MultiplayerMode.GameMode.Bloodbath
var _lobby_max_players:int = 4 # the default

# filters
var _lobby_filter_map:String = "Any"
var _lobby_filter_gamemode:String = "Any"

func open_lobby_list() -> void:
	if _lobby_filter_map != "Any":
		Steam.addRequestLobbyListStringFilter( "map", _lobby_filter_map, Steam.LobbyComparison.LOBBY_COMPARISON_EQUAL )
	if _lobby_filter_gamemode != "Any":
		Steam.addRequestLobbyListStringFilter( "gamemode", _lobby_filter_gamemode, Steam.LobbyComparison.LOBBY_COMPARISON_EQUAL )
	
	Steam.addRequestLobbyListDistanceFilter( Steam.LobbyDistanceFilter.LOBBY_DISTANCE_FILTER_WORLDWIDE )
	Steam.requestInternetServerList( SteamManager._steam_app_id, [] )
	Steam.requestLobbyList()

func _on_lobby_match_list( lobbies: Array ) -> void:
	_lobby_list.clear()
	for lobby in lobbies:
		_lobby_list.push_back( lobby )

func _cmd_lobby_metadata() -> void:
	Console.print_line( "[LOBBY METADATA]" )
	Console.print_line( "Map: " + _lobby_mapname )
	Console.print_line( "GameMode: " + MultiplayerMode.GameMode.keys()[ _lobby_gamemode ] )
	Console.print_line( "Name: " + _lobby_name )

func _cmd_lobby_list_players() -> void:
	Console.print_line( "[LOBBY MEMBERS]" )
	for member in _lobby_members:
		Console.print_line( "[" + var_to_str( member[ "steam_id" ] ) + "] " + Steam.getFriendPersonaName( member[ "steam_id" ] ) )

func _on_lobby_chat_update( lobbyId: int, changeId: int, playerId: int, chatState: int ) -> void:
	var changerName := Steam.getFriendPersonaName( changeId )
	
	match chatState:
		Steam.CHAT_MEMBER_STATE_CHANGE_ENTERED:
			print( "%s: %s has joined the fray..." % [ SteamManager._steam_username, changerName ] )
			emit_signal( "client_joined_lobby", changeId )
		Steam.CHAT_MEMBER_STATE_CHANGE_LEFT:
			print( "%s: %s has fled..." % [ SteamManager._steam_username, changerName ] )
			emit_signal( "client_left_lobby", changeId )

func join_lobby( lobbyId: int ) -> void:
	_lobby_members.clear()
	Steam.joinLobby( lobbyId )

func leave_lobby() -> void:
	if _lobby_id != 0:
		print( "Leaving lobby %s" % _lobby_id )
		
		Steam.leaveLobby( _lobby_id )
		_lobby_id = 0
		
		for member in _lobby_members:
			var sessionState := Steam.getP2PSessionState( member )
			if sessionState.has( "connection_active" ) && sessionState[ "connection_active" ]:
				Steam.closeP2PSessionWithUser( member )
		
		_lobby_members.clear()
		emit_signal( "client_left_lobby", SteamManager._steam_id )

func _on_lobby_created( connect: int, lobbyId: int ) -> void:
	if connect != 1:
		push_error( "lobby couldn't be created!" )
		return
	
	print( "Created lobby %s" % lobbyId )
	_lobby_id = lobbyId
	
	update_lobby_members()
	
	Steam.setLobbyJoinable( _lobby_id, true )
	Steam.setLobbyData( _lobby_id, "name", _lobby_name )
	Steam.setLobbyData( _lobby_id, "map", var_to_str( _lobby_map ) )
	Steam.setLobbyData( _lobby_id, "gamemode", var_to_str( _lobby_gamemode ) )
	
	var setRelay := Steam.allowP2PPacketRelay( true )
	print( "Relay configuration response: %s" % setRelay )
	
	emit_signal( "lobby_created", lobbyId )

func _on_lobby_joined( lobbyId: int, permissions: int, locked: bool, response: int ) -> void:
	print( "Lobby %s joined." % lobbyId )
	
	_lobby_id = lobbyId
	_lobby_owner_id = Steam.getLobbyOwner( lobbyId )
	update_lobby_members()
	emit_signal( "lobby_joined", lobbyId )

func _on_lobby_data_update( success: bool, lobbyId: int, memberId: int ) -> void:
	if success:
		var host := Steam.getLobbyOwner( _lobby_id )
		if host != _lobby_owner_id && host > 0:
			emit_signal( "lobby_owner_changed", _lobby_owner_id, host )
			_lobby_owner_id = host
		
		emit_signal( "lobby_data_updated", memberId )

func get_lobby_members() -> Array:
	update_lobby_members()
	return _lobby_members

func _on_lobby_message( result: int, senderSteamId: int, message: String, chatType: int ) -> void:
	if result == 0:
		push_error( "Received lobby message, but 0 bytes were sent!" )
	
	match chatType:
		Steam.CHAT_ENTRY_TYPE_CHAT_MSG:
			if !_lobby_members.has( senderSteamId ):
				push_error( "Received a message from a user that ain't here!" )
			
			emit_signal( "chat_message_received", senderSteamId, message )

func _ready() -> void:
	Steam.lobby_chat_update.connect( _on_lobby_chat_update )
	Steam.lobby_created.connect( _on_lobby_created )
	Steam.lobby_match_list.connect( _on_lobby_match_list )
	Steam.lobby_joined.connect( _on_lobby_joined )
	Steam.lobby_data_update.connect( _on_lobby_data_update )
	Steam.lobby_message.connect( _on_lobby_message )
#	Steam.join_requested.connect( _on_lobby_join_requested )
	
	open_lobby_list()
	
	Console.add_command( "lobby.list_players", _cmd_lobby_list_players )
	Console.add_command( "lobby.data", _cmd_lobby_metadata )

#func _process( delta: float ) -> void:
#	if _lobby_id > 0:
#		read_all_p2p_packets()

func create_lobby() -> void:
	if _lobby_id == 0:
		_is_host = true
		
		var lobbyType:Steam.LobbyType
		match _lobby_visibility:
			Visibility.Private:
				lobbyType = Steam.LobbyType.LOBBY_TYPE_PRIVATE
			Visibility.Public:
				lobbyType = Steam.LobbyType.LOBBY_TYPE_PUBLIC
			Visibility.FriendsOnly:
				lobbyType = Steam.LobbyType.LOBBY_TYPE_FRIENDS_ONLY
		
		print( "Initializing SteamLobby..." )
		Steam.createLobby( lobbyType, _lobby_max_members )

func update_lobby_members() -> void:
	_lobby_members.clear()
	
	var memberCount := Steam.getNumLobbyMembers( _lobby_id )
	
	for member in range( 0, memberCount ):
		var memberSteamId := Steam.getLobbyMemberByIndex( _lobby_id, member )
		var memberSteamName := Steam.getFriendPersonaName( memberSteamId )
		
		_lobby_members.append( { "steam_id": memberSteamId, "steam_name": memberSteamName } )
