extends Control

@onready var _chat:TextEdit = $TextEdit
@onready var _message:LineEdit = $LineEdit

var _text_buffer:Array[ String ]

func _on_chat_message_recieved( sendor: int, message: String ) -> void:
	var username := Steam.getFriendPersonaName( sendor )
	_chat.text += str( "[", username, "] %s\n" % message )

func _ready() -> void:
	_chat.hide()

func _process( _delta: float ) -> void:
	if Input.is_action_just_pressed( "chat_open" ):
		_chat.show()
		_message.editable = true
	if Input.is_action_just_pressed( "chat_send" ):
		_message.editable = false
		SteamLobby.send_message( _message.text )
