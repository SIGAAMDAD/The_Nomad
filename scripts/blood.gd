extends Area2D

@onready var surface:Sprite2D = $surface

var is_colliding:bool = false

var vspeed:float = randf_range( -5.0, 5.0 )
var hspeed:float = randf_range( -3.0, 3.0 )
var blood_acc:float = randf_range( 0.05, 0.1 )

var do_wobble:bool = false

var max_count:float = randf_range( 10, 50 )
var count:int = 0

func _physics_process(delta: float) -> void:
	HandleBloodMovement()
	
	if is_colliding:
		surface.draw_blood( position )
	if position.y > 1000:
		queue_free()

func _on_body_entered(body: Node2D) -> void:
	is_colliding = true

func _on_body_exited(body: Node2D) -> void:
	is_colliding = false

func HandleBloodMovement() -> void:
	if !is_colliding: # in air
		do_wobble = false
		vspeed = lerp( vspeed, 5.0, 0.02 )
		hspeed = lerp( hspeed, 0.0, 0.02 )
		visible = true
	else: # touching platform
		count += 1
		if count > max_count:
			queue_free()
		
		if vspeed > 3:
			vspeed = 3
		
		vspeed = lerp( vspeed, 0.1, blood_acc )
		
		# if we're moving downwards in a line, add wobble
		if hspeed > 0.1 or hspeed < -0.1:
			hspeed = lerp( hspeed, 0.0, 0.1 )
		else:
			do_wobble = true
		
		visible = false
	
	if  do_wobble:
		hspeed += randf_range( -0.01, 0.01 )
		hspeed = clamp( hspeed, -0.1, 0.1 )
	
	position.x += vspeed
	position.y += hspeed;
