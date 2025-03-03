extends Control

@onready var _campaign_menu:Control = $"../../"
@onready var _loading_screen:Resource = load( "res://scenes/menus/loading_screen.tscn" )
@onready var _new_loading_screen:Node

@onready var _meme_mode_name_list:Array = [
	"POV: Kazuma",
	"Dark Souls",
	"Writing in C++",
	"Metal Goose Rising: REVENGEANCE",
	"Hell Itself",
	"Suicidal Encouragement",
	"Cope & Seethe, Repeat",
	"Sounds LIke a U Problem",
	"GIT GUD",
	"THE MEMES",
	"Deal With It",
	"Just A Minor Inconvenience",
	"YOU vs God",
	"The Ultimate Bitch-Slap",
	"GIT REKT",
	"GET PWNED",
	"Wish U Had A BFG?",
	"Skill Issue",
	"DAKKA",
	"OOOOF",
	"So sad, Too bad",
	"Actual Living Hell",
	"RNJesus Hates You",
	"I AM THE DANGER",
	"Awwww Does This Make U Cry?",
	"Asian"
];

#TODO: asian dad skin

@onready var _difficulty_description:Label = $DifficultyDescriptionLabel

@onready var _meme_mode_button:Button = $VBoxContainer/MemeModeButton
@onready var _intended_experience_button:Button = $VBoxContainer/IntendedModeButton
@onready var _power_fantasy_button:Button = $VBoxContainer/PowerFantasyModeButton

#var _num_meme_mode_names:int = 0

#FIXME:
#func set_strings() -> void:
#	_easy_mode_description = StringManager.StringTable[ "MENU_EASY_MODE_DESCRIPTION" ]
#	_normal_mode_description = StringManager.StringTable[ "MENU_NORMAL_MODE_DESCRIPTION" ]
#	_hard_mode_description = StringManager.StringTable[ "MENU_HARD_MODE_DESCRIPTION" ]
#	_very_hard_mode_description = StringManager.StringTable[ "MENU_VERY_HARD_MODE_DESCRIPTION" ]
#	_insane_mode_description = StringManager.StringTable[ "MENU_INSANE_MODE_DESCRIPTION" ]
#	_meme_mode_description = StringManager.StringTable[ "MENU_MEME_MODE_DESCRIPTION" ]
	
#	_easy_mode_name = StringManager.StringTable[ "MENU_EASY_MODE_NAME" ]
#	_normal_mode_name = StringManager.StringTable[ "MENU_NORMAL_MODE_NAME" ]
#	_hard_mode_name = StringManager.StringTable[ "MENU_HARD_MODE_NAME" ]
#	_very_hard_mode_name = StringManager.StringTable[ "MENU_VERY_HARD_MODE_NAME" ]
#	_insane_mode_name = StringManager.StringTable[ "MENU_INSANE_MODE_NAME" ]
	
#	while true:
#		if StringManager.StringTable.has( "MENU_MEME_MODE_NAME_" + var_to_str( _num_meme_mode_names ) ):
#			_num_meme_mode_names += 1
#		else:
#			break

func _on_intended_mode_button_pressed() -> void:
	begin_level( GameConfiguration.GameDifficulty.Intended )

func _on_intended_mode_button_mouse_entered() -> void:
	_difficulty_description.text = "INTENDED_MODE_DESCRIPTION"

func _on_power_fantasy_mode_button_pressed() -> void:
	begin_level( GameConfiguration.GameDifficulty.PowerFantasy )

func _on_power_fantasy_mode_button_mouse_entered() -> void:
	_difficulty_description.text = "POWER_FANTASY_MODE_DESCRIPTION"

func _ready() -> void:
	_intended_experience_button.pressed.connect( _on_intended_mode_button_pressed )
	_intended_experience_button.mouse_entered.connect( _on_intended_mode_button_mouse_entered )
	_power_fantasy_button.pressed.connect( _on_power_fantasy_mode_button_pressed )
	_power_fantasy_button.mouse_entered.connect( _on_power_fantasy_mode_button_mouse_entered )

func set_meme_mode_name() -> void:
	# change the name every time we enter the tree
	# for that extra comedic effect
	_meme_mode_button.text = _meme_mode_name_list[ randi_range( 0, _meme_mode_name_list.size() - 1 ) ]

func _on_easy_mode_button_mouse_entered() -> void:
	_difficulty_description.text = "EASY_MODE_DESCRIPTION"

func _on_normal_mode_button_mouse_entered() -> void:
	_difficulty_description.text = "NORMAL_MODE_DESCRIPTION"

func _on_hard_mode_button_mouse_entered() -> void:
	_difficulty_description.text = "HARD_MODE_DESCRIPTION"

func _on_very_hard_mode_button_mouse_entered() -> void:
	_difficulty_description.text = "VERY_HARD_MODE_DESCRIPTION"

func _on_insane_mode_button_mouse_entered() -> void:
	_difficulty_description.text = "INSANE_MODE_DESCRIPTION"

func _on_meme_mode_button_mouse_entered() -> void:
	_difficulty_description.text = "MEME_MODE_DESCRIPTION"

func on_finish_loading() -> void:
	GameConfiguration.LoadedLevel.ChangeScene()
	_new_loading_screen.hide()
	_new_loading_screen.queue_free()
	SoundManager.stop_music( 1.5 )
	self.hide()

func begin_level( difficulty: GameConfiguration.GameDifficulty ) -> void:
	GameConfiguration._game_difficulty = difficulty
	
	var playerList := Input.get_connected_joypads()
	
	var levelName := "res://levels/level" + var_to_str( ArchiveSystem._current_part ) + var_to_str( ArchiveSystem._current_chapter )
	self.hide()
	_new_loading_screen = _loading_screen.instantiate()
	get_tree().get_root().add_child( _new_loading_screen )
	
	if playerList.size() < 2:
		# we either have someone with a controller hooked up,
		# or we're just running solo
		print( "Loading level ", levelName + "_sp.tscn", "..." )
		GameConfiguration.LoadedLevel = AsyncScene.new( levelName + "_sp.tscn", AsyncScene.LoadingSceneOperation.Replace )
	elif playerList.size() == 2:
		print( "Loading level ", levelName + "_2p.tscn", "..." )
		GameConfiguration.LoadedLevel = AsyncScene.new( levelName + "_2p.tscn", AsyncScene.LoadingSceneOperation.Replace )
	
	GameConfiguration.LoadedLevel.OnComplete.connect( on_finish_loading )
