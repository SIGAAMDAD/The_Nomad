class_name SaveField
extends Node

enum FieldType {
	Int,
	Byte,
	Bool,
	Vec2,
	Vec3,
	Color,
	String,
	Float,
	Buffer,
	Object,
};

var _name:String
var _type:FieldType = FieldType.Int
var _value

func save( name: String, value, file: FileAccess ) -> void:
	file.store_pascal_string( name )
	file.store_var( value )

func load( file:FileAccess ) -> void:
	self._name = file.get_pascal_string()
	self._type = file.get_32()
	
	match self._type:
		FieldType.Int:
			_value = file.get_32()
		FieldType.Vec2:
			var vec:Vector2
			vec.x = file.get_float()
			vec.y = file.get_float()
			_value = vec
		FieldType.Vec3:
			var vec:Vector3
			vec.x = file.get_float()
			vec.y = file.get_float()
			vec.z = file.get_float()
			_value = vec
		FieldType.String:
			_value = file.get_pascal_string()
		FieldType.Float:
			_value = file.get_float()
		FieldType.Object:
			_value = file.get_var( true )
		FieldType.Buffer:
			var length := file.get_32() # fetch the length
			_value = file.get_buffer( length )
		_:
			print( "Invalid save field type ", self._type, ", corruption or incompatible version?" )
