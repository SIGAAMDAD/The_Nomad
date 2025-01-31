class_name Inventory extends Node2D

const _MAX_WEAPON_SLOTS:int = 8

@export var _left_arm:Arm = null
@export var _right_arm:Arm = null
@export var _parent:Player = null

var _max_weight:int = 0
var _current_weapon:int = 0
var _slots:Array[ ItemStack ]
var _weapon_slots:Array[ WeaponSlot ]

func save( file: FileAccess, playerID: int = 0 ) -> void:
	var section := JSON.new()
	var sectionName = "inventory_" + str( playerID )
	
	section[ sectionName ][ "max_weight" ] = _max_weight
	section[ sectionName ][ "current_weapon" ] = _current_weapon
	section[ sectionName ][ "num_slots" ] = _slots.size()
	
	file.store_line( section.to_string() )

func load( file: FileAccess, playerID: int = 0 ) -> void:
	var section := SaveSection.new()
	
	section.load( file )

func _ready() -> void:
	_weapon_slots.resize( _MAX_WEAPON_SLOTS )
	for i in _MAX_WEAPON_SLOTS:
		_weapon_slots[i] = WeaponSlot.new()
		_weapon_slots[i]._index = i

func _inventory_contains_item( name: String ) -> bool:
	for item in _slots:
		if item._data._name == name:
			return true
	
	return false

func _inventory_get_item( name: String ) -> int:
	for item in range( 0, _slots.size() - 1 ):
		if _slots[item]._data._name == name:
			return item
	return -1

func get_equipped_weapon() -> WeaponSlot:
	return _weapon_slots[ _current_weapon ]

func equip_slot( slot: int ) -> void:
	var currentWeapon := slot
	
	var weapon := _weapon_slots[ slot ]._weapon
	if weapon:
		# apply rules of various weapno properties
		if weapon._last_used_mode & WeaponBase.Properties.IsTwoHanded:
			_left_arm._weapon_slot = _current_weapon
			_right_arm._weapon_slot = _current_weapon
			
			# this will automatically override any other modes
			_weapon_slots[ _left_arm._weapon_slot ]._mode = weapon._data._default_mode
			_weapon_slots[ _right_arm._weapon_slot ]._mode = weapon._data._default_mode
		
		_weapon_slots[ _parent._last_used_arm._weapon_slot ]._mode = weapon._data._properties
	else:
		_left_arm._weapon_slot = -1
		_right_arm._weapon_slot = -1
	
	# update hand data
	_parent._last_used_arm._weapon_slot = _current_weapon

func pickup_ammo( ammo: AmmoEntity ) -> void:
	var stack := _inventory_get_item( ammo._data._name )
	if stack == -1:
		stack = _slots.size()
		_slots.push_back( ItemStack.new() )
	
	_slots[ stack ].set_item_type( ammo._data._name )
	_slots[ stack ].add_items( ammo._data._item_stack_add )
	
	for i in _MAX_WEAPON_SLOTS:
		var slot := _weapon_slots[i]
		if slot.is_used() && slot._weapon._data._ammo_type == ammo._data._ammo_type:
			slot._weapon.set_reserve( _slots[ stack ] )
			slot._weapon.set_ammo( ammo )

func pickup_weapon( weapon: WeaponEntity ) -> void:
	for i in _MAX_WEAPON_SLOTS:
		if !_weapon_slots[i].is_used():
			_weapon_slots[i]._weapon = weapon
			break
	
	if SettingsData._equip_weapon_on_pickup:
		# apply rules of various weapon properties
		if weapon._last_used_mode & WeaponBase.Properties.IsTwoHanded:
			_left_arm._weapon_slot = _current_weapon
			_right_arm._weapon_slot = _current_weapon
			
			# this will automatically override any other modes
			_weapon_slots[ _left_arm._weapon_slot ]._mode = weapon._data._default_mode
			_weapon_slots[ _right_arm._weapon_slot ]._mode = weapon._data._default_mode
			return
		
		# update the hand data
		_parent._last_used_arm._weapon_slot = _current_weapon
		_weapon_slots[ _parent._last_used_arm._weapon_slot ]._mode = weapon._data._properties
