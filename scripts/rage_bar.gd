extends ProgressBar

var rage = 0.0 : set = _set_rage

func _set_rage( _rage: float ) -> void:
	rage = _rage
	value = rage

func init( _rage ):
	rage = _rage
	max_value = _rage
	value = _rage
