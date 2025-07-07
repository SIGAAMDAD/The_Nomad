class_name Journal extends TabBar

var _owner: CharacterBody2D

@onready var _quest_name: Label = $"MarginContainer/TabContainer/Quest Log/ActiveQuestContainer/QuestNameLabel"
@onready var _quest_objective: Label = $"MarginContainer/TabContainer/Quest Log/ActiveQuestContainer/QuestObjectiveLabel"
@onready var _quest_type: Label = $"MarginContainer/TabContainer/Quest Log/ActiveQuestContainer/QuestTypeLabel"
@onready var _contract_data: VBoxContainer = $"MarginContainer/TabContainer/Quest Log/ActiveQuestContainer/ContractData"
@onready var _contract_employer: Label = $"MarginContainer/TabContainer/Quest Log/ActiveQuestContainer/ContractData/ContractEmployer"
@onready var _contract_pay: Label = $"MarginContainer/TabContainer/Quest Log/ActiveQuestContainer/ContractData/ContractPay"
@onready var _contract_guild: Label = $"MarginContainer/TabContainer/Quest Log/ActiveQuestContainer/ContractData/ContractGuild"
@onready var _quest_description: Label = $"MarginContainer/TabContainer/Quest Log/ActiveQuestContainer/QuestDescription"

func _ready() -> void:
	_owner = get_node( "/root/LevelData" ).ThisPlayer
