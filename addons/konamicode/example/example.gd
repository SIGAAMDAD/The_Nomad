extends Control

@onready var status_label: Label = %StatusLabel

func _on_konami_code_success() -> void:
	status_label.text = "success!"
	status_label.label_settings.font_color = Color.DARK_GREEN
