extends CanvasLayer

func _process( _delta: float ) -> void:
	if ( Engine.get_process_frames() % 30 ) != 0:
		return
	
	$Label.text = var_to_str( Engine.get_frames_per_second() )
