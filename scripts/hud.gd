class_name HeadsUpDisplay extends CanvasLayer

@onready var _health_bar:ProgressBar = $HealthBar
@onready var _rage_bar:ProgressBar = $RageBar
@onready var _status_bar_timer:Timer = $StatusBarTimer

@onready var _reflex_overlay:TextureRect = $Overlays/ReflexModeOverlay
@onready var _dash_overlay:TextureRect = $Overlays/DashOverlay

@onready var _save_timer:Timer = $SaveSpinner/SaveTimer
@onready var _save_spinner:Spinner = $SaveSpinner/SaveSpinner

# why do I hear boss music...?
@onready var _boss_health_bar:Control = $BossHealthBar

func show_status_bars() -> void:
	_health_bar.show()
	_rage_bar.show()
	_status_bar_timer.start()

func set_health( health: float ) -> void:
	_health_bar.health = health
	show_status_bars()

func set_rage( rage: float ) -> void:
	_rage_bar.rage = rage
	show_status_bars()

func save_start() -> void:
	_save_spinner.show()

func save_end() -> void:
	_save_timer.start()

func release_boss() -> void:
	_boss_health_bar.hide()

func set_boss( boss: BossBase ) -> void:
	_boss_health_bar.show()
	_boss_health_bar.init( boss )

func init( _health, _rage ):
	_health_bar.init( _health )
	_rage_bar.init( _rage )
	
	ArchiveSystem.connect( "on_save_game_start", save_start )
	ArchiveSystem.connect( "on_save_game_end", save_end )

func _process( _delta: float ) -> void:
	if Engine.time_scale == 0.0:
		hide()
	else:
		show()

func _on_status_bar_timer_timeout() -> void:
	_health_bar.hide()
	_rage_bar.hide()

func _on_save_timer_timeout() -> void:
	_save_spinner.hide()
