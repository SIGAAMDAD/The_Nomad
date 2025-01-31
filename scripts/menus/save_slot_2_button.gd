extends Button

func _ready() -> void:
	if ArchiveSystem.slot_exists( 2 ):
		text = "SLOT 2"
	else:
		text = "UNUSED"
