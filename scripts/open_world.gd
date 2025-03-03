class_name OpenWorld extends Node2D

@export var _biomes_cache:Array[ PackedScene ] = []

var _biomes_loaded:Array[ Biome ] = []

func _ready() -> void:
	for biome in _biomes_cache:
		_biomes_loaded.push_back( biome.instantiate() )
