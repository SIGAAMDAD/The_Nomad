extends Control

@onready var _boss_name:Label = $BossName
@onready var _health_bar:ProgressBar = $HealthBar

func init( boss: BossBase ) -> void:
	_boss_name.text = boss._name
	_health_bar.init( boss._health )
