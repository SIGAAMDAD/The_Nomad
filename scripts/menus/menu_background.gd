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

extends Control

# TODO: make different intensity levels on the glow for a save's amount of meliora and/or sanity

const BLOOM_SCENE = preload( "res://autoloads/compatibility_bloom.tscn" )

#
# ===============
# _on_viewport_size_changed
# ===============
#
func _on_viewport_size_changed() -> void:
	var _window_size: Vector2 = get_viewport_rect().size
	var _extents: Vector3 = Vector3( _window_size.x / 2.0, 0.0, 0.0 )
	var _position: Vector2 = Vector2( _extents.x, _window_size.y )
	
	var _ember_emitter: GPUParticles2D = get_node( "EmberParticlesEmitter" )
	_ember_emitter.global_position = _position
	( _ember_emitter.process_material as ParticleProcessMaterial ).emission_box_extents = _extents
	
	var _sand_emitter: GPUParticles2D = get_node( "SandParticlesEmitter" )
	_sand_emitter.global_position = _position
	( _sand_emitter.process_material as ParticleProcessMaterial ).emission_box_extents = _extents
	
	$WorldEnvironment.environment.set( "glow_levels/7", lerp( 0.5, 8.0, _window_size.y / 2160 ) )


#
# ===============
# _ready
# ===============
#
func _ready() -> void:
	get_viewport().size_changed.connect( _on_viewport_size_changed )
	
	var _window_size: Vector2 = get_viewport_rect().size
	var _extents: Vector3 = Vector3( _window_size.x / 2.0, 0.0, 0.0 )
	var _position: Vector2 = Vector2( _extents.x, _window_size.y )
	
	$WorldEnvironment.environment.set( "glow_levels/7", lerp( 0.5, 8.0, 3840 / _window_size.y ) )
	
	var _ember_emitter: GPUParticles2D = get_node( "EmberParticlesEmitter" )
	_ember_emitter.global_position = _position
	( _ember_emitter.process_material as ParticleProcessMaterial ).emission_box_extents = _extents
	
	var _sand_emitter: GPUParticles2D = get_node( "SandParticlesEmitter" )
	_sand_emitter.global_position = _position
	( _sand_emitter.process_material as ParticleProcessMaterial ).emission_box_extents = _extents
	
	if RenderingServer.get_current_rendering_driver_name() != "vulkan":
		add_child( BLOOM_SCENE.instantiate() )
		remove_child( get_node( "WorldEnvironment" ) )
