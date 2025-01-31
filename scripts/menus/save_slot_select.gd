extends Control

signal set_difficulty_select_menu

func next_menu( slot: int ) -> void:
	ArchiveSystem.set_slot( slot )
	
#	if ArchiveSystem.slot_exists( slot ):
#		ArchiveSystem.load_game()
#	else:
	emit_signal( "set_difficulty_select_menu" )

func _on_save_slot_0_button_pressed() -> void:
	next_menu( 0 )

func _on_save_slot_1_button_pressed() -> void:
	next_menu( 1 )

func _on_save_slot_2_button_pressed() -> void:
	next_menu( 2 )
