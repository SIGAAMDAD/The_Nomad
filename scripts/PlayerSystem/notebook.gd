class_name Notebook extends MarginContainer

enum TabSelect {
	Backpack,
	Contracts,
	RecentEvents,
	Equipment,

	Count
};

const _backpack_item_minimum_size: Vector2 = Vector2( 64.0, 64.0 )
const _max_items_per_row: int = 4

var _owner: CharacterBody2D

@onready var _weapon_bladed_damage: Label = $NotebookContainer/TabContainer/Backpack/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BladedInfo/DamageLabel
@onready var _weapon_bladed_range: Label = $NotebookContainer/TabContainer/Backpack/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BladedInfo/RangeLabel
@onready var _weapon_blunt_damage: Label = $NotebookContainer/TabContainer/Backpack/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BluntInfo/DamageLabel
@onready var _weapon_blunt_range: Label = $NotebookContainer/TabContainer/Backpack/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/BluntInfo/RangeLabel
@onready var _weapon_ammodata: TextureRect = $NotebookContainer/TabContainer/Backpack/MarginContainer/VBoxContainer/HBoxContainer/ItemInfo/WeaponStatusContainer/AmmoLoaded/AmmoIcon

@onready var _item_name: Label
@onready var _item_type: Label
