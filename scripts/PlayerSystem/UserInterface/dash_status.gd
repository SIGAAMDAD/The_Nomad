#
# ===========================================================================
# Copyright (C) 2023-2025 Noah Van Til
#
# This file is part of The Nomad source code.
#
# The Nomad source code is free software; you can redistribute it
# and/or modify it under the terms of the GNU Affero General Public License as
# published by the Free Software Foundation; either version 2 of the License,
# or (at your option) any later version.
#
# The Nomad source code is distributed in the hope that it will be
# useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
# GNU Affero General Public License for more details.
#
# You should have received a copy of the GNU Affero General Public License
# along with The Nomad source code; if not, write to the Free Software
# Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
# ===========================================================================
#

class_name DashStatus extends TextureRect

@onready var _show_timer: Timer = $ShowTimer

var _max_value: float = 0.0

var DashBurnout: float:
	get:
		return DashBurnout
	set( value ):
		process_mode = PROCESS_MODE_PAUSABLE
		modulate = Color( 1.0, 1.0, 1.0, 1.0 )
		_show_timer.start()
		create_tween().tween_property( material, "shader_parameter/progress", value, 0.25 )


#
# ===============
# init
# ===============
#
func init( max_burnout: float ) -> void:
	_max_value = max_burnout
	material.set( "shader_parameter/progress", 0.0 )


#
# ===============
# _on_show_timer_timeout
# ===============
#
func _on_show_timer_timeout() -> void:
	var _tweener: Tween = create_tween()
	_tweener.tween_property( self, "modulate", Color( 0.0, 0.0, 0.0, 0.0 ), 1.0 )
	_tweener.connect( "finished", func(): process_mode = PROCESS_MODE_DISABLED )
