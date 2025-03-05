extends NavigationRegion2D

func synchronize() -> void:
	await get_tree().physics_frame

func _ready() -> void:
	call_deferred( "synchronize" )
