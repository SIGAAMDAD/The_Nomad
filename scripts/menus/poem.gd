extends Control

var _loading:bool = false

@onready var _transition_screen:CanvasLayer = $Fade
@onready var _current_timer:int = 0
@onready var _author:Label = $AuthorName
@onready var _press_enter:Label = $PressEnter
@onready var TIMERS:Array[ Timer ] = [
	$Label/Timer1,
	$Label2/Timer2,
	$Label3/Timer3,
	$Label4/Timer4,
	$Label5/Timer5
];
@onready var LABELS:Array[ Label ] = [
	$Label,
	$Label2,
	$Label3,
	$Label4,
	$Label5
];

func _on_finished_loading() -> void:
	LoadingScreen.FadeOut()
	SoundManager.stop_music( 1.5 )

func on_finished_loading_scene() -> void:
	GameConfiguration.LoadedLevel.ChangeScene()
	queue_free()
	
	var scene:Node = GameConfiguration.LoadedLevel.currentSceneNode
	scene.FinishedLoading.connect( _on_finished_loading )

func _process( _delta: float ) -> void:
	if Input.is_action_just_pressed( "ui_advance" ):
		call_deferred( "advance_timer" )

func advance_timer() -> void:
	if _loading:
		return

	if _current_timer >= LABELS.size():
		_loading = true

		_transition_screen.transition()
		await _transition_screen.transition_finished

		hide()

		LoadingScreen.FadeIn()

		Console.print_line( "Loading game..." )

		var scene:AsyncScene = AsyncScene.new( "res://levels/world.tscn", AsyncScene.LoadingSceneOperation.ReplaceImmediate )
		GameConfiguration.LoadedLevel = scene

		scene.OnComplete.connect( on_finished_loading_scene )
		return
	
	_current_timer += 1
	if _current_timer < TIMERS.size():
		LABELS[ _current_timer ].show()
		TIMERS[ _current_timer ].start()
		if _current_timer == TIMERS.size() - 1:
			_author.show()

func _on_timer_1_timeout() -> void:
	advance_timer()

func _on_timer_2_timeout() -> void:
	advance_timer()

func _on_timer_3_timeout() -> void:
	advance_timer()

func _on_timer_4_timeout() -> void:
	advance_timer()

func _on_timer_5_timeout() -> void:
	advance_timer()
