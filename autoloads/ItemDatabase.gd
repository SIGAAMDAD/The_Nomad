class_name ItemManager extends Node

@onready var _database:InventoryDatabase = preload( "res://resources/ItemDatabase.tres" )

func get_item( id: String ) -> ItemDefinition:
	return _database.get_item( id )
