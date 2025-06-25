class_name CheckpointInteractor extends MarginContainer

class WarpPoint extends HBoxContainer:
	var _icon: TextureRect
	var _confirm_button: Button
	var _biome_label: Label
	
	func _ready() -> void:
		_icon = get_node( "Icon" )
		_confirm_button = get_node( "ConfirmButton" )
		_biome_label = get_node( "BiomeLabel" )

const _rage_used_on_warp = 20.0

var _owner: CharacterBody2D
var _current_checkpoint: Area2D

@onready var _checkpoint_name_label: Label = $VBoxContainer/CheckpointNameLabel

@onready var _warp_locations_scroll: VScrollBar = $VBoxContainer/MarginContainer/WarpLocations
@onready var _warp_locations_container: VBoxContainer = $VBoxContainer/MarginContainer/WarpLocations/WarpLocationsContainer
@onready var _warp_cloner: HBoxContainer = $VBoxContainer/MarginContainer/WarpLocations/WarpLocationsContainer/Cloner

@onready var _inactive_container: VBoxContainer = $VBoxContainer/MarginContainer/InactiveContainer
@onready var _checkpoint_maincontainer: VBoxContainer = $VBoxContainer/MarginContainer/MainContainer
@onready var _resting_container: VBoxContainer = $VBoxContainer/MarginContainer/RestingContainer

func _on_activate_button_pressed() -> void:
	pass

func _on_leave_button_pressed() -> void:
	hide()
	
	_owner.LeaveCampfire()
	
	_checkpoint_maincontainer.show()
	_warp_locations_container.hide()
	_resting_container.hide()

func _on_warp_to_checkpoint( warpPoint: WarpPoint ) -> void:
	pass
#	if _owner.GetRage() - _rage_used_on_warp < 0.0:
#		HeadsUpDisplay.start_thought_bubble(  )

func load_warp_points() -> void:
	var checkpoints: Array[Node] = get_tree().get_nodes_in_group( "Checkpoints" )
	for i: int in range( 0, checkpoints.size() ):
		if !checkpoints[ i ].GetActivated():
			continue
		
		var _warp_point: WarpPoint = _warp_cloner.duplicate() as HBoxContainer
		_warp_locations_container.add_child( _warp_point )
		_warp_point._confirm_button.text = checkpoints[ i ].GetTitle()
		_warp_point._confirm_button.connect( "pressed", func(): _on_warp_to_checkpoint( _warp_point ) )
		_warp_point._biome_label.text = checkpoints[ i ].GetLocation().GetBiome().GetAreaName()
		_warp_point.set_meta( "Checkpoint", checkpoints[ i ] )
		_warp_point.show()

func _on_open_storage_button_pressed() -> void:
	pass

func _on_warp_button_pressed() -> void:
	_checkpoint_maincontainer.hide()
	_resting_container.hide()
	
	for i: int in range( 1, _warp_locations_container.get_child_count() ):
		_warp_locations_container.call_deferred( "remove_child", _warp_locations_container.get_child( i ) )
		_warp_locations_container.get_child( i ).call_deferred( "queue_free" )
	
	call_deferred( "load_warp_points" )
	
	_warp_locations_scroll.call_deferred( "show" )

func BeginInteraction( item: Area2D ) -> void:
	_current_checkpoint = item
	if _current_checkpoint == null:
		Console.PrintError( "CheckpointInteractor.BeginInteraction: invalid checkpoint!" )
		return
	
	_checkpoint_name_label.text = _current_checkpoint.GetTitle()
	if _current_checkpoint.GetActivated():
		_inactive_container.hide()
		_checkpoint_maincontainer.show()
	else:
		_inactive_container.show()
		_checkpoint_maincontainer.hide()

func EndInteraction() -> void:
	pass
