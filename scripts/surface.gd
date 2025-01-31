extends Sprite2D

var surface_image:Image = Image.new()
var blood_image:Image = Image.new()

var surface_texture:ImageTexture = ImageTexture.new()

var blood_size:Vector2

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	surface_image = Image.create( 2000, 2000, false, Image.Format.FORMAT_RGBA8 )
	surface_image.fill( Color( 0, 0, 0, 0 ) )
	
	blood_image.load( "res://textures/blood1.png" )
	blood_image.convert( Image.FORMAT_RGBA8 )
	blood_size = blood_image.get_size()

func draw_blood(draw_pos: Vector2) -> void:
	surface_image.blit_rect( blood_image, Rect2i( Vector2( 0, 0 ), blood_size ), draw_pos )

func _physics_process(delta: float) -> void:
	texture = ImageTexture.create_from_image( surface_image )

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
