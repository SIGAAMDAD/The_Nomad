class_name WarpContainer extends MarginContainer

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

@onready var _warp_locations_scroll: VScrollBar = $WarpLocations
@onready var _warp_locations_container: VBoxContainer = $WarpLocations/WarpLocationsContainer
@onready var _warp_cloner: HBoxContainer = $WarpLocations/WarpLocationsContainer/Cloner

@onready var _main_container: MarginContainer = $"../CheckpointContainer"

func _on_back_button_pressed() -> void:
	hide()
	_main_container.show()

func _on_warp_to_checkpoint( warpPoint: WarpPoint ) -> void:
	if _owner.GetRage() - _rage_used_on_warp < 0.0:
		_owner.ThoughtBubble( "You: Shit, don't have enough mana" )
		return
	elif warpPoint.get_meta( "checkpoint" ) == _owner.GetCurrentCheckpoint():
		_owner.ThoughtBubble( "You: I'm already here..." )
		return
	
	# TODO: play warp animation
	_owner.SetRage( _owner.GetRage() - _rage_used_on_warp )
	TransitionScreen.transition()
	
	_owner.global_position = warpPoint.get_meta( "checkpoint" ).global_position

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
		_warp_point.set_meta( "checkpoint", checkpoints[ i ] )
		_warp_point.show()

func _on_show() -> void:
	for i: int in range( 1, _warp_locations_container.get_child_count() ):
		_warp_locations_container.call_deferred( "remove_child", _warp_locations_container.get_child( i ) )
		_warp_locations_container.get_child( i ).call_deferred( "queue_free" )
	
	call_deferred( "load_warp_points" )
	
	_warp_locations_scroll.call_deferred( "show" )

func _ready() -> void:
	_owner = get_parent()._owner
	
	connect( "visibility_changed", _on_show )
