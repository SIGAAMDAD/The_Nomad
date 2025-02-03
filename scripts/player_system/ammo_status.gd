extends Label

var _bullet_count:int = 4
var _magazine_size:int = 40

func set_bullet_count( count: int ) -> void:
	_bullet_count = count

func set_magazine_size( size: int ) -> void:
	_magazine_size = size

func _process( delta: float ) -> void:
	var color := Color.WHITE
	var ratio := _bullet_count / _magazine_size
	if ratio <= 0.25:
		color = Color( 1.0, 0.0, 0.0, 1.0 )
	
	set( "modulate", color )
