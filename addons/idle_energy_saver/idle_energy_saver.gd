extends Node
##
## This class lowers CPU/GPU usage as soon as the
## application window loses focus by changing some
## of the project settings. Restores the original
## values as soon as the window is focussed again.
##


##
## Will be emitted when the application/game window
## enters the idle state which is when it loses its
## focus.[br]
## You can simply connect to it like so:
## [codeblock]
## func _ready() -> void:
##     $IdleEnergySaver.entered_idle_state.connect(_on_entered_idle_state)
##
## func _on_entered_idle_state() -> void:
##     # let an "idle" overlay appear, for example
##     pass
## [/codeblock]
##
signal entered_idle_state
##
## Will be emitted when the application/game window
## exits the idle state which is when it gains focus.[br]
## You can simply connect to it like so:
## [codeblock]
## func _ready() -> void:
##     $IdleEnergySaver.exited_idle_state.connect(_on_exited_idle_state)
##
## func _on_exited_idle_state() -> void:
##     # let that "idle" overlay disappear again, for example
##     pass
## [/codeblock]
##
signal exited_idle_state


##
## The amount of microseconds to sleep between frames
## when application is idling in background.
##
@export var idle_sleep_msec: int = 500000
##
## The maximum number of frames per second to be used
## when application is idling in background.
##
@export var idle_max_fps: int = 2


##
## Helper variable to keep track of original value for
## [member OS.low_processor_usage_mode].
##
var low_processor_usage_mode: bool
##
## Helper variable to keep track of original value for
## [member OS.low_processor_usage_mode_sleep_usec].
##
var low_processor_usage_mode_sleep_usec: int
##
## Helper variable to keep track of original value for
## [member Engine.max_fps].
##
var max_fps: int


func _ready() -> void:
	# keep in mind original values as set in project settings
	# in order to be able to reset them again later on
	low_processor_usage_mode = OS.low_processor_usage_mode
	low_processor_usage_mode_sleep_usec = OS.low_processor_usage_mode_sleep_usec
	max_fps = Engine.max_fps


func _notification(what: int) -> void:
	# check for window focus events
	match what:
		NOTIFICATION_APPLICATION_FOCUS_OUT:
			_idle()
		NOTIFICATION_APPLICATION_FOCUS_IN:
			_unidle()


##
## Lowers the CPU/GPU usage by changing some settings.
##
func _idle() -> void:
	OS.low_processor_usage_mode = true
	OS.low_processor_usage_mode_sleep_usec = idle_sleep_msec
	Engine.max_fps = idle_max_fps

	entered_idle_state.emit()


##
## Resets the CPU/GPU usage by restoring the original setting values.
##
func _unidle() -> void:
	OS.low_processor_usage_mode = low_processor_usage_mode
	OS.low_processor_usage_mode_sleep_usec = low_processor_usage_mode_sleep_usec
	Engine.max_fps = max_fps

	exited_idle_state.emit()
