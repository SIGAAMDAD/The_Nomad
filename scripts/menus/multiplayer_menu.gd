extends Control

@onready var _lobby_browser:Control = $LobbyBrowser
@onready var _lobby_factory:Control = $LobbyFactory

func _on_lobby_browser_host_game_pressed() -> void:
	_lobby_browser.hide()
	_lobby_factory.show()

func _ready() -> void:
	_lobby_browser.on_host_game.connect( _on_lobby_browser_host_game_pressed )
