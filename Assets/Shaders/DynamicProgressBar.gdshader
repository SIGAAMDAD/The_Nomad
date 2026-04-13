shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0) = 0.5;
uniform vec4 background_color : source_color = vec4(0.5, 0.5, 0.5, 1.0);
uniform sampler2D color_gradient : source_color;
uniform sampler2D mask_texture : hint_default_white;

// Wave effect parameters
uniform float wave_amplitude : hint_range(0.0, 0.1) = 0.02;
uniform float wave_frequency : hint_range(0.0, 10.0) = 3.0;
uniform float wave_speed : hint_range(0.0, 5.0) = 1.5;

// Particle effect parameters
uniform float particle_speed : hint_range(0.01, 0.5) = 0.05;
uniform float particle_lifetime : hint_range(1.0, 10.0) = 3.0;
uniform float particle_softness : hint_range(0.001, 1.0) = 0.01;
uniform float particle_spacing : hint_range(0.01, 1.0) = 0.1;
uniform vec2 particle_offset = vec2(0.0, 0.0);
uniform vec2 particle_start_scale = vec2(0.02, 0.02);
uniform vec2 particle_end_scale = vec2(0.01, 0.01);
uniform float particle_scale_randomness : hint_range(0.0, 1.0) = 0.5;
uniform sampler2D particle_alpha_gradient : source_color;
uniform int particle_amount : hint_range(0, 50) = 20;

// Pseudo-random function
float rand(vec2 co) {
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

// Function to generate a single particle
float particle(vec2 uv, float particle_id, float time, float mask_value) {
    float random_offset = rand(vec2(particle_id, 0.0));
    float spawn_time = particle_id * particle_spacing;
    float life = mod(time - spawn_time, particle_lifetime) / particle_lifetime;
    
    if (life < 0.0 || life >= 1.0) return 0.0;
    
    float vertical_pos = rand(vec2(particle_id, 1.0));
    float horizontal_pos = progress + life * particle_speed;
    
    vec2 particle_pos = vec2(horizontal_pos, vertical_pos) + particle_offset;
    
    // Calculate scale with randomness
    float scale_random = rand(vec2(particle_id, 2.0)) * particle_scale_randomness;
    vec2 start_scale = particle_start_scale * (1.0 - scale_random);
    vec2 end_scale = particle_end_scale * (1.0 - scale_random);
    vec2 current_scale = mix(start_scale, end_scale, life);
    
    vec2 scaled_uv = (uv - particle_pos) / current_scale;
    float dist = length(scaled_uv);
    
    float alpha = texture(particle_alpha_gradient, vec2(life, 0.5)).a;
    
    return smoothstep(1.0 + particle_softness, 1.0, dist) * alpha * mask_value;
}

void fragment() {
    vec2 uv = UV;
    
    // Sample the mask texture or use default (fully visible) if not provided
    float mask = texture(mask_texture, uv).r;
    
    // Calculate wave offset
    float wave_offset = wave_amplitude * sin(wave_frequency * uv.y + TIME * wave_speed);
    
    // Apply wave offset to progress
    float wave_progress = progress + wave_offset;
    
    // Sample the color from the gradient texture based on progress
    vec4 gradient_color = texture(color_gradient, vec2(progress, 0.5));
    
    // Start with the background color
    vec4 final_color = background_color;
    
    // Apply the progress bar with mask
    if (uv.x < wave_progress) {
        final_color = mix(background_color, gradient_color, mask);
    }
    
    // Generate particles
    float particles = 0.0;
    for (float i = 0.0; i < float(particle_amount); i++) {
        particles += particle(uv, i, TIME, mask);
    }
    particles = clamp(particles, 0.0, 1.0);
    
    // Add particles to the final color
    final_color = mix(final_color, gradient_color, particles * step(uv.x, 1.0));
    
    COLOR = final_color;
}