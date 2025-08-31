extends Control

@onready var _interaction_container: BlacksmithInteractor = $BlacksmithContainer
@onready var _upgrades_container: MarginContainer = $BlacksmithUpgradeContainer

func _ready() -> void:
	_interaction_container.connect( "blacksmith_show_upgrades", _upgrades_container.show )
	_interaction_container.connect( "blacksmith_talk",  )
	_interaction_container.connect( "blacksmith_exit", hide )
