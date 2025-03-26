class_name TileMapFloor extends Node2D

@export var _floor:TileMapLayer = null
@export var _decor:TileMapLayer = null
@export var _area:Area2D = null

@export var _interior_layers:Array[ TileMapFloor ] = []
@export var _upper_layer:TileMapFloor = null
@export var _lower_layer:TileMapFloor = null

@export var _is_exterior:bool = false
@export var _show_exterior_on_exit:bool = false
@export var _hide_exterior_on_enter:bool = false
@export var _exterior:TileMapLayer = null

var _player_here:bool = false

func _ready() -> void:
	_area.body_shape_entered.connect( _on_area_2d_body_shape_entered )
	_area.body_shape_exited.connect( _on_area_2d_body_shape_exited )

func _on_area_2d_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	print( "entered tilemapfloor " + name )
	if _upper_layer:
		if _is_exterior && !_upper_layer._player_here:
			_upper_layer.hide()
		else:
			_upper_layer.show()
	if _lower_layer:
		if _is_exterior:
			_lower_layer.hide()
		else:
			_lower_layer.show()
	
	if _is_exterior:
		for layer in _interior_layers:
			layer.hide()
	
	show()
	
	_player_here = true

func _on_area_2d_body_shape_exited( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	print( "left tilemapfloor " + name )
	
	if !_is_exterior:
		hide()
	
	if _upper_layer && !_player_here:
		_upper_layer.hide()
	if _lower_layer:
		if _is_exterior:
			_lower_layer.hide()
		else:
			_lower_layer.show()
	
	_player_here = false
