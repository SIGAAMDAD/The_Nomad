var _mapping_contexts: Array[ GUIDEMappingContext ] = [
	preload( "res://resources/binds/binds_keyboard.tres" ),
	preload( "res://resources/binds/binds_gamepad.tres" ),
	preload( "res://resources/binds/binds_ui.tres" )
]
var _remapper: GUIDERemapper = null
var _remapping_config: GUIDERemappingConfig = null
var _remappable_items: Array[ GUIDERemapper.ConfigItem ] = []
var _mapping_formatter: GUIDEInputFormatter = GUIDEInputFormatter.new()

func _init() -> void:
	_remapper = GUIDERemapper.new()
	_remapping_config = load( "user://input_context.tres" )
	if !_remapping_config:
		_remapping_config = GUIDERemappingConfig.new()
	
	Console.PrintLine( "Initializing remapper..." )
	_remapper.initialize( _mapping_contexts, _remapping_config )
	_remappable_items = _remapper.get_remappable_items()
