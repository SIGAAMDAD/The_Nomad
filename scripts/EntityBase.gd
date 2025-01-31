class_name EntityBase extends CharacterBody2D

enum Type {
	Item,
	Weapon,
	Mob,
	Player,
};

signal _on_damage( damage: float )
signal _on_death( attacker: EntityBase )

@export var _name:String = ""
@export var _type:Type = Type.Item
