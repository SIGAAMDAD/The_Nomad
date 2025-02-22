extends CanvasLayer

@onready var _image:TextureRect = $Panel/TextureRect

@onready var _image_cache:Array[Resource] = [
	ResourceLoader.load( "res://textures/art/IMG_1709.JPG" ),
	ResourceLoader.load( "res://textures/art/IMG_2189.JPG" ),
	ResourceLoader.load( "res://textures/art/IMG_2190.jpg" ),
	ResourceLoader.load( "res://textures/art/IMG_2251.JPG" ),
	ResourceLoader.load( "res://textures/art/IMG_2412.JPG" ),
	ResourceLoader.load( "res://textures/art/IMG_2414.JPG" ),
	ResourceLoader.load( "res://textures/fromeaglespeak.jpg" )
]

func _ready() -> void:
	_image.texture = _image_cache[ randi_range( 0, _image_cache.size() - 1 ) ]

func _on_image_change_timeout() -> void:
	if _image.texture == _image_cache.back():
		_image.texture = _image_cache.front()
	
	_image.texture = _image_cache[ randi_range( 0, _image_cache.size() - 1 ) ]
