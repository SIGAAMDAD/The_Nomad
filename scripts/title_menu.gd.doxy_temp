class_name TitleMenu extends Control

enum MenuState {
	Main,
	SaveSlots,
	Extras,
	Settings,
	Help,
	Credits,
	Mods
};

var _extras_menu: Control
var _settings_menu: SettingsMenu
var _story_menu: Control
var _credits_menu: Control
var _main_menu: Control
var _exit_button: Button

var _state: MenuState = MenuState.Main

func free_menu( _menu: Control ) -> void:
	var _index: int = _menu.get_index()
	remove_child( _menu )
	_menu.queue_free()
	_menu = null

	_main_menu = ResourceCache.GetScene( "res://scenes/menus/main_main.tscn" ).instantiate()
	
	add_child( _main_menu )
	move_child( _main_menu, _index )

	_exit_button.hide()
	_main_menu.show()
	_state = MenuState.Main

func set_menu( _scene: String, _menu_state: MenuState ) -> Control:
	var _menu: Control = ResourceCache.GetScene( _scene ).instantiate()

	var _index: int = _main_menu.get_index()
	remove_child( _main_menu )
	add_child( _menu )
	move_child( _menu, _index )

	_main_menu.queue_free()

	_exit_button.show()
	_state = _menu_state

	return _menu

func _on_exit_button_pressed() -> void:
	UiAudioManager.OnButtonPressed()

	match _state:
		MenuState.SaveSlots:
			free_menu( _story_menu )
		MenuState.Extras:
			free_menu( _extras_menu )
		MenuState.Settings:
			free_menu( _settings_menu )
		MenuState.Credits:
			free_menu( _credits_menu )

func _on_main_menu_extras_menu() -> void:
	_extras_menu = set_menu( "res://scenes/menus/extras_menu.tscn", MenuState.Extras )

func _on_main_menu_settings_menu() -> void:
	_settings_menu = set_menu( "res;//scenes/menus/settings_menu.tscn", MenuState.Settings )

func _on_main_menu_save_slots_menu() -> void:
	_story_menu = set_menu( "res://scenes/menus/save_slots_menu.tscn", MenuState.SaveSlots )

func _on_konamic_code_activated() -> void:
	Console.PrintLine( "========== Meme Mode Activated ==========" )
	UiAudioManager.PlayCustomSound( ResourceCache.GetSound( "res://sounds/ui/meme_mode_activated.ogg" ) )
	SteamAchievements.ActivateAchievement( "ACH_DNA_OF_THE_SOUL" );

func _ready() -> void:
	PhysicsServer2D.set_active( false )

	add_child( ResourceCache.GetScene( "res://scenes/menus/menu_background.tscn" ).instantiate() )

	var _konami_code: Node = get_node( "KonamiCode" )
	_konami_code.connect( "activated", _on_konamic_code_activated )

	_main_menu = get_node( "MainMenu" )
	_main_menu.connect( "save_slots_menu", _on_main_menu_save_slots_menu )
	_main_menu.connect( "settings_menu", _on_main_menu_settings_menu )
	_main_menu.connect( "extras_menu", _on_main_menu_extras_menu )

	_exit_button = get_node( "ExitButton" )
	_exit_button.connect( "focus_entered", UiAudioManager.OnButtonFocused )
	_exit_button.connect( "mouse_entered", UiAudioManager.OnButtonFocused )
	_exit_button.connect( "pressed", _on_exit_button_pressed )

	get_tree().current_scene = self

	set_process( false )
	set_physics_process( false )