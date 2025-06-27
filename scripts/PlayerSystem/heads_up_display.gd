class_name HeadsUpDisplay extends CanvasLayer

@export var _owner: CharacterBody2D

const _default_color: Color = Color( 1.0, 1.0, 1.0, 1.0 )
const _hidden_color: Color = Color( 0.0, 0.0, 0.0, 0.0 )
const _arm_used_color: Color = Color( 0.0, 0.0, 1.0, 1.0 )
const _arm_unused_color: Color = Color( 0.20, 0.20, 0.20, 1.0 )

var _fadeout_tween: Tween
var _fadein_tween: Tween

@onready var _notebook: Notebook = $NotebookContainer
@onready var _healthbar: HealthBar = $MainHUD/HealthBar
@onready var _ragebar: RageBar = $MainHUD/RageBar

@onready var _emote_menu: Control = $MainHUD/EmoteMenu

@onready var _announcement_container: MarginContainer = $MainHUD/AnnouncementLabel
@onready var _announcement_background: TextureRect = $MainHUD/AnnouncementLabel/TextureRect
@onready var _announcement_text: Label = $MainHUD/AnnouncementLabel/TextureRect/Label
@onready var _announcement_timer: Timer = $MainHUD/AnnouncementLabel/Timer

@onready var _checkpoint_interactor: MarginContainer = $CheckpointContainer
@onready var _jump_interactor: MarginContainer = $JumpContainer
@onready var _door_interactor: MarginContainer = $DoorContainer

var _current_interactor: MarginContainer

# eagles peak interaction
@onready var _jump_yes_button: Button = $JumpContainer/JumpQueryContainer/VBoxContainer/YesButton
@onready var _jump_no_button: Button = $JumpContainer/JumpQueryContainer/VBoxContainer/NoButton
@onready var _jump_view_image: TextureRect = $JumpContainer/JumpQueryContainer/Background
@onready var _jump_music: AudioStreamPlayer = $JumpContainer/Theme
var _on_yes_pressed: Callable
var _on_no_pressed: Callable

@onready var _left_arm_indicator: TextureRect = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/ArmUsage/LeftArmIndicator
@onready var _right_arm_indicator: TextureRect = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/ArmUsage/RightArmIndicator

@onready var _reflex_overlay: TextureRect = $MainHUD/Overlays/ReflexModeOverlay
@onready var _dash_overlay: TextureRect = $MainHUD/Overlays/DashOverlay
@onready var _parry_overlay: TextureRect = $MainHUD/Overlays/ParryOverlay

@onready var _save_timer: Timer = $MainHUD/SaveSpinner/SaveTimer
@onready var _save_spinner: Spinner = $MainHUD/SaveSpinner/SaveSpinner

var _weapon_data: Node = null
var _weapon_status_timer: Timer = Timer.new()

@onready var _weapon_status: TextureRect = $MainHUD/WeaponStatus
@onready var _weapon_mode_bladed: TextureRect = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBladed
@onready var _weapon_mode_blunt: TextureRect = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBlunt
@onready var _weapon_mode_firearm: TextureRect = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/StatusContainer/StatusFirearm

@onready var _weapon_status_firearm: VBoxContainer = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/FireArmStatus
@onready var _weapon_status_melee: VBoxContainer = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MeleeStatus
@onready var _weapon_status_melee_icon: TextureRect = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/MeleeStatus/WeaponIcon
@onready var _weapon_status_firearm_icon: TextureRect = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/FireArmStatus/WeaponIcon
@onready var _weapon_status_bullet_count: Label = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletCountLabel
@onready var _weapon_status_bullet_reserve: Label = $MainHUD/WeaponStatus/MarginContainer/VBoxContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletReserveLabel

@onready var _interaction_data

@onready var _open_door_button: Button = $DoorContainer/MarginContainer/OpenButton
@onready var _use_door_button: Button = $DoorContainer/MarginContainer/UseButton

@onready var _boss_health_bar: Control = $MainHUD/BossHealthBar

@onready var _world_time_year: Label = $MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer/YearLabel
@onready var _world_time_month: Label = $MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer/MonthLabel
@onready var _world_time_day: Label = $MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/DayLabel
@onready var _world_time_hour: Label = $MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/HourLabel
@onready var _world_time_minute: Label = $MainHUD/WorldTimeContainer/VBoxContainer/HBoxContainer2/MinuteLabel

@onready var _interact_prompt: RichTextLabel = $MainHUD/InteractPrompt
var _interact_resource = preload( "res://resources/binds/actions/keyboard/interact.tres" )

@onready var _location_label: Label = $MainHUD/LocationLabel
var _location_status_timer:Timer = Timer.new()

@onready var _objective_label: Label = $MainHUD/ObjectiveLabel
var _objective_status_timer: Timer = Timer.new()

var _status_icons: Dictionary[String, TextureRect]

@onready var _hellbreaker_overlay: Control = $MainHUD/HellbreakerOverlay

enum HandsUsed {
	Left,
	Right,
	Both
};

enum WeaponProperties {
	IsOneHanded			= 0b01000000,
	IsTwoHanded			= 0b00100000,
	IsBladed			= 0b00000001,
	IsBlunt				= 0b00000010,
	IsFirearm			= 0b00001000,

	OneHandedBlade		= IsOneHanded | IsBladed,
	OneHandedBlunt		= IsOneHanded | IsBlunt,
	OneHandedFirearm	= IsOneHanded | IsFirearm,

	TwoHandedBlade		= IsTwoHanded | IsBladed,
	TwoHandedBlunt		= IsTwoHanded | IsBlunt,
	TwoHandedFirearm	= IsTwoHanded | IsFirearm,

	SpawnsObject		= 0b10000000,

	None				= 0b00000000
};

enum InteractionType {
	Checkpoint,
	TreasureChest,
	Tutorial,
	Note,
	Door,

	EaglesPeak,

	Count
};

enum DoorState {
	Locked,
	Unlocked,

	Count
};

func GetCurrentCheckpoint() -> Area2D:
	return _checkpoint_interactor.GetCurrentCheckpoint()

func GetHealthBar() -> ProgressBar:
	return _healthbar

func GetRageBar() -> ProgressBar:
	return _ragebar

func GetParryOverlay() -> TextureRect:
	return _parry_overlay

func SetRage( rage: float ) -> void:
	_ragebar.value = rage

func SaveStart() -> void:
	_save_spinner.set_process( true )
	_save_spinner.show()

func SaveEnd() -> void:
	_save_timer.start()

func _on_save_timer_timeout() -> void:
	_save_spinner.hide()
	_save_spinner.set_process( false )

func _on_use_door_transition_finished() -> void:
	get_node( "/root/TransitionScreen" ).disconnect( "transition_finished", _on_use_door_transition_finished )
	
	_owner.global_position = _interaction_data.GetDestination().global_position
	_owner.BlockInput( false )

func _on_use_door_button_pressed() -> void:
	_owner.BlockInput( true )
	get_node( "/root/TransitionScreen" ).connect( "transition_finished", _on_use_door_transition_finished )
	get_node( "/root/TransitionScreen" ).call( "transition" )
	_owner.PlaySound( null, load( "res://sounds/env/open_door_" + var_to_str( randi_range( 0, 2 ) ) + ".ogg" ) )
	
	_door_interactor.hide()

func _on_hands_status_updated( handsUsed: int ) -> void:
	_weapon_status.modulate = _default_color
	_weapon_status_timer.start()
	
	match handsUsed:
		HandsUsed.Left:
			_left_arm_indicator.modulate = _arm_used_color
			_right_arm_indicator.modulate = _arm_unused_color
		HandsUsed.Right:
			_left_arm_indicator.modulate = _arm_unused_color
			_right_arm_indicator.modulate = _arm_used_color
		HandsUsed.Both:
			_left_arm_indicator.modulate = _arm_used_color
			_right_arm_indicator.modulate = _arm_used_color

func _on_weapon_status_updated( source: Node, properties: int ) -> void:
	if source == null:
		_weapon_data = null
		
		_weapon_status.modulate = _hidden_color
		return
	if source != _weapon_data:
		return
	
	_weapon_status.modulate = _default_color
	_weapon_status_timer.start()
	
	_weapon_mode_bladed.visible = source.LastUsedMode & WeaponProperties.IsBladed
	_weapon_mode_blunt.visible = source.LastUsedMode & WeaponProperties.IsBlunt
	
	if source.LastUsedMode & WeaponProperties.IsFirearm:
		_weapon_mode_firearm.show()
		_weapon_status_bullet_count.text = var_to_str( source.BulletsLeft )
		if source.Reserve != null:
			_weapon_status_bullet_reserve.text = var_to_str( source.Reserve.Amount )
	else:
		_weapon_mode_firearm.hide()

func _on_announcement_timer_timeout() -> void:
	create_tween().tween_property( _announcement_background.material, "shader_parameter/alpha", 0.0, 1.0 )
	create_tween().tween_property( _announcement_text, "modulate", _hidden_color, 1.0 )

func _fade_ui_element( element: Control, duration: float, timer: Timer ) -> void:
	create_tween().tween_property( element, "modulate", _hidden_color, duration )
	timer.process_mode = PROCESS_MODE_DISABLED

func _on_location_changed( location: Area2D ) -> void:
	_location_status_timer.process_mode = PROCESS_MODE_PAUSABLE
	
	_location_label.text = location.GetAreaName()
	
	var _tweener: Tween = create_tween()
	_tweener.tween_property( _location_label, "modulate", _default_color, 1.5 )
	_tweener.connect( "finished", func(): _location_status_timer.start() )

func _on_weapon_reloaded( source: Node) -> void:
	_weapon_status.modulate = _default_color
	_weapon_status_timer.process_mode = PROCESS_MODE_PAUSABLE
	_weapon_status_timer.start()
	
	_weapon_status_bullet_count.text = var_to_str( source.BulletsLeft )
	_weapon_status_bullet_reserve.text = var_to_str( source.Reserve.Amount ) if source.Reserve != null else "0"

func _on_weapon_used( source: Node ) -> void:
	_weapon_status.modulate = _default_color
	_weapon_status_timer.process_mode = PROCESS_MODE_PAUSABLE
	_weapon_status_timer.start()
	
	if source.LastUsedMode & WeaponProperties.IsFirearm:
		_weapon_status_bullet_count.text = var_to_str( source.BulletsLeft )

func _on_switched_weapon( weapon: Node ) -> void:
	if _weapon_data != weapon && _weapon_data != null:
		_weapon_data.disconnect( "Reloaded", _on_weapon_reloaded )
		_weapon_data.disconnect( "Used", _on_weapon_used )
	
	if weapon == null:
		_weapon_status.modulate = _hidden_color
		_weapon_status.process_mode = PROCESS_MODE_DISABLED
		return
	else:
		_weapon_status.process_mode = PROCESS_MODE_PAUSABLE
	
	_weapon_status.modulate = _default_color
	_weapon_status_timer.process_mode = PROCESS_MODE_PAUSABLE
	_weapon_status_timer.start()
	
	if weapon.LastUsedMode & WeaponProperties.IsFirearm:
		_weapon_status_firearm.show()
		_weapon_status_melee.hide()
		
		_weapon_status_firearm_icon.texture = weapon.Icon
		
		_weapon_status_bullet_count.text = var_to_str( weapon.BulletsLeft )
		_weapon_status_bullet_reserve.text = var_to_str( weapon.Reserve.Amount ) if weapon.Reserve != null else "0"
	else:
		_weapon_status_firearm.hide()
		_weapon_status_melee.show()
		_weapon_status_melee_icon.texture = weapon.Icon
	
	_weapon_data = weapon
	_weapon_data.connect( "Reloaded", _on_weapon_reloaded )
	_weapon_data.connect( "Used", _on_weapon_used )

func EndParry() -> void:
	_parry_overlay.hide()

func _ready() -> void:
	_weapon_status_timer.name = "WeaponStatusTimer"
	_weapon_status_timer.wait_time = 5.90
	_weapon_status_timer.one_shot = true
	_weapon_status_timer.process_mode = PROCESS_MODE_DISABLED
	_weapon_status_timer.connect( "timeout", func(): _fade_ui_element( _weapon_status, 1.5, _weapon_status_timer ) )
	add_child( _weapon_status_timer )
	
	_location_status_timer.name = "LocationStatusTimer"
	_location_status_timer.wait_time = 5.90
	_location_status_timer.one_shot = true
	_location_status_timer.process_mode = PROCESS_MODE_DISABLED
	_location_status_timer.connect( "timeout", func(): _fade_ui_element( _location_label, 2.5, _location_status_timer ) )
	add_child( _location_status_timer )
	
	_objective_status_timer.name = "ObjectiveStatusTimer"
	_objective_status_timer.wait_time = 5.90
	_objective_status_timer.one_shot = true
	_objective_status_timer.process_mode = PROCESS_MODE_DISABLED
	_objective_status_timer.connect( "timeout", func(): _fade_ui_element( _objective_label, 2.5, _objective_status_timer ) )
	add_child( _objective_status_timer )
	
	_owner.connect( "Interaction", func(): _interact_prompt.parse_bbcode( AccessibilityManager.GetBindString( _interact_resource ) ) )
	_owner.connect( "DashStart", func(): _dash_overlay.show() )
	_owner.connect( "DashEnd", func(): _dash_overlay.hide() )
	_owner.connect( "BulletTimeStart", func(): _reflex_overlay.show() )
	_owner.connect( "BulletTimeEnd", func(): _reflex_overlay.hide() )
	_owner.connect( "HideInteraction", func(): HideInteraction() )
	_owner.connect( "ShowInteraction", func( interaction: Area2D ): ShowInteraction( interaction ) )
	_owner.connect( "InventoryToggled", func(): OnShowInventory() )
	_owner.connect( "ParrySuccess", func(): _parry_overlay.show() )
	_owner.connect( "HealthChanged", func( health: float ): _healthbar.SetHealth( health ) )
	_owner.connect( "RageChanged", func( rage: float ): _ragebar.Rage = rage )
	_owner.connect( "LocationChanged", _on_location_changed )
	_owner.connect( "Damaged", func( source: CharacterBody2D, target: CharacterBody2D, amount: float ): _healthbar.SetHealth( target.GetHealth() ) )
	_owner.connect( "SwitchedWeapon", _on_switched_weapon )
	_owner.connect( "HandsStatusUpdated", _on_hands_status_updated )
	_owner.connect( "WeaponStatusUpdated", _on_weapon_status_updated )

func OnShowInventory() -> void:
	if _notebook.visible:
		_notebook.visible = false
		_notebook.process_mode = PROCESS_MODE_DISABLED
		$MainHUD.mouse_filter = Control.MOUSE_FILTER_IGNORE
		return
	
	$MainHUD.mouse_filter = Control.MOUSE_FILTER_STOP
	
	_notebook.process_mode = PROCESS_MODE_PAUSABLE
	_notebook.visible = true
	_notebook.on_show_backpack()

func ShowInteraction( item: Area2D ) -> void:
	match item.GetInteractionType():
		InteractionType.Checkpoint:
			_checkpoint_interactor.BeginInteraction( item )
			_current_interactor = _checkpoint_interactor
		InteractionType.Door:
			_current_interactor = _door_interactor
	
	if _current_interactor != null:
		_current_interactor.show()
	else:
		return
	
	Input.set_custom_mouse_cursor( load( "res://cursor_n.png" ) )
	
	_interaction_data = item
	
	if _current_interactor == _door_interactor:
		match item.GetState():
			DoorState.Locked:
				_open_door_button.show()
				_use_door_button.hide()
			DoorState.Unlocked:
				_open_door_button.hide()
				_use_door_button.show()

func HideInteraction() -> void:
	if _current_interactor == _checkpoint_interactor:
		_checkpoint_interactor.EndInteraction()
	
	if _current_interactor != null:
		_current_interactor.hide()
	
	_current_interactor = null
	Input.set_custom_mouse_cursor( load( "res://textures/hud/crosshairs/crosshairi.tga" ), Input.CURSOR_ARROW )

func ShowAnnouncment( text: String ) -> void:
	_announcement_container.show()
	
	_announcement_timer.start()
	
	_announcement_background.material.set( "shader_parameter/alpha", 0.0 )
	_announcement_text.text = text
	
	_announcement_timer.start()
	
	create_tween().tween_property( _announcement_background.material, "shader_parameter/alpha", 0.90, 0.90 )
	create_tween().tween_property( _announcement_text, "modulate", Color( 1.0, 1.0, 1.0, 1.0 ), 0.90 )

func IsNotebookOpen() -> bool:
	return _notebook.visible

func _unhandled_input( event: InputEvent ) -> void:
	if Input.is_action_just_pressed( "open_emote_menu" ):
		_emote_menu.show()
		_emote_menu.set_process_input( true )
		_emote_menu.process_mode = PROCESS_MODE_ALWAYS
	if Input.is_action_just_released( "open_emote_menu" ):
		_emote_menu.hide()
		_emote_menu.set_process_input( false )
		_emote_menu.process_mode = PROCESS_MODE_DISABLED
