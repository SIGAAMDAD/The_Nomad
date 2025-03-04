extends Node

var _target_spotted:Array[ AudioStream ]
var _man_down_1:Array[ AudioStream ]
var _man_down_2:AudioStream
var _man_down_3:AudioStream
var _heavy_dead:Array[ AudioStream ]
var _curse:Array[ AudioStream ]
var _deaf:AudioStream
var _ceasefire:Array[ AudioStream ]
var _alert:Array[ AudioStream ]
var _confusion:Array[ AudioStream ]

func _ready() -> void:
	pass

func cache() -> void:
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21199.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21167.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21200.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21201.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21202.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21203.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21204.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21205.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21207.mp3" ) )
	_target_spotted.push_back( ResourceLoader.load( "res://sounds/barks/21208.mp3" ) )
	_man_down_1.push_back( ResourceLoader.load( "res://sounds/barks/21348.mp3" ) )
	_man_down_1.push_back( ResourceLoader.load( "res://sounds/barks/21359.mp3" ) )
	_man_down_2 = ResourceLoader.load( "res://sounds/barks/man_down_2_callout_0.mp3" )
	_man_down_3 = ResourceLoader.load( "res://sounds/barks/men_down_3_callout_0.mp3" )
	_heavy_dead.push_back( ResourceLoader.load( "res://sounds/barks/14859.mp3" ) )
	_heavy_dead.push_back( ResourceLoader.load( "res://sounds/barks/14860.mp3" ) )
	_heavy_dead.push_back( ResourceLoader.load( "res://sounds/barks/14861.mp3" ) )
	_curse.push_back( ResourceLoader.load( "res://sounds/barks/21009.mp3" ) )
	_curse.push_back( ResourceLoader.load( "res://sounds/barks/21010.mp3" ) )
	_curse.push_back( ResourceLoader.load( "res://sounds/barks/21011.mp3" ) )
	_deaf = ResourceLoader.load( "res://sounds/barks/deaf_callout.mp3" )
#	_ceasefire.push_back( ResourceLoader.load( "res://sounds/barks/ceasefire_cmd_0.mp3" ) )
#	_ceasefire.push_back( ResourceLoader.load( "res://sounds/barks/ceasefire_cmd_1.mp3" ) )
#	_ceasefire.push_back( ResourceLoader.load( "res://sounds/barks/ceasefire_cmd_2.mp3" ) )
#	_ceasefire.push_back( ResourceLoader.load( "res://sounds/barks/ceasefire_cmd_3.mp3" ) )
	_alert.push_back( ResourceLoader.load( "res://sounds/barks/21164.mp3" ) )
	_alert.push_back( ResourceLoader.load( "res://sounds/barks/21170.mp3" ) )
	_alert.push_back( ResourceLoader.load( "res://sounds/barks/21169.mp3" ) )
	_confusion.push_back( ResourceLoader.load( "res://sounds/barks/21028.mp3" ) )
	_confusion.push_back( ResourceLoader.load( "res://sounds/barks/21029.mp3" ) )
	_confusion.push_back( ResourceLoader.load( "res://sounds/barks/21033.mp3" ) )
	_confusion.push_back( ResourceLoader.load( "res://sounds/barks/21034.mp3" ) )
	_confusion.push_back( ResourceLoader.load( "res://sounds/barks/21030.mp3" ) )

func target_spotted() -> AudioStream:
	return _target_spotted[ randi_range( 0, _target_spotted.size() - 1 ) ]
