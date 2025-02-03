extends Button

func _ready() -> void:
	if ArchiveSystem.slot_exists( 0 ):
		text = "SLOT 0"
	else:
		text = "UNUSED_SLOT"
