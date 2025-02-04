class_name HeadsUpDisplay extends CanvasLayer

@onready var _health_bar:ProgressBar = $HealthBar
@onready var _rage_bar:ProgressBar = $RageBar
@onready var _status_bar_timer:Timer = $StatusBarTimer

@onready var _reflex_overlay:TextureRect = $Overlays/ReflexModeOverlay
@onready var _dash_overlay:TextureRect = $Overlays/DashOverlay

@onready var _save_timer:Timer = $SaveSpinner/SaveTimer
@onready var _save_spinner:Spinner = $SaveSpinner/SaveSpinner

@onready var _weapon_data:WeaponEntity = null
@onready var _weapon_status:TextureRect = $WeaponStatus
@onready var _weapon_mode_bladed:TextureRect = $WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBladed
@onready var _weapon_mode_blunt:TextureRect = $WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusBlunt
@onready var _weapon_mode_firearm:TextureRect = $WeaponStatus/MarginContainer/HBoxContainer/MarginContainer/StatusContainer/StatusFirearm
@onready var _weapon_status_firearm:VBoxContainer = $WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus
@onready var _weapon_status_melee:VBoxContainer = $WeaponStatus/MarginContainer/HBoxContainer/MeleeStatus
@onready var _weapon_status_melee_icon:TextureRect = $WeaponStatus/MarginContainer/HBoxContainer/MeleeStatus/WeaponIcon
@onready var _weapon_status_firearm_icon:TextureRect = $WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/WeaponIcon
@onready var _weapon_status_bullet_count:Label = $WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletCountLabel
@onready var _weapon_status_bullet_reserve:Label = $WeaponStatus/MarginContainer/HBoxContainer/FireArmStatus/AmmunitionContainer/BulletReserveLabel

# why do I hear boss music...?
@onready var _boss_health_bar:Control = $BossHealthBar

func show_status_bars() -> void:
	_health_bar.show()
	_rage_bar.show()
	_status_bar_timer.start()

func set_weapon( weapon: WeaponEntity ) -> void:
	if weapon == null:
		_weapon_status.hide()
		return
	else:
		_weapon_status.show()
	
	if weapon._last_used_mode & WeaponBase.Properties.IsFirearm:
		_weapon_status_firearm.show()
		_weapon_status_melee.hide()
		
		_weapon_status_firearm_icon.texture = weapon._data.icon
		_weapon_status_bullet_count.text = str( weapon._bullets_left )
		if weapon._reserve:
			_weapon_status_bullet_reserve.text = str( weapon._reserve.amount )
		else:
			_weapon_status_bullet_reserve.text = "0"
		
		_weapon_data = weapon
	else:
		_weapon_status_firearm.hide()
		_weapon_status_melee.show()
		
		_weapon_status_melee_icon.texture = weapon._data.icon

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
		return
	else:
		show()
	
	if _weapon_data:
		_weapon_mode_bladed.material.set( "shader_parameter/status_active", _weapon_data._last_used_mode & WeaponBase.Properties.IsBladed )
		_weapon_mode_blunt.material.set( "shader_parameter/status_active", _weapon_data._last_used_mode & WeaponBase.Properties.IsBlunt )
		if _weapon_data._last_used_mode & WeaponBase.Properties.IsFirearm:
			_weapon_mode_firearm.material.set( "shader_parameter/status_active", true )
			_weapon_status_bullet_count.text = str( _weapon_data._bullets_left )
			if _weapon_data._reserve:
				_weapon_status_bullet_reserve.text = str( _weapon_data._reserve.amount )
		else:
			_weapon_mode_firearm.material.set( "shader_parameter/status_active", false )

func _on_status_bar_timer_timeout() -> void:
	_health_bar.hide()
	_rage_bar.hide()

func _on_save_timer_timeout() -> void:
	_save_spinner.hide()
