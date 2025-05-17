func serialize() -> PackedByteArray:
	return var_to_bytes_with_objects( Questify.serialize() )

func deserialize( data: PackedByteArray ) -> void:
	Questify.deserialize( bytes_to_var_with_objects( data ) )
