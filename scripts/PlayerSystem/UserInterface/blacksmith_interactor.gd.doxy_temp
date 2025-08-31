class_name BlacksmithInteractor extends MarginContainer

signal blacksmith_show_upgrades
signal blacksmith_show_repairs
signal blacksmith_talk
signal blacksmith_exit

func _on_upgrade_button_pressed() -> void:
	emit_signal( "blacksmith_show_upgrades" )

func _on_repair_button_pressed() -> void:
	emit_signal( "blacksmith_show_repairs")

func _on_talk_button_pressed() -> void:
	emit_signal( "blacksmith_talk")

func _on_leave_button_pressed() -> void:
	emit_signal( "blacksmith_exit" )
