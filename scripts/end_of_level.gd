class_name LevelStats extends Control

@onready var _level_name:Label = $MarginContainer/VBoxContainer/LevelNameLabel
@onready var _time_score:Label = $MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer2/TimeLabel
@onready var _kills_score:Label = $MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer2/KillsLabel

@onready var _deaths_score:Label = $MarginContainer/VBoxContainer/HBoxContainer3/VBoxContainer2/DeathsLabel
@onready var _collateral_score:Label = $MarginContainer/VBoxContainer/HBoxContainer3/VBoxContainer2/CollateralLabel

var _hellbreaks:int = 0
var _death_count:int = 0
var _kill_count:int = 0
var _collateral_amount:int = 0

var _total_score:int = 0

func add_death() -> void:
	_death_count += 1

func add_kill() -> void:
	_kill_count += 1

func calc_scores() -> void:
	if _death_count == 0:
		# deathless bonus
		_total_score += 10000
	else:
		_total_score += 100 * _hellbreaks
