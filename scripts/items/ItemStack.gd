class_name ItemStac extends Node

var _item_type:String = ""
var _item_count:int = 0

func size() -> int:
	return _item_count

func type() -> String:
	return _item_type

func num_items() -> int:
	return _item_count

func push_item() -> void:
	_item_count += 1

func pop_item() -> bool:
	if _item_count == 0:
		return false
	
	_item_count -= 1
	return true

func remove_items( numItems: int ) -> int:
	return _item_count if _item_count - numItems < 0 else numItems

func add_items( numItems: int ) -> void:
	_item_count += numItems

func set_item_type( name: String ) -> void:
	_item_type = name

func init( item = null ) -> void:
	if !item:
		_item_type = "null"
	else:
		_item_type = item._data._name
	_item_count = 0
