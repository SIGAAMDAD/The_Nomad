extends Control

@onready var _save_slot_select:Control = $SaveSlotSelect
@onready var _difficulty_select:Control = $DifficultySelect

func _on_save_slot_select_set_difficulty_select_menu() -> void:
	_save_slot_select.hide()
	_difficulty_select.set_meme_mode_name()
	_difficulty_select.show()
