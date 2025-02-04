extends Control

@onready var _campaign_menu:Control = $CampaignMenu
@onready var _multiplayer_menu:Control = $MultiplayerMenu
@onready var _settings_menu:Control = $SettingsMenu
@onready var _main_menu:Control = $MainMenu
@onready var _exit_button:Control = $ExitButton

@onready var _save_slot_select:Control = $CampaignMenu/SaveSlotSelect
@onready var _difficulty_select:Control = $CampaignMenu/DifficultySelect

@onready var _lobby_browser:Control = $MultiplayerMenu/LobbyBrowser
@onready var _lobby_factory:Control = $MultiplayerMenu/LobbyFactory

enum MenuState {
	Main,
	Campaign,
	Multiplayer,
	Settings,
	Help,
	Mods
};

var _menu_state:MenuState = MenuState.Main

func _ready() -> void:
	SoundManager.play_music( ResourceLoader.load( "res://music/ui/main.ogg" ) )

func _on_exit_button_pressed() -> void:
	match _menu_state:
		MenuState.Campaign:
			_campaign_menu.hide()
			_save_slot_select.hide()
			_difficulty_select.hide()
		MenuState.Multiplayer:
			_multiplayer_menu.hide()
			_lobby_browser.hide()
			_lobby_factory.hide()
		MenuState.Settings:
			_settings_menu.hide()
		_:
			push_error( "Invalid menu state!" )

	_exit_button.hide()
	_main_menu.show()
	_menu_state = MenuState.Main


func _on_main_menu_campaign_menu() -> void:
	_main_menu.hide()
	_campaign_menu.show()
	_save_slot_select.show()
	_exit_button.show()
	_menu_state = MenuState.Campaign

func _on_main_menu_multiplayer_menu() -> void:
	_main_menu.hide()
	_multiplayer_menu.show()
	_exit_button.show()
	_lobby_browser.show()
	_menu_state = MenuState.Multiplayer

func _on_main_menu_settings_menu() -> void:
	_main_menu.hide()
	_settings_menu.show()
	_exit_button.show()
	_menu_state = MenuState.Settings
