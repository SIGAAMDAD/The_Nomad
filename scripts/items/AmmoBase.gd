class_name AmmoBase extends Resource

enum Type {
	Heavy,
	Light,
	Pellets
};

enum ShotgunBullshit {
	Flechette,
	Buckshot,
	Birdshot,
	Shrapnel,
	Slug,
	
	None
};

@export var _name:String = ""
@export var _damage:float = 0.0

@export var _ammo_type:Type = Type.Heavy
@export var _shotgun_bullshit:ShotgunBullshit = 0
@export var _item_stack_add:int = 0
@export var _icon:ImageTexture = null

@export var _pickup_sfx:AudioStream = null
