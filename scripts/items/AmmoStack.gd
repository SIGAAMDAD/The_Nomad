class_name AmmoStack extends ItemStack

func push_item() -> void:
	amount += 1

func pop_item() -> bool:
	if amount == 0:
		return false
	
	amount -= 1
	return true

func remove_items( numItems: int ) -> int:
	return amount if amount - numItems < 0 else numItems

func add_items( numItems: int ) -> void:
	amount += numItems
