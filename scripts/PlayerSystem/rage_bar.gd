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

class_name RageBar extends ProgressBar

@onready var _show_timer: Timer = $ShowTimer

var Rage: float:
	get:
		return Rage
	set( value ):
		process_mode = PROCESS_MODE_PAUSABLE
		modulate = Color( 1.0, 1.0, 1.0, 1.0 )
		_show_timer.start()
		self.value = value

#
# ===============
# init
# ===============
#
func init( rage: float ) -> void:
	max_value = rage
	value = rage


#
# ===============
# _on_show_timer_timeout
# ===============
#
func _on_show_timer_timeout() -> void:
	var _tweener: Tween = create_tween()
	_tweener.tween_property( self, "modulate", Color( 0.0, 0.0, 0.0, 0.0 ), 1.0 )
	_tweener.connect( "finished", func(): process_mode = PROCESS_MODE_DISABLED )
