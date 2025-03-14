extends CanvasLayer

@onready var _tip_label:Label = $Tips/TipLabel
@onready var _progress_label:Label = $Tips/ProgressLabel
#@onready var _image_timer:Timer = $ImageChange

#var _current_image:int = 0

@onready var _tips:Array[ String ] = [
	"You can parry anything that's flashing green, including blades, bullets, etc.",
	"Dashing into an enemy will send them flying",
	"Just parry the bullet dude!",
	"Different enemies require different tactics. No more brainless shooting!",
	"You're a 500 pound hunk of muscle and metal, use that to your advantage",
	"The harder you are hit, the harder you hit back",
	"Death follows you everywhere you go",
	"Don't be scared to experiment",
	"Bathe in the blood of your enemies for some health",
	"Remember: warcrimes don't exist anymore!",
	"A mission can count as a stealth mission if there aren't any witnesses",
	"There are tips here, y'know, read 'em",
	"Always keep in mind that STEALTH is optional",
	"ANYTHING and EVERYTHING is a weapon",
	"You can slice bullets in half, just make sure whatever's behind you can take the hit",
	"You are literally too angry to die",
	"Stop hiding behind cover like a little coward",
	"Don't blame the game for your skill issue"
]

func _ready() -> void:
	set_process( false )

func _process( _delta: float ) -> void:
	_progress_label.text = var_to_str( GameConfiguration.LoadedLevel.progress )

func _on_image_change_timeout() -> void:
#	$"Panel/Images".get_child( _current_image ).hide()
#	_current_image = randi_range( 0, $"Panel/Images".get_child_count() - 1 )
#	$"Panel/Images".get_child( _current_image ).show()
	
	var tipIndex:int = randi_range( 0, _tips.size() - 1 )
	var newTip:String = _tips[ tipIndex ]
	if newTip == _tip_label.text:
		if tipIndex == _tips.size() - 1:
			tipIndex = 0
		else:
			tipIndex += 1
	
	_tip_label.text = _tips[ tipIndex ]

func _on_visibility_changed() -> void:
#	for image in $"Panel/Images".get_children():
#		image.queue_free()
#		$"Panel/Images".remove_child( image )
#	
	set_process( visible )
#	
#	if !visible:
#		_image_timer.timeout.disconnect( _on_image_change_timeout )
#		_image_timer.stop()
#		return
#	
#	var image0:TextureRect = TextureRect.new()
#	image0.set_anchors_preset( Control.LayoutPreset.PRESET_FULL_RECT )
#	image0.texture = ResourceLoader.load( "res://textures/fromeaglespeak.dds" )
#	$"Panel/Images".add_child( image0 )
#	
#	var image1:TextureRect = TextureRect.new()
#	image1.set_anchors_preset( Control.LayoutPreset.PRESET_FULL_RECT )
#	image1.texture = ResourceLoader.load( "res://textures/art/IMG_1709.dds" )
#	$"Panel/Images".add_child( image1 )
#	
#	var image2:TextureRect = TextureRect.new()
#	image2.set_anchors_preset( Control.LayoutPreset.PRESET_FULL_RECT )
#	image2.texture = ResourceLoader.load( "res://textures/art/IMG_2189.dds" )
#	$"Panel/Images".add_child( image2 )
#	
#	var image3:TextureRect = TextureRect.new()
#	image3.set_anchors_preset( Control.LayoutPreset.PRESET_FULL_RECT )
#	image3.texture = ResourceLoader.load( "res://textures/art/IMG_2190.dds" )
#	$"Panel/Images".add_child( image3 )
#	
#	var image4:TextureRect = TextureRect.new()
#	image4.set_anchors_preset( Control.LayoutPreset.PRESET_FULL_RECT )
#	image4.texture = ResourceLoader.load( "res://textures/art/IMG_2251.dds" )
#	$"Panel/Images".add_child( image4 )
#	
#	var image5:TextureRect = TextureRect.new()
#	image5.set_anchors_preset( Control.LayoutPreset.PRESET_FULL_RECT )
#	image5.texture = ResourceLoader.load( "res://textures/art/IMG_2412.dds" )
#	$"Panel/Images".add_child( image5 )
#	
#	var image6:TextureRect = TextureRect.new()
#	image6.set_anchors_preset( Control.LayoutPreset.PRESET_FULL_RECT )
#	image6.texture = ResourceLoader.load( "res://textures/art/IMG_2414.dds" )
#	$"Panel/Images".add_child( image6 )
#	
#	_image_timer.timeout.connect( _on_image_change_timeout )
#	_image_timer.start()
#	
#	_current_image = randi_range( 0, $"Panel/Images".get_child_count() - 1 )
#	$"Panel/Images".get_child( _current_image ).show()
#	_tip_label.text = _tips[ randi_range( 0, _tips.size() - 1 ) ]
