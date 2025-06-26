class_name CheckpointInteractor extends MarginContainer

var _owner: CharacterBody2D
var _active_callback: Callable

@onready var _activate_button: Button = $VBoxContainer/MarginContainer/InactiveContainer/ActivateButton
@onready var _checkpoint_name_label: Label = $VBoxContainer/CheckpointNameLabel
@onready var _main_interactor: VBoxContainer = $VBoxContainer

@onready var _inactive_container: VBoxContainer = $VBoxContainer/MarginContainer/InactiveContainer
@onready var _checkpoint_maincontainer: VBoxContainer = $VBoxContainer/MarginContainer/MainContainer
@onready var _resting_container: VBoxContainer = $VBoxContainer/MarginContainer/RestingContainer

@onready var _warp_container: MarginContainer = $"../WarpContainer"

func _on_activate_button_pressed( checkpoint: Area2D ) -> void:
	UiAudioManager.OnButtonPressed()
	
	checkpoint.Activate()
	get_parent().ShowAnnouncment( "ACQUIRED_MEMORY" )
	
	_inactive_container.hide()
	_checkpoint_maincontainer.show()

func _on_leave_button_pressed() -> void:
	UiAudioManager.OnButtonPressed()
	
	hide()
	
	_owner.LeaveCampfire()
	
	_checkpoint_maincontainer.show()
	_warp_container.hide()
	_resting_container.hide()

func _on_open_storage_button_pressed() -> void:
	UiAudioManager.OnButtonPressed()
	
	hide()
	$"../StorageContainer".show()

func _on_rest_here_button_pressed() -> void:
	UiAudioManager.OnButtonPressed()
	
	_owner.SetHealth( 100.0 )
	_owner.SetRage( 100.0 )
	_owner.RestAtCampfire()
	
	_checkpoint_maincontainer.hide()
	_resting_container.show()
	
	ArchiveSystem.SaveGame( SettingsData.GetSaveSlot() )

func _on_warp_button_pressed() -> void:
	UiAudioManager.OnButtonPressed()
	
	_checkpoint_maincontainer.hide()
	_resting_container.hide()
	
	_warp_container.show()
	hide()

func BeginInteraction( item: Area2D ) -> void:
	var _current_checkpoint: Area2D = item
	if _current_checkpoint == null:
		Console.PrintError( "CheckpointInteractor.BeginInteraction: invalid checkpoint!" )
		return
	
	if _active_callback:
		_activate_button.disconnect( "pressed", _active_callback )
	
	_active_callback = func(): _on_activate_button_pressed( _current_checkpoint )
	_activate_button.connect( "pressed", _active_callback )
	
	_checkpoint_name_label.text = _current_checkpoint.GetTitle()
	if _current_checkpoint.GetActivated():
		_inactive_container.hide()
		_checkpoint_maincontainer.show()
	else:
		_inactive_container.show()
		_checkpoint_maincontainer.hide()

func EndInteraction() -> void:
	pass

func _ready() -> void:
	_owner = get_parent()._owner
	
	_main_interactor.connect( "visibility_changed", func(): get_node( "../StorageContainer" ).visible = !_main_interactor.visible )
