extends AnimatedSprite2D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	var screenSize = DisplayServer.window_get_size();
	var mousePosition = get_viewport().get_mouse_position();
	
	if mousePosition.x > screenSize.x / 2:
		flip_h = false;
	elif mousePosition.x < screenSize.x / 2:
		flip_h = true;
