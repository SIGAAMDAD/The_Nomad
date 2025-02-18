class_name ComplexTilemap extends Node2D

@export var _floors:Array[ TileMapFloor ] = []
@export var _exterior:TileMapLayer = null
@export var _quest:QuestResource = null

func _ready() -> void:
	Questify.start_quest( _quest.instantiate() )

func _physics_process( delta: float ) -> void:
	pass

func hide_all() -> void:
	for floor in _floors:
		floor._floor.hide()
		floor._decor.hide()
	
	_exterior.hide()
