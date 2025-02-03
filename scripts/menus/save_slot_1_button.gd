extends Button

func _ready() -> void:
	if ArchiveSystem.slot_exists( 1 ):
		text = "SLOT 1"
	else:
		text = "UNUSED_SLOT"
