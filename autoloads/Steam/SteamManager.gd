extends Node

const _VIP_ID:int = 76561199403850315

var _steam_app_id:int = 3512240
var _steam_id:int = 0
var _steam_username:String = ""
var _is_me:bool = false

var _auth_ticket:Dictionary = {}
var _client_auth_tickets:Array = []

func _on_get_auth_session_ticket_response( authTicket: int, result: int ) -> void:
	print( "Auth session result: %s" % result )

func _on_validate_auth_ticket_response( authId: int, response: int, ownerId: int ) -> void:
	print( "Ticket Owner: %s" % authId )
	
	var verboseResponse:String
	match response:
		0: verboseResponse = "Steam has verified the user is online, the ticket is valid and ticket has not been reused."
		1: verboseResponse = "The user in question is not connected to Steam."
		2: verboseResponse = "The user doesn't have a license for this App ID or the ticket has expired."
		3: verboseResponse = "The user is VAC banned for this game."
		4: verboseResponse = "The user account has logged in elsewhere and the session containing the game instance has been disconnected."
		5: verboseResponse = "VAC has been unable to perform anti-cheat checks on this user."
		6: verboseResponse = "The ticket has been canceled by the issuer."
		7: verboseResponse = "This ticket has already been used, it is not valid."
		8: verboseResponse = "This ticket is not from a user instance currently connected to steam."
		9: verboseResponse = "The user is banned for this game. The ban came via the Web API and not VAC."
	
	print( "Auth response: %s" % verboseResponse )
	print( "Game owner ID: %s" % ownerId )

func _ready() -> void:
	if Engine.has_singleton( "Steam" ):
		OS.set_environment( "SteamAppID", str( _steam_app_id ) )
		OS.set_environment( "SteamGameID", str( _steam_app_id ) )
		var status := Steam.steamInitEx( true, _steam_app_id )
		print( "SteamInit status: ", status )
		
		var isRunning = Steam.isSteamRunning()
		if !isRunning:
			push_error( "Steam isn't running!" )
		
		_steam_id = Steam.getSteamID()
		_steam_username = Steam.getFriendPersonaName( _steam_id )
		
		SteamAchievements.init()
		
		if _steam_id == _VIP_ID:
			_is_me = true
			print( "'ello" )
		
		print( "SteamAPI initialized with username ", _steam_username )
		
		Steam.get_auth_session_ticket_response.connect( _on_get_auth_session_ticket_response )
		Steam.validate_auth_ticket_response.connect( _on_validate_auth_ticket_response )
		
		_auth_ticket = Steam.getAuthSessionTicket()
	else:
		_steam_id = 0
		_steam_username = ""

func _process( _delta: float ) -> void:
	Steam.run_callbacks()

func save_cloud_file( path: String ) -> void:
	if !Steam.isCloudEnabledForAccount() || !Steam.isCloudEnabledForApp():
		print( "Steam cloud isn't enabled for this application or the account" )
	
	var file := FileAccess.open( "user://" + path, FileAccess.READ )
	if !file:
		push_error( "Error opening file \"%s\" in read mode!" % path )
	
	file.seek_end()
	var length := file.get_position()
	file.seek( 0 )
	var data:PackedByteArray = file.get_buffer( length )
	
	print( "Saving file \"%s\" to Steam Cloud..." % path )
	Steam.fileWrite( path, data, length )
