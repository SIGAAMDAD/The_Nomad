extends Sprite2D

var _surface_image:Image = Image.new()
var _blood_image:Image = Image.new()

var _surface_texture:ImageTexture = ImageTexture.new()

var _blood_size:Vector2 = Vector2.ZERO

func _ready() -> void:
	global_position.x = -1132
	global_position.y = -682
	
	_surface_image = Image.create( 4627, 2409, false, Image.FORMAT_RGBA8 )
	_surface_image.fill( Color( 0, 0, 0, 0 ) )
	
	_blood_image.load( "res://textures/blood.png" )
	_blood_image.convert( Image.FORMAT_RGBA8 )
	_blood_size = _blood_image.get_size()

func draw_blood( drawPosition: Vector2 ) -> void:
	print( "Drawing blood" )
	# stamp the blood onto the surface
	_surface_image.blit_rect( _blood_image, Rect2i( Vector2i( 0, 0 ), _blood_size ), drawPosition )
	texture = ImageTexture.create_from_image( _surface_image )

func _physics_process( _delta: float ) -> void:
	pass

func ClearTexture() -> void:
	_surface_image.fill( Color( 0, 0, 0, 0 ) )
