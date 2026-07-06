/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Godot;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Gameplay.Player.JumpKit
{
	internal sealed class PlayerJumpKitVisualFeedback
	{
		private readonly SpotLight3D? _light;
		private readonly GpuParticles3D? _particles;

		public PlayerJumpKitVisualFeedback(
			PlayerPrefab prefab,
			string dashLightPath,
			string dashParticlesPath
		)
		{
			_light = string.IsNullOrEmpty( dashLightPath )
				? null
				: prefab.GetNodeOrNull<SpotLight3D>( dashLightPath );

			_particles = string.IsNullOrEmpty( dashParticlesPath )
				? null
				: prefab.GetNodeOrNull<GpuParticles3D>( dashParticlesPath );
		}

		public void SetDashActive( bool active )
		{
			if ( _light != null ) {
				_light.Visible = active;
			}

			if ( _particles != null ) {
				_particles.Emitting = active;
			}
		}
	}
}
