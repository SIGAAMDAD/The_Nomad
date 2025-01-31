extends Area2D

@onready var _surface_image:Image
@onready var _blood_image:Image = Image.new()

@onready var _blood_size:Vector2 = Vector2( 0, 0 )

func _ready() -> void:
	_surface_image = Image.create( 2000, 2000, false, Image.Format.FORMAT_RGBA8 )
	_surface_image.fill( Color( 0, 0, 0, 0 ) )
	
	_blood_image.convert( Image.FORMAT_RGBA8 )
	_blood_image.load( "res://textures/blood1.png" )
	_blood_size = _blood_image.get_size()
