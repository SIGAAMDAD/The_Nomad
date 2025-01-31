extends Label


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if ArchiveSystem._save_datas[ 2 ].exists:
		text = "SAVE SLOT 3";
	else:
		text = "EMPTY";


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
