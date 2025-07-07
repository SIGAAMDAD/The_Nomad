class_name Equipment extends TabBar

enum WeaponSlotIndex {
	Primary,
	HeavyPrimary,
	Sidearm,
	HeavySidearm
};

var _owner: CharacterBody2D

@onready var _weapon_list: VBoxContainer = $MarginContainer/SelectionScreen/VBoxContainer/VBoxContainer/VScrollBar/WeaponList
@onready var _primary_weapon: TextureRect = $MarginContainer/LoadoutScreen/PrimaryWeaponsContainer/PrimaryIcon
@onready var _heavy_primary_weapon: TextureRect = $MarginContainer/LoadoutScreen/PrimaryWeaponsContainer/HeavyPrimaryIcon
@onready var _sidearm_weapon: TextureRect = $MarginContainer/LoadoutScreen/SidearmWeaponsContainer/SidearmIcon
@onready var _heavy_sidearm_weapon: TextureRect = $MarginContainer/LoadoutScreen/SidearmWeaponsContainer/HeavySidearmIcon
@onready var _selection_container: MarginContainer = $MarginContainer/SelectionScreen
@onready var _loadout_container: VBoxContainer = $MarginContainer/LoadoutScreen
@onready var _background: MarginContainer = $Background

func _on_selection_container_visibility_changed() -> void:
	_loadout_container.visible = !_selection_container.visible
	_background.visible = _loadout_container.visible

func _ready() -> void:
	_owner = get_node( "/root/LevelData" ).ThisPlayer
	
	connect( "visibility_changed", init_equipment )
	
	var _go_back_button: Button = _selection_container.get_node( "VBoxContainer/GoBackButton" )
	_go_back_button.connect( "pressed", func(): _selection_container.hide() )
	
	_selection_container.connect( "visibility_changed", _on_selection_container_visibility_changed )
	
	_primary_weapon.connect( "gui_input", func( gui_event: InputEvent ): _on_weapon_selected( gui_event, _primary_weapon ) )
	_primary_weapon.set_meta( "category", "WEAPON_CATEGORY_PRIMARY" )
	
	_sidearm_weapon.connect( "gui_input", func( gui_event: InputEvent ): _on_weapon_selected( gui_event, _sidearm_weapon ) )
	_sidearm_weapon.set_meta( "category", "WEAPON_CATEGORY_SIDEARM" )

static func is_item_select_input_valid( gui_event: InputEvent ) -> bool:
	if gui_event is InputEventMouseButton:
		return gui_event.button_index == MOUSE_BUTTON_LEFT
	elif gui_event is InputEventJoypadButton:
		return gui_event.button_index == JOY_BUTTON_A
	return false

func init_equipment() -> void:
	if _owner.WeaponSlots[ WeaponSlotIndex.Primary ].GetWeapon() != null:
		_primary_weapon.texture = _owner.WeaponSlots[ WeaponSlotIndex.Primary ].GetWeapon().Icon
	if _owner.WeaponSlots[ WeaponSlotIndex.Sidearm ].GetWeapon() != null:
		_sidearm_weapon.texture = _owner.WeaponSlots[ WeaponSlotIndex.Sidearm ].GetWeapon().Icon

func _get_weapon_categories( weapon: Node, category: String, mutex: Mutex, weapons: Array[ Node ] ) -> void:
	for it in weapon.Data.categories:
		if it.id == category:
			mutex.lock()
			weapons.push_back( weapon )
			mutex.unlock()

func get_weapons_in_category( category: String ) -> Array[ Node ]:
	var _weapons: Array[ Node ] = []
	var _lock: Mutex = Mutex.new()
	
	for weapon in _owner.GetWeaponsStack().values():
		_get_weapon_categories( weapon, category, _lock, _weapons )
	
	return _weapons

func _on_weapon_item_selected( gui_event: InputEvent, item: TextureRect, category: String ) -> void:
	if !is_item_select_input_valid( gui_event ):
		return
	
	match category:
		"WEAPON_CATEGORY_PRIMARY":
			_owner.SetPrimaryWeapon( item.get_meta( "weapon" ) )
		"WEAPON_CATEGORY_SIDEARM":
			_owner.SetSidearmWeapon( item.get_meta( "weapon" ) )
	
	init_equipment()
	_selection_container.get_node( "VBoxContainer/GoBackButton" ).emit_signal( "pressed" )

func _on_weapon_selected( gui_event: InputEvent, weapon_slot: TextureRect ) -> void:
	if !is_item_select_input_valid( gui_event ):
		return
	
	_selection_container.show()
	
	for child in _weapon_list.get_children():
		_weapon_list.call_deferred( "remove_child", child )
		child.call_deferred( "queue_free" )
	
	var _weapons: Array[ Node ] = get_weapons_in_category( weapon_slot.get_meta( "category" ) )
	for weapon in _weapons:
		var _item: TextureRect = TextureRect.new()
		_item.texture = weapon.Icon
		_item.size_flags_horizontal = Control.SIZE_SHRINK_BEGIN
		_item.custom_minimum_size = Vector2( 128, 64 )
		_item.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED
		_item.tooltip_text = weapon.Data.name
		_item.set_meta( "weapon", weapon )
		_item.connect( "gui_input", func( gui_event: InputEvent ): _on_weapon_item_selected( gui_event, _item, weapon_slot.get_meta( "category" ) ) )
		_weapon_list.call_deferred( "add_child", _item )
