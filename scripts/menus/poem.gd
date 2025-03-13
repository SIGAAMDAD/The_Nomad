extends Control

signal poem_completed

@onready var _current_timer:int = 0
@onready var _author:Label = $AuthorName
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

func on_finished_loading() -> void:
	GameConfiguration.LoadedLevel.ChangeScene()
	LoadingScreen.hide()
	SoundManager.stop_music( 1.5 )
	hide()

func _unhandled_input( event: InputEvent ) -> void:
	if event is not InputEventKey:
		return
	elif ( event as InputEventKey ).keycode != KEY_ENTER:
		return
	
	advance_timer()

func advance_timer() -> void:
	if _current_timer >= LABELS.size():
		hide()
		LoadingScreen.show()
		
		Console.print_line( "Loading game..." )
		
		var scene:AsyncScene = AsyncScene.new( "res://levels/world.tscn", AsyncScene.LoadingSceneOperation.ReplaceImmediate )
		GameConfiguration.LoadedLevel = scene
		
		scene.OnComplete.connect( on_finished_loading )
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
