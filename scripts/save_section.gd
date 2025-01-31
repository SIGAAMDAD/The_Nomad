class_name SaveSection extends Node

var _name:String = ""

# meant for saving
var _saving:bool = false
var _save_position:int = 0
var _field_count:int = 0
var _save_file:FileAccess = null

var _field_list:Array[ SaveField ]

func get_field( name: String ) -> SaveField:
	for field in _field_list:
		if field._name == name:
			return field
	
	push_warning( "SaveField \"", name, "\" not found in SaveSection \"", _name, "\"" )
	return null

func save( name: String, file: FileAccess ) -> void:
	print( "Saving section ", name )
	
	# placeholder header
	file.store_pascal_string( name )
	
	self._save_position = file.get_position()
	file.store_32( 0 )
	
	self._name = name
	self._saving = true
	self._save_file = file

func flush() -> void:
	if _saving:
		return
	
	var now := _save_file.get_position()
	_save_file.seek( _save_position )
	_save_file.store_32( _field_count )
	_save_file.seek( now )

func load( file: FileAccess ) -> void:
	var fieldCount := file.get_32()
	
	self._name = file.get_pascal_string()
	print( "Loading save section ", self._name, "..." )
	
	for i in fieldCount:
		var field := SaveField.new()
		field.load( file )
		_field_list.push_back( field )

func load_var( name: String ):
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func load_buffer( name: String ) -> PackedByteArray:
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func load_bool( name: String ) -> bool:
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func load_byte( name: String ) -> int:
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func load_int( name: String ) -> int:
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func load_float( name: String ) -> float:
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func load_string( name: String ) -> String:
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func load_vector2( name: String ) -> Vector2:
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func load_vector3( name: String ) -> Vector3:
	var field := get_field( _save_file.get_pascal_string() )
	return field._value

func save_var( name: String, value ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.Object )
	_save_file.store_var( value, true )

func save_buffer( name: String, value: PackedByteArray ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.Buffer )
	_save_file.store_32( value.size() )
	_save_file.store_buffer( value )

func save_bool( name: String, value: bool ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.Int )
	_save_file.store_8( value )

func save_byte( name: String, value: int ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.Byte )
	_save_file.store_8( value )

func save_int( name: String, value: int ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.Int )
	_save_file.store_32( value )

func save_float( name: String, value: float ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.Float )
	_save_file.store_float( value )

func save_string( name: String, value: String ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.String )
	_save_file.store_pascal_string( value )

func save_vector2( name: String, value: Vector2 ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.Vec2 )
	_save_file.store_float( value.x )
	_save_file.store_float( value.y )

func save_vector3( name: String, value: Vector3 ) -> void:
	_save_file.store_pascal_string( name )
	_save_file.store_32( SaveField.FieldType.Vec3 )
	_save_file.store_float( value.x )
	_save_file.store_float( value.y )
	_save_file.store_float( value.z )
