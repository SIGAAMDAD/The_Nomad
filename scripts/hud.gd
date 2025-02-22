class_name HeadsUpDisplay extends CanvasLayer

@export var _owner:Player = null
@onready var _health_bar:HealthBar = $HealthBar
@onready var _rage_bar:RageBar = $RageBar
@onready var _status_bar_timer:Timer = $StatusBarTimer
@onready var _inventory:MarginContainer = $Inventory/MarginContainer
@onready var _stack_list:VBoxContainer = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/Cloner
@onready var _item_stack_cloner:VBoxContainer = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/Cloner

@onready var _item_name:Label = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/NameLabel
@onready var _item_type:Label = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/TypeContainer/Label
@onready var _item_count:Label = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/CountLabel
@onready var _item_stack_max:Label = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/MaxLabel
@onready var _item_icon:TextureRect = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/Icon
@onready var _item_description:RichTextLabel = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DescriptionLabel
@onready var _item_effect:Label = $Inventory/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/EffectContainer/Label2

@onready var _demon_eye_overlay:TextureRect = $Overlays/DemonEyeOverlay
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

#func show_status_bars() -> void:
#	_health_bar.show()
#	_rage_bar.show()
#	_status_bar_timer.start()

func free() -> void:
	_status_bar_timer.queue_free()
	_demon_eye_overlay.queue_free()
	_reflex_overlay.queue_free()
	_dash_overlay.queue_free()
	_save_spinner.queue_free()
	_save_timer.queue_free()
	_health_bar.queue_free()
	_rage_bar.queue_free()
	_inventory.queue_free()
	_weapon_data.queue_free()
	_weapon_status.queue_free()
	_weapon_mode_bladed.queue_free()
	_weapon_mode_blunt.queue_free()
	_weapon_mode_firearm.queue_free()
	_weapon_status_melee.queue_free()
	_weapon_status_melee_icon.queue_free()
	_weapon_status_firearm_icon.queue_free()
	_weapon_status_bullet_count.queue_free()
	_weapon_status_bullet_reserve.queue_free()
	_weapon_status.queue_free()
	_boss_health_bar.queue_free()
	queue_free()

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
	pass
#	_health_bar.health = health
#	show_status_bars()

func set_rage( rage: float ) -> void:
	_rage_bar.rage = rage
#	show_status_bars()

func get_item_count( id: String ) -> int:
	for item in _owner._inventory.stacks:
		if item.item_id == id:
			return item.amount
	return 0

func _on_inventory_item_selected( event: InputEvent, item: TextureRect ) -> void:
	if !item.has_meta( "item_id" ) || event is not InputEventMouseButton:
		return
	elif event.button_index != MOUSE_BUTTON_LEFT:
		return
	
	var item_type := _owner._inventory.database.get_item( item.get_meta( "item_id" ) )
	
	if item_type.properties.has( "description" ):
		_item_description.show()
		_item_description.text = item_type.properties.description
	else:
		_item_description.hide()
	
	if item_type.properties.has( "effect" ):
		_item_effect.show()
		_item_effect.text = item_type.properties.effect
	else:
		_item_effect.hide()
	
	_item_name.text = item_type.name
	_item_icon.texture = item_type.icon
	_item_type.text = item_type.categories[0].name
	
	match item.get_meta( "category" ):
		"misc", "ammo":
			_item_count.text = var_to_str( get_item_count( item_type.id ) )
			_item_stack_max.text = var_to_str( item_type.max_stack )
		"weapon":
			_item_count.text = "1"
			_item_stack_max.text = var_to_str( item_type.max_stack )

func add_item_to_inventory( row: HBoxContainer, stack: ItemStack ) -> HBoxContainer:
	if row.get_child_count() == 4:
		row = HBoxContainer.new()
		_stack_list.add_child( row )
	
	var item := TextureRect.new()
	row.add_child( item )
	
	var event:InputEvent = null
	
	item.gui_input.connect( Callable( _on_inventory_item_selected ).bind( item ) )
	item.texture = _owner._inventory.database.get_item( stack.item_id ).icon
	item.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	item.custom_minimum_size.x = 64
	item.custom_minimum_size.y = 64
	item.set_meta( "item_id", stack.item_id )
	
	return row

func _on_show_inventory() -> void:
	if _inventory.visible:
		_inventory.hide()
		return
	
	for child in _stack_list.get_children():
		for image in child.get_children():
			child.remove_child( image )
			image.queue_free()
		
		_stack_list.remove_child( child )
		child.queue_free()
	
	_stack_list.show()
	
	var row := HBoxContainer.new()
	
	_stack_list.add_child( row )
	for stack in _owner._ammo_light_stacks:
		row = add_item_to_inventory( row, stack )
	
	for stack in _owner._ammo_pellet_stacks:
		row = add_item_to_inventory( row, stack )
	
	for stack in _owner._weapon_stacks:
		row = add_item_to_inventory( row, stack )
	
	for stack in _owner._inventory.stacks:
		row = add_item_to_inventory( row, stack )
	
	_inventory.show()

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
	pass
#	_health_bar.hide()
#	_rage_bar.hide()

func _on_save_timer_timeout() -> void:
	_save_spinner.hide()
