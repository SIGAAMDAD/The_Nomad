extends Control

@onready var _campaign_button:Button = $VBoxContainer/CampaignButton

signal campaign_menu
signal settings_menu
signal valdens_book
signal multiplayer_menu
signal tales_around_the_campfire

func _on_campaign_button_pressed() -> void:
	emit_signal( "campaign_menu" )

func _on_settings_button_pressed() -> void:
	emit_signal( "settings_menu" )

func _on_quit_game_button_pressed() -> void:
	get_tree().quit()

func _on_multiplayer_button_pressed() -> void:
	emit_signal( "multiplayer_menu" )
