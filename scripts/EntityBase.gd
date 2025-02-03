class_name EntityBase extends CharacterBody2D

enum Type {
	Item,
	Weapon,
	Mob,
	Player,
};

signal on_damage( damage: float )
signal on_death( attacker: EntityBase, target: EntityBase )

@export var _name:String = ""
@export var _type:Type = Type.Item
