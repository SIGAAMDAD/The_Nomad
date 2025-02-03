class_name CaptureTheFlagMode extends MultiplayerMode

var _match_timer:Timer = Timer.new()

var _team_1_flag:Node2D = null
var _team_2_flag:Node2D = null

var _team_1_score:int = 0
var _team_2_score:int = 0

# player currently holding team 2's flag
var _team_1_flag_holder:Player = null

# player currently holding team 1's flag
var _team_2_flag_holder:Player = null

func _on_match_timer_timeout() -> void:
	pass

func _on_flag_1_grab( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		return
	elif body._multiplayer_team != 2:
		return
	
	_team_1_flag_holder = body
	_team_1_flag.reparent( body )
	emit_signal( "flag_1_captured" )

func _on_flag_2_grab( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		return
	elif body._multiplayer_team != 1:
		return
	
	_team_2_flag_holder = body
	_team_2_flag.reparent( body )
	emit_signal( "flag_2_captured" )

func _init( flag1: Node2D, flag2: Node2D ) -> void:
	print( "Initializing CaptureTheFlag mode..." )
	print( "Flag1 Position: ", flag1.global_position )
	print( "Flag2 Position: ", flag2.global_position )
	
	_team_1_flag = flag1
	_team_2_flag = flag2
	
	( _team_1_flag.get_child( 0 ) as Sprite2D ).texture = ResourceLoader.load( "res://textures/env/flag_blue.png" )
	( _team_2_flag.get_child( 0 ) as Sprite2D ).texture = ResourceLoader.load( "res://textures/env/flag_red.png" )
	
	( _team_1_flag.get_child( 1 ) as Area2D ).body_shape_entered.connect( _on_flag_1_grab )
	( _team_2_flag.get_child( 1 ) as Area2D ).body_shape_entered.connect( _on_flag_2_grab )
	
	var overlay = load( "res://scenes/multiplayer/modes/overlays/ctf_overlay.tscn" ).instantiate()
	
	# default ctf time is 5 minutes
	_match_timer.wait_time = 300
	_match_timer.timeout.connect( _on_match_timer_timeout )
