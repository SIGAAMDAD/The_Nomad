class_name Backpack extends TabBar

enum PlayerFlags {
	Sliding = 0x00000001,
	Crouching = 0x00000002,
	BulletTime = 0x00000004,
	Dashing = 0x00000008,
	DemonRage = 0x00000010,
	UsedMana = 0x00000020,
	DemonSight = 0x00000040,
	OnHorse = 0x00000080,
	IdleAnimation = 0x00000100,
	Checkpoint = 0x00000200,
	BlockedInput = 0x00000400,
	UsingWeapon = 0x00000800,
	Inventory = 0x00001000,
	Resting = 0x00002000,
	UsingMelee = 0x00004000,
	Parrying = 0x00008000,
	Encumbured = 0x00010000,
	Emoting = 0x00020000,
	Sober = 0x00040000,
};

const _backpack_item_minimum_size: Vector2 = Vector2( 64.0, 64.0 )
const _max_items_per_row: int = 4

var _owner: CharacterBody2D

@onready var _weapon_bladed_damage: Label = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BladedInfo/DamageLabel
@onready var _weapon_bladed_range: Label = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BladedInfo/RangeLabel
@onready var _weapon_blunt_damage: Label = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BluntInfo/DamageLabel
@onready var _weapon_blunt_range: Label = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BluntInfo/RangeLabel
@onready var _weapon_ammodata: TextureRect = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/AmmoLoaded/AmmoIcon

@onready var _stack_list: VBoxContainer = $MarginContainer/VBoxContainer/HBoxContainer/MarginContainer/VScrollBar/Cloner

@onready var _item_name: Label = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/NameLabel
@onready var _item_type: Label = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/TypeContainer/Label
@onready var _item_count: Label = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/CountLabel
@onready var _item_stack_max: Label = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/VBoxContainer/MetaData/NoHeldContainer/HBoxContainer/MaxLabel
@onready var _item_icon: TextureRect = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/HBoxContainer/Icon
@onready var _item_description: RichTextLabel = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DescriptionLabel
@onready var _item_effect: RichTextLabel = $MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/EffectContainer/Label2

@onready var _encumbrance_amount_label: Label = $MarginContainer/VBoxContainer/BackpackDataContainer/EncumbranceContainer/AmountLabel
@onready var _overweight_label: Label = $MarginContainer/VBoxContainer/BackpackDataContainer/EncumbranceContainer/OverweightLabel

var _selected_item: NodePath
var _pickup_script: CSharpScript = preload( "res://scripts/Interactables/ItemPickup.cs" )

func _on_drop_item_stack() -> void:
	var _item: TextureRect = get_node( _selected_item )
	if !_item:
		return
	
	var _item_type: Resource = _item.get_meta( "item" )
	
	var _pickup: Area2D = _pickup_script.new()
	_pickup.name = "ItemPickup_ " + var_to_str( _item_type.id )
	_pickup.global_position = _owner.global_position
	_pickup.Amount = _item.get_meta( "amount" )
	_pickup.Data = _owner.GetInventory().database.get_item( _item_type.id )
	
	var _category: String = _item.get_meta( "category" )
	if _category == "Weapon":
		_owner.DropWeapon( _item.get_meta( "hash" ) )
	elif _category == "Ammo":
		_owner.DropAmmo( _item.get_meta( "hash" ) )
	
	var _row: HBoxContainer = _item.get_parent()
	_row.remove_child( _item )
	_item.call_deferred( "queue_free" )
	
	if _row.get_child_count() == 0:
		_stack_list.remove_child( _row )
		_row.queue_free()

func _ready() -> void:
	_owner = get_node( "/root/LevelData" ).ThisPlayer
	
	var _drop_item_stack_button: Button = get_node( "MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/DropStackButton" )
	_drop_item_stack_button.connect( "pressed", _on_drop_item_stack )
	
	connect( "visibility_changed", _on_visibility_changed )

func _on_visibility_changed() -> void:
	for child in _stack_list.get_children():
		for image in child.get_children():
			child.remove_child( image )
			image.queue_free()
		_stack_list.remove_child( child )
		child.queue_free()
	
	_stack_list.show()
	
	var _row: HBoxContainer = HBoxContainer.new()
	_stack_list.add_child( _row )
	
	var _weight: float = 0.0
	for stack in _owner.GetAmmoStacks().values():
		_row = add_ammo_stack_to_backpack( _row, stack )
		_weight += stack.Amount * stack.AmmoType.Data.weight
	for stack in _owner.GetWeaponsStack().values():
		_row = add_weapon_to_backpack( _row, stack )
		_weight += stack.Weight
	for stack in _owner.GetInventory().stacks:
		var _item_resource: Resource = _owner.GetInventory().database.get_item( stack.item_id )
		_row = add_item_to_backpack( _row, _item_resource )
		_weight += _item_resource.weight * stack.amount
	
	_encumbrance_amount_label.text = var_to_str( _weight ) + "/" + var_to_str( _owner.MaximumInventoryWeight )
	if ( _owner.GetFlags() & PlayerFlags.Encumbured ):
		_overweight_label.show()
	else:
		_overweight_label.hide()

static func is_item_select_input_valid( gui_event: InputEvent ) -> bool:
	if gui_event is InputEventMouseButton:
		return gui_event.button_index == MOUSE_BUTTON_LEFT
	elif gui_event is InputEventJoypadButton:
		return gui_event.button_index == JOY_BUTTON_A
	return false

func _on_backpack_item_selected( gui_event: InputEvent, item: TextureRect ) -> void:
	if !item.has_meta( "item" ) || !is_item_select_input_valid( gui_event ):
		return
	
	var _item: Resource = item.get_meta( "item" )
	
	if item.has_meta( "description" ):
		_item_description.show()
		_item_description.parse_bbcode( TranslationServer.translate( item.get_meta( "description" ) ) )
	else:
		_item_description.hide()
	
	if item.has_meta( "effects" ):
		_item_effect.show()
		_item_effect.parse_bbcode( TranslationServer.translate( item.get_meta( "effects" ) ) )
	else:
		_item_effect.hide()
	
	var _category: String = item.get_meta( "category" )
	if _category == "Weapon":
		var _properties: Dictionary = _item.properties
		
		if _properties.has( "bladed_damage" ):
			_weapon_bladed_damage.text = var_to_str( _properties.get( "bladed_damage" ) )
		if _properties.has( "bladed_range" ):
			_weapon_bladed_range.text = var_to_str( _properties.get( "bladed_range" ) )
		if _properties.has( "blunt_damage" ):
			_weapon_blunt_damage.text = var_to_str( _properties.get( "blunt_damage" ) )
		if _properties.has( "blunt_range" ):
			_weapon_blunt_range.text = var_to_str( _properties.get( "blunt_range" ) )
#	elif _category == "Ammo":
	
	_item_name.text = _item.name
	_item_icon.texture = item.texture
	_item_type.text = _category
	_item_count.text = var_to_str( item.get_meta( "amount" ) )
	_item_stack_max.text = var_to_str( _item.max_stack )
	
	_selected_item = item.get_meta( "node" )

func add_item( row: HBoxContainer, item_type: Resource, stack_amount: int, hash: int ) -> void:
	var _category: int = 0
	var _found: bool = false
	
	for i: int in item_type.categories.size():
		if _found:
			break
		
		var _id: String = item_type.categories[ i ].id
		match _id:
			"ITEM_CATEGORY_MISC", "ITEM_CATEGORY_CONSUMABLE", "ITEM_CATEGORY_AMMO", "ITEM_CATEGORY_WEAPON":
				_category = i
				_found = true
			_:
				pass
	
	if !_found:
		Console.PrintError( "Notebook.AddItem: invalid item category \"" + item_type.categories[ _category ].id + "\"" )
		return
	
	if row.get_child_count() == _max_items_per_row:
		row = HBoxContainer.new()
		_stack_list.add_child( row )
	
	var _item: TextureRect = TextureRect.new()
	row.add_child( _item )
	
	_item.texture = item_type.icon
	_item.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	_item.custom_minimum_size = _backpack_item_minimum_size
	_item.connect( "gui_input", func( gui_event: InputEvent ): _on_backpack_item_selected( gui_event, _item ) )
	_item.set_meta( "item", item_type )
	_item.set_meta( "amount", stack_amount )
	_item.set_meta( "category", item_type.categories[ _category ].name )
	_item.set_meta( "node", _item.get_path() ) 
	_item.set_meta( "hash", hash )
	
	if item_type.properties.has( "description" ):
		_item.set_meta( "description", item_type.properties[ "description" ] )
	if item_type.properties.has( "effects" ):
		_item.set_meta( "effects", item_type.properties[ "effects" ] )

func add_ammo_stack_to_backpack( row: HBoxContainer, stack: Node ) -> HBoxContainer:
	add_item( row, stack.AmmoType.Data, stack.Amount, stack.get_meta( "hash" ) )
	return row

func add_weapon_to_backpack( row: HBoxContainer, weapon: Node ) -> HBoxContainer:
	add_item( row, weapon.Data, 1, weapon.get_meta( "hash" ) )
	return row

func add_item_to_backpack( row: HBoxContainer, stack: Resource ) -> HBoxContainer:
	add_item( row, _owner.GetInventory().database.get_item( stack.item_id ), stack.amount, stack.get_meta( "hash" ) )
	return row
