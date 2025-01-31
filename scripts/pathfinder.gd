class_name PathfindingManager extends Node

const PATH_COST:String = "path_cost"

var astar_grid:AStarGrid2D = AStarGrid2D.new()
var astar:AStar2D = AStar2D.new()
var path_array:Array[Vector2i] = []

@export var tile_map_layer:TileMapLayer = null

func _ready() -> void:
	tile_map_layer = get_parent()
	
	setup_grid()
	set_terrain_movement_cost()

func setup_grid() -> void:
	astar_grid.region = tile_map_layer.get_viewport_rect()
	astar_grid.cell_size = tile_map_layer.tile_set.tile_size
	astar_grid.diagonal_mode = AStarGrid2D.DIAGONAL_MODE_ONLY_IF_NO_OBSTACLES
	astar_grid.default_compute_heuristic = AStarGrid2D.HEURISTIC_EUCLIDEAN
	astar_grid.update()

func set_terrain_movement_cost() -> void:
	for cell_position in tile_map_layer.get_used_cells():
		var cell_data := tile_map_layer.get_cell_tile_data( cell_position )
		var cell_path_cost:int = cell_data.get_custom_data( PATH_COST )
		
		astar_grid.set_point_weight_scale( cell_position, 1.0 )

func get_valid_path( start: Vector2i, end: Vector2i ) -> Array[Vector2i]:
	var array:Array[Vector2i]
	for point in astar_grid.get_point_path( start, end ):
		var current_point:Vector2i = point
		current_point += tile_map_layer.tile_set.tile_size / 2
		
		array.append( current_point )
	
	return array
