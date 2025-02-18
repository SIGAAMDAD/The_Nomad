extends TextureRect

var _surface_image:Image = Image.new()
var _blood_image:Image = Image.new()
var _surface_texture:ImageTexture = ImageTexture.new()
var _blood_size:Vector2 = Vector2.ZERO

func _ready() -> void:
	_surface_image = Image.create( size.x, size.y, false, Image.FORMAT_RGBA8 )
	_surface_image.fill( Color( 0, 0, 0, 0 ) )
	
	_blood_image.load( "res://textures/blood.png" )
	_blood_image.convert( Image.FORMAT_RGBA8 )
	_blood_size = _blood_image.get_size()
	
	Engine.register_singleton( "Background", self )

func draw_blood( draw_pos: Vector2 ):
	#stamp the blood on to surface
	_surface_image.blit_rect( _blood_image, Rect2i( Vector2( 0, 0 ), _blood_size ), draw_pos )
	texture = ImageTexture.create_from_image( _surface_image )

func _physics_process( delta: float ) -> void:
	pass
