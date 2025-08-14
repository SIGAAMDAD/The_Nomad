extends CanvasLayer

func _ready() -> void:
	visible = get_tree().root.get_node( "/root/SettingsData" ).GetPerformanceOverlay() == 1
	process_mode = Node.PROCESS_MODE_ALWAYS if visible else Node.PROCESS_MODE_DISABLED

func _process( _delta: float ) -> void:
	if ( Engine.get_process_frames() % 30 ) != 0:
		return
	
	$Label.text = var_to_str( Engine.get_frames_per_second() )
