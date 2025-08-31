#
#===========================================================================
# Copyright (C) 2023-2025 Noah Van Til
#
# This file is part of The Nomad source code.
#
# The Nomad source code is free software; you can redistribute it
# and/or modify it under the terms of the GNU General Public License as
# published by the Free Software Foundation; either version 2 of the License,
# or (at your option) any later version.
#
# The Nomad source code is distributed in the hope that it will be
# useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with Foobar; if not, write to the Free Software
# Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
#===========================================================================
#

extends CanvasLayer

signal transition_finished()

@onready var _color_rect:ColorRect = $ColorRect
@onready var _animation_player:AnimationPlayer = $AnimationPlayer

func _ready() -> void:
	_animation_player.animation_finished.connect( _on_animation_finished )

func _on_animation_finished( animationName: String ) -> void:
	if animationName == "fade_to_black":
		transition_finished.emit()
		_animation_player.play( "fade_to_normal" )
	elif animationName == "fade_to_normal":
		_color_rect.hide()

func transition() -> void:
	_color_rect.show()
	_animation_player.play( "fade_to_black" )
