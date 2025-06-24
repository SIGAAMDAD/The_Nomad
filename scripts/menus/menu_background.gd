extends Control

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

	if SettingsData.GetWindowMode() < 2:
		# windowed 
		$WorldEnvironment.environment.glow_intensity = 0.05
	else:
		# fullscreen mode, make it brighter
		$WorldEnvironment.environment.glow_intensity = 0.02

func _ready() -> void:
	get_viewport().size_changed.connect( _on_viewport_size_changed )
	
	var _window_size: Vector2 = get_viewport_rect().size
	var _extents: Vector3 = Vector3( _window_size.x / 2.0, 0.0, 0.0 )
	var _position: Vector2 = Vector2( _extents.x, _window_size.y )
	
	var _ember_emitter: GPUParticles2D = get_node( "EmberParticlesEmitter" )
	_ember_emitter.global_position = _position
	( _ember_emitter.process_material as ParticleProcessMaterial ).emission_box_extents = _extents
	
	var _sand_emitter: GPUParticles2D = get_node( "SandParticlesEmitter" )
	_sand_emitter.global_position = _position
	( _sand_emitter.process_material as ParticleProcessMaterial ).emission_box_extents = _extents
