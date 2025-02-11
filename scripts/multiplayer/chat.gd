extends Control

@onready var _chat:TextEdit = $TextEdit
@onready var _message:LineEdit = $LineEdit

var _text_buffer:Array[ String ]

func _on_chat_message_received( sendor: int, message: String ) -> void:
	print( "got message" )
	var username := Steam.getFriendPersonaName( sendor )
	Console.print_line( "[%s] %s\n" % [ username, message ] )
	_chat.text += str( "[%s] %s\n" % [ username, message ] )

func _ready() -> void:
	print( "Connecting" )
	SteamLobby.chat_message_received.connect( _on_chat_message_received )
	_chat.hide()

func _process( _delta: float ) -> void:
	if Input.is_action_just_pressed( "chat_open" ):
		if _message.editable:
			_chat.hide()
		else:
			_chat.show()
		
		_message.editable = !_message.editable
	
	if Input.is_action_just_pressed( "chat_send" ) && _message.editable:
		_message.editable = false
		Steam.sendLobbyChatMsg( SteamManager._steam_id, _message.text )
		_message.clear()
