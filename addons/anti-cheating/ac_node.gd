extends Node
class_name ac_node

var disturb: int = 0:
	set(v):
		_dict.disturb = v
	get:
		return _dict.disturb

var _dict: ac_dict = ac_dict.new()

func _init(parent: Node = null):
	if parent:
		parent.add_child(self)

# Set value of a key
func set_value(key: String, value: ac_value):
	_dict.set_value(key, value)

# Get value by key with an optional default
func get_value(key: String, default_value: ac_value = ac_value.new()) -> ac_value:
	return _dict.get_value(key, default_value)

# Adds a dictionary with specific key-value pair assignments
func assimilate(dictionary: Dictionary):
	for key in dictionary.keys():
		if dictionary.has(key):  # Ensure the key still exists
			var value = dictionary[key]
			var new_ac_val: ac_value
			if value is String:
				if is_json(value):
					if typeof(JSON.parse_string(value)) == TYPE_DICTIONARY or typeof(JSON.parse_string(value)) == TYPE_ARRAY:
						new_ac_val = ac_list.new(JSON.parse_string(value), null, null)  # No parent, no parent_key
					else:
						push_error("Parsed JSON for key '%s' is not a dictionary or array: %s" % [key, JSON.parse_string(value)])
						continue
				else:
					new_ac_val = ac_string.new(value)
			elif value is int:
				new_ac_val = ac_int.new(value)
			elif value is float:
				new_ac_val = ac_float.new(value)
			else:
				push_error("Unsupported value type for key '%s': %s" % [key, typeof(value)])
				continue
			_dict.set_value(key, new_ac_val)

# Additional utility functions for managing the dictionary
func has(key: String) -> bool:
	return _dict.has(key)

func keys() -> Array[String]:
	return _dict.keys()

func erase(key: String) -> bool:
	return _dict.erase(key)

func clear():
	_dict.clear()

func is_empty() -> bool:
	return _dict.is_empty()

func is_json(value: String) -> bool:
	var json = JSON.new()
	return json.parse(value) == OK
