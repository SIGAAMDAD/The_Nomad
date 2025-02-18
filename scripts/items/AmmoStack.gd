class_name AmmoStack extends ItemStack

var _ammo_type:ItemDefinition = null
var _player:Player = null

func set_type( ammo: AmmoEntity ) -> void:
	_ammo_type = ammo._data
	item_id = _ammo_type.id

func push_item() -> void:
	amount += 1

func pop_item() -> bool:
	if amount == 0:
		return false
	
	amount -= 1
	return true

func remove_items( numItems: int ) -> int:
	if amount - numItems < 0:
		var tmp := amount
		amount = 0
		return tmp
	
	amount -= numItems
	return numItems

func add_items( numItems: int ) -> void:
	amount += numItems
