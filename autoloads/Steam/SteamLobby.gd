extends Node

const _PACKET_READ_LIMIT:int = 32

enum Visibility {
	Private,
	Public,
	FriendsOnly
};

signal chat_message_received( senderSteamId: int, message: String )

signal client_left_lobby( steamId: int )
signal client_joined_lobby( steamId: int )
signal lobby_joined( lobbyId: int )
signal lobby_created( lobbyId: int )
signal lobby_data_updated()
signal lobby_owner_changed( formerOwnerId: int, newOwnerId: int )

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

func _on_lobby_created( connect: int, lobbyId: int ) -> void:
	if connect != 1:
		return
	
	print( "Created lobby %s." % lobbyId )
	_lobby_id = lobbyId
	_is_host = true
	
	Steam.setLobbyJoinable( lobbyId, true )
	Steam.setLobbyData( lobbyId, "name", _lobby_name )
	Steam.setLobbyData( lobbyId, "map", var_to_str( _lobby_map ) )
	Steam.setLobbyData( lobbyId, "gamemode", var_to_str( _lobby_gamemode ) )
	
	var setRelay := Steam.allowP2PPacketRelay( true )
	if !setRelay:
		push_error( "[STEAM] couldn't enable p2p packet relay!" )
	
	lobby_created.emit( lobbyId )

func _on_lobby_joined( lobbyId: int, permissions: int, locked: bool, response: int ) -> void:
	if response == Steam.CHAT_ROOM_ENTER_RESPONSE_SUCCESS:
		_lobby_id = lobbyId
		_lobby_owner_id = Steam.getLobbyOwner( lobbyId )
		
		print( "Joined lobby %s." % lobbyId )
		get_lobby_members()
		make_p2p_handshake()
		
		lobby_joined.emit( lobbyId )

func get_lobby_members() -> void:
	_lobby_members.clear()
	
	var lobbyMemberCount := Steam.getNumLobbyMembers( _lobby_id )
	
	for member in range( 0, lobbyMemberCount ):
		var steamId := Steam.getLobbyMemberByIndex( _lobby_id, member )
		var username := Steam.getFriendPersonaName( steamId )
		
		_lobby_members.push_back( { "steam_id": steamId, "name": username } )

func leave_lobby() -> void:
	if _lobby_id == 0:
		return
	
	print( "Leaving lobby %s..." % _lobby_id )
	Steam.leaveLobby( _lobby_id )
	_lobby_id = 0
	
	for member in _lobby_members:
		var sessionState := Steam.getP2PSessionState( member )
		if sessionState.has( "connection_active" ) && sessionState[ "connection_active" ]:
			Steam.closeP2PSessionWithUser( member )
	
	_lobby_members.clear()
	
	client_left_lobby.emit( SteamManager._steam_idclietn )

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

func join_lobby( lobbyId: int ) -> void:
	Steam.joinLobby( lobbyId )

func send_p2p_packet( target: int, packet: Dictionary, sendType: int = 0 ) -> void:
	var channel := 0
	
	var data:PackedByteArray = var_to_bytes( packet )
	
	# send to everyone
	if target == 0:
		if _lobby_members.size() > 1:
			for member in _lobby_members:
				if member[ "steam_id" ] != SteamManager._steam_id:
					Steam.sendP2PPacket( member[ "steam_id" ], data, sendType, channel )
	else:
		Steam.sendP2PPacket( target, data, sendType, channel )

func read_p2p_packet() -> void:
	var packetSize := Steam.getAvailableP2PPacketSize( 0 )
	if packetSize > 0:
		var packet := Steam.readP2PPacket( packetSize, 0 )
		var senderId:int = packet[ "remote_steam_id" ]
		var data:Dictionary = bytes_to_var( packet[ "data" ] )
		
		match data[ "message" ]:
			"handshake":
				print( "%s joined the fray" % data[ "username" ] )
				get_lobby_members()

func read_all_packets( readCount: int = 0 ) -> void:
	if readCount >= _PACKET_READ_LIMIT:
		return
	
	if Steam.getAvailableP2PPacketSize( 0 ) > 0:
		read_p2p_packet()
		read_all_packets( readCount + 1 )

func _on_p2p_session_request( remoteId: int ) -> void:
	var requester := Steam.getFriendPersonaName( remoteId )
	
	Steam.acceptP2PSessionWithUser( remoteId )

func _on_lobby_data_update( success: int, lobbyId: int, memberId: int ) -> void:
	if success:
		var host := Steam.getLobbyOwner( _lobby_id )
		if host != _lobby_owner_id && host > 0:
			lobby_owner_changed.emit( _lobby_owner_id, host )
			_lobby_owner_id = host
		
		print( "Lobby data updated..." )
		lobby_data_updated.emit()

func _on_lobby_message( result: int, senderSteamId: int, message: String, chatType: int ) -> void:
	if result == 0:
		push_error( "Received lobby message, but not characters were processed!" )
		return
	match chatType:
		Steam.CHAT_ENTRY_TYPE_CHAT_MSG:
			if !_lobby_members.has( senderSteamId ):
				push_error( "Received a message from a user that ain't here!" )
			
			chat_message_received.emit( senderSteamId, message )

func _on_lobby_chat_update( lobbyId: int, changeId: int, playerId: int, chatState: int ) -> void:
	var changerName := Steam.getFriendPersonaName( changeId )
	
	match chatState:
		Steam.CHAT_MEMBER_STATE_CHANGE_ENTERED:
			print( "%s: %s has joined the fray..." % [ SteamManager._steam_username, changerName ] )
			client_joined_lobby.emit( changeId )
		Steam.CHAT_MEMBER_STATE_CHANGE_LEFT:
			print( "%s: %s has fled..." % [ SteamManager._steam_username, changerName ] )
			client_left_lobby.emit( changeId )

func make_p2p_handshake() -> void:
	send_p2p_packet( 0, { "message": "handshake", "remote_steam_id": SteamManager._steam_id, "username": SteamManager._steam_username } )

func _ready() -> void:
	Steam.lobby_chat_update.connect( _on_lobby_chat_update )
	Steam.lobby_created.connect( _on_lobby_created )
	Steam.lobby_match_list.connect( _on_lobby_match_list )
	Steam.lobby_joined.connect( _on_lobby_joined )
	Steam.p2p_session_request.connect( _on_p2p_session_request )
	Steam.lobby_data_update.connect( _on_lobby_data_update )
	Steam.lobby_message.connect( _on_lobby_message )
	
	open_lobby_list()
	
	Console.add_command( "lobby.list_players", _cmd_lobby_list_players )
	Console.add_command( "lobby.data", _cmd_lobby_metadata )

func _process( _delta: float ) -> void:
	if _lobby_id > 0:
		read_all_packets()
