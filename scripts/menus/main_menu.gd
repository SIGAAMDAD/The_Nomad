#
# ===========================================================================
# The Nomad AGPL Source Code
# Copyright (C) 2025 Noah Van Til
#
# The Nomad Source Code is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as published
# by the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# The Nomad Source Code is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
#
# You should have received a copy of the GNU Affero General Public License
# along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.
#
# If you have questions concerning this license or the applicable additional
# terms, you may contact me via email at nyvantil@gmail.com.
# ===========================================================================
#

class_name MainMenu extends Control

enum IndexedButton {
	ContinueGame,
	NewGame,
	LoadGame,
	Extras,
	Settings,
	Mods,
	Credits,
	Quit,

	Count
};

@onready var _continue_game_button: Button = $VBoxContainer/ContinueGameButton
@onready var _load_game_button: Button = $VBoxContainer/LoadGameButton
@onready var _new_game_button: Button = $VBoxContainer/NewGameButton
@onready var _tutorials_popup: ConfirmationDialog = $TutorialsPopup

var _loaded: bool = false
var _button_index: IndexedButton = IndexedButton.ContinueGame
var _button_list: Array[ Button ]

signal save_slots_menu
signal settings_menu
signal help_menu
signal extras_menu
signal mods_menu
signal credits_menu

signal begin_game

func _on_continue_game_finished() -> void:
	get_node( "/root/TransitionScreen" ).disconnect( "transition_finished", _on_continue_game_finished )

	hide()
	get_node( "/root/LoadingScreen" ).FadeIn( "res://levels/world.tscn", func(): ArchiveSystem.LoadGame( SettingsData.GetSaveSlot() ) )

	Console.PrintLine( "Loading game..." )

func _on_button_focused( _index: IndexedButton ) -> void:
	UiAudioManager.OnButtonFocused()
	_button_index = _index
	_button_list[ _button_index ].grab_focus()

func _ready() -> void:
	PhysicsServer2D.set_active( false )

	var _has_save_data: bool = DirAccess.dir_exists_absolute( ProjectSettings.globalize_path( "user://SaveData" ) )

	UiAudioManager.PlayTheme()
	_loaded = false
	
	MultiplayerMapManager.Init()
	ArchiveSystem.CheckSaveData()

	theme = AccessibilityManager.DyslexiaTheme if SettingsData.GetDyslexiaMode() else AccessibilityManager.DefaultTheme
	process_mode = Node.PROCESS_MODE_ALWAYS

	if _has_save_data:
		_continue_game_button.connect( "focus_entered", func(): _on_button_focused( IndexedButton.ContinueGame ) )
		_continue_game_button.connect( "mouse_entered", func(): _on_button_focused( IndexedButton.ContinueGame ) )
		_continue_game_button.connect( "pressed", func(): emit_signal( "save_slots_menu" ) )
	else:
		_continue_game_button.modulate = Color( 0.50, 0.50, 0.50, 0.75 )