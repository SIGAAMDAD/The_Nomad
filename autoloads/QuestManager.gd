class_name QuestStateManager extends Node

var _current_quest:QuestResource = null
var _quests:Array[ QuestResource ] = []
var _quest_state:Dictionary = {}

func _on_quest_completed( quest: QuestResource ) -> void:
	pass

func _on_condition_query_requested( type: String, key: String, value: Variant, requester: QuestCondition ) -> void:
	if type.begins_with( "var" ):
		var operator := type.get_slice( ":", 1 )
		var variable = _quest_state[ key ]
		var result := false
		match operator:
			type, "eq", "==":
				result = variable == value
			"neq", "ne", "!eq", "!=":
				result = variable != value
			"lt", "<":
				assert( not variable is bool, "incorrect variable type for quest condition query operator" )
				result = variable < value
			"lte", "<=":
				assert( not variable is bool, "incorrect variable type for quest condition query operator" )
				result = variable <= value
			"gt", ">":
				assert( not variable is bool, "incorrect variable type for quest condition query operator" )
				result = variable > value
			"gte", ">=":
				assert( not variable is bool, "incorrect variable type for quest condition query operator" )
				result = variable >= value
			_:
				printerr( "unknown operator '%s' in quest condition query" % operator )
		
		requester.set_completed( result )

func _on_quest_started( quest: QuestResource ) -> void:
	_current_quest = quest
	_quest_state.clear()

func _ready() -> void:
	Questify.quest_started.connect( _on_quest_started )
	Questify.quest_completed.connect( _on_quest_completed )
	Questify.condition_query_requested.connect( _on_condition_query_requested )

func set_state( key: String, value: Variant ) -> void:
	_quest_state[ key ] = value

func save( file: FileAccess ) -> void:
	pass
