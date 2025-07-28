extends Node

signal apply_flash
signal apply_shake

var active = true

func apply_camera_flash(color: Color, duration = 0.3):
	if not active: return
	apply_flash.emit(color, duration)

func apply_camera_shake(force: float = 2.0, duration = 0.4):
	if not active: return
	apply_shake.emit(force, duration)
