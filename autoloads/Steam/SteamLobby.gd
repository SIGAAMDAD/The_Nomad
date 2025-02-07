extends Node

const _PACKET_READ_LIMIT:int = 32

enum Visibility {
	Private,
	Public,
	FriendsOnly
};

signal on_chat_message_recieved( sendor: int, message: String )
signal on_data_recieved( sendor: int, data: Dictionary )

var _is_host:bool = false
var _lobby_id:int = 0
var _lobby_members:Array = []
var _lobby_visibility:Visibility = Visibility.Public
var _lobby_max_members:int = 4

var _lobby_name:String = ""
var _lobby_list:Array[ int ] = []
var _matchmaking_phase:int = 0

# filters
var _lobby_filter_map:String = "Any"
var _lobby_filter_gamemode:String = "Any"

func matchmaking_loop() -> void:
	if _matchmaking_phase < 4:
		Steam.addRequestLobbyListDistanceFilter( _matchmaking_phase )
		
		# get the list
		Steam.requestLobbyList()

func open_lobby_list() -> void:
	if _lobby_filter_map != "Any":
		Steam.addRequestLobbyListStringFilter( "map", _lobby_filter_map, Steam.LobbyComparison.LOBBY_COMPARISON_EQUAL )
	if _lobby_filter_gamemode != "Any":
		Steam.addRequestLobbyListStringFilter( "gamemode", _lobby_filter_gamemode, Steam.LobbyComparison.LOBBY_COMPARISON_EQUAL )
	Steam.addRequestLobbyListDistanceFilter( Steam.LobbyDistanceFilter.LOBBY_DISTANCE_FILTER_WORLDWIDE )
	Steam.requestInternetServerList( SteamManager._steam_app_id, [] )
	Steam.requestLobbyList()

func _on_lobby_match_list( lobbies ) -> void:
	for lobby in lobbies:
		_lobby_list.push_back( lobby )

func _ready() -> void:
	Steam.lobby_created.connect( _on_lobby_create )
	Steam.lobby_joined.connect( _on_lobby_joined )
	Steam.lobby_match_list.connect( _on_lobby_match_list )
	Steam.p2p_session_request.connect( _on_p2p_session_request )
	open_lobby_list()

func _process( delta: float ) -> void:
	if _lobby_id > 0:
		read_all_p2p_packets()

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
		
		Steam.createLobby( lobbyType, _lobby_max_members )

func _on_lobby_create( connect: int, lobbyId: int ) -> void:
	if connect != 1:
		push_error( "lobby couldn't be created!" )
		return
	
	print( "Created lobby %s" % lobbyId )
	_lobby_id = lobbyId
	Steam.setLobbyJoinable( _lobby_id, true )
	Steam.setLobbyData( _lobby_id, "name", _lobby_name )
	
	var setRelay := Steam.allowP2PPacketRelay( true )

func _on_lobby_joined( lobbyId: int, permissions: int, locked: bool, response: int ) -> void:
	if response == Steam.CHAT_ROOM_ENTER_RESPONSE_SUCCESS:
		_lobby_id = lobbyId

func get_lobby_members() -> void:
	_lobby_members.clear()
	
	var memberCount := Steam.getNumLobbyMembers( _lobby_id )
	
	for member in range( 0, memberCount ):
		var memberSteamId := Steam.getLobbyMemberByIndex( _lobby_id, member )
		var memberSteamName := Steam.getFriendPersonaName( memberSteamId )
		
		_lobby_members.append( { "steam_id": memberSteamId, "steam_name": memberSteamName } )

func send_message( text: String ) -> void:
	var packetData:Dictionary = {
		"message": "chat",
		"steam_id": SteamManager._steam_id,
		"username": SteamManager._steam_username,
		"data": text
	};
	
	print( "Sending chat message..." )
	for member in _lobby_members:
		if member[ "steam_id" ] != SteamManager._steam_id:
			send_p2p_packet( member[ "steam_id" ], packetData )

func send_p2p_packet( target: int, packetData: Dictionary, sendType: int = 0 ) -> void:
	var channel := 0
	var data:PackedByteArray
	data.append_array( var_to_bytes( packetData ) )
	
	if target == 0:
		if _lobby_members.size() > 1:
			for member in _lobby_members:
				if member[ "steam_id" ] != SteamManager._steam_id:
					Steam.sendP2PPacket( member[ "steam_id" ], data, sendType, channel )
	else:
		Steam.sendP2PPacket( target, data, sendType, channel )

func _on_p2p_session_request( remoteId: int ) -> void:
	var requester:String = Steam.getFriendPersonaName( remoteId )
	Steam.acceptP2PSessionWithUser( remoteId )

func make_p2p_handshake() -> void:
	send_p2p_packet( 0, { "message": "handshake", "steam_id": SteamManager._steam_id, "username": SteamManager._steam_username } )

func read_all_p2p_packets( readCount: int = 0 ) -> void:
	if readCount >= _PACKET_READ_LIMIT:
		return
	
	if Steam.getAvailableP2PPacketSize( 0 ) > 0:
		read_p2p_packet()
		read_all_p2p_packets( readCount )

func read_p2p_packet() -> void:
	var packetSize := Steam.getAvailableP2PPacketSize( 0 )
	
	if packetSize > 0:
		var packet := Steam.readP2PPacket( packetSize, 0 )
		var packetSender:int = packet[ "steam_id" ]
		var data:Dictionary = bytes_to_var( packet[ "data" ] )
		
		match data[ "message" ]:
			"data":
				emit_signal( "on_data_recieved", packetSender, data[ "data" ] )
			"chat":
				emit_signal( "on_chat_message_recieved", packetSender, data[ "text" ] )
			"handshake":
				print( "" )
				get_lobby_members()
