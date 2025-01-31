extends Node2D

@onready var Blood:PackedScene = preload( "res://scenes/blood.tscn" )
@onready var BloodParticleCount:int = 5

func _enter_tree() -> void:
	var level_name:String = "res://scenes/level" + var_to_str( ArchiveSystem.current_part ) + var_to_str( ArchiveSystem.current_chapter ) + ".tscn"
#	LoadManager.load_scene( level_name )

func _physics_process(_delta: float) -> void:
	for i in range( BloodParticleCount ):
		var blood_instance:Area2D = Blood.instantiate()
		blood_instance.global_position = get_global_mouse_position()
