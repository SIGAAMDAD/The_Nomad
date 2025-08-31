#
# ===========================================================================
# The Nomad AGPL Source Code
# Copyright (C) 2025 Noah Van Til
#
# The Nomad Source Code is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as published
# by the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# The Nomad Source Code is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
#
# You should have received a copy of the GNU Affero General Public License
# along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.
#
# If you have questions concerning this license or the applicable additional
# terms, you may contact me via email at nyvantil@gmail.com.
# ===========================================================================
#

extends CanvasLayer

signal transition_finished

@onready var _color_rect:ColorRect = $ColorRect
@onready var _animation_player:AnimationPlayer = $AnimationPlayer


#
# ===============
# _on_animation_finished
# ===============
#
func _on_animation_finished( animationName: String ) -> void:
	if animationName == "fade_to_black":
		transition_finished.emit()
		_animation_player.play( "fade_to_normal" )
	elif animationName == "fade_to_normal":
		_color_rect.hide()


#
# ===============
# _transition
# ===============
#
func transition() -> void:
	_color_rect.show()
	_animation_player.play( "fade_to_black" )


#
# ===============
# _ready
# ===============
#
func _ready() -> void:
	_animation_player.animation_finished.connect( _on_animation_finished )
