class_name KillFeed extends Control

@onready var _feed:VBoxContainer = $MarginContainer/VBoxContainer
@onready var _cloner:HBoxContainer = $MarginContainer/VBoxContainer/Cloner

func _on_killfeed_item_timeout( item: HBoxContainer ) -> void:
	_feed.remove_child( item )
	item.queue_free()

func push( source: CharacterBody2D, weaponIcon: Texture2D, target: CharacterBody2D ) -> void:
	var data:HBoxContainer = _cloner.duplicate()
	
	data.show()
	data.get_child( 0 ).text = target._multiplayer_username
	data.get_child( 1 ).texture = weaponIcon
	if source == null:
		data.get_child( 2 ).hide()
	else:
		data.get_child( 2 ).text = source._multiplayer_username
	data.get_child( 3 ).timeout.connect( Callable( _on_killfeed_item_timeout ).bind( data ) )
