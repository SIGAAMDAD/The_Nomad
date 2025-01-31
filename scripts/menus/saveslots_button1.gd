extends Button


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if ArchiveSystem._save_datas[ 1 ].exists:
		text = "CONTINUE";
	else:
		text = "START";

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
