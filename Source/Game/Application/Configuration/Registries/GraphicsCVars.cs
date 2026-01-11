/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Game.Application.Configuration.Enums;
using Nomad.Core;
using Nomad.CVars;

namespace Game.Application.Configuration.Registries {
	/*
	===================================================================================
	
	GraphicsCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class GraphicsCVars {
		/*
		===============
		Register
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public static void Register( ICVarSystemService cvarSystem ) {
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Graphics.BAKED_LIGHTS,
					DefaultValue: false,
					Description: "Forces the usage of Sprite2D lights with Additive CanvasModulate, if this is toggled on, shadows are disabled as PointLight2D nodes aren't used.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<ShadowAtlasSize>(
					Name: Constants.CVars.Graphics.SHADOW_ATLAS_SIZE,
					DefaultValue: ShadowAtlasSize.Default,
					Description: "Sets godot's 2D shadow atlas size, will always be rounded to a power of two. Higher values have an increased effect on performance.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= ShadowAtlasSize.Size1024 && value < ShadowAtlasSize.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Graphics.SHADOW_FILTER_SMOOTH,
					DefaultValue: 0.0f,
					Description: "Sets in-game 2D shadow filtering smoothness.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= 0.0f && value <= 1.0f
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<ShadowFilterQuality>(
					Name: Constants.CVars.Graphics.SHADOW_FILTER_TYPE,
					DefaultValue: ShadowFilterQuality.Default,
					Description: "Sets the in-game 2D shadow filtering quality, higher values have a heavy impact on performance.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= ShadowFilterQuality.Off && value < ShadowFilterQuality.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<ParticleQuality>(
					Name: Constants.CVars.Graphics.PARTICLE_QUALITY,
					DefaultValue: ParticleQuality.Low,
					Description: "Sets the game's quality of particles.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= ParticleQuality.Low && value < ParticleQuality.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<AnimationQuality>(
					Name: Constants.CVars.Graphics.ANIMATION_QUALITY,
					DefaultValue: AnimationQuality.Low,
					Description: "Sets the quality of in-game animations, performance isn't impacted heavily by this (CPU only).",
					Flags: CVarFlags.Archive,
					Validator: value => value >= AnimationQuality.Low && value < AnimationQuality.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Graphics.PHYSICALLY_BASED_RENDERING,
					DefaultValue: false,
					Description: "Forces usage of the burley based (Disney/pixar animation studio) lighting model over lambert. Has an impact on performance.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Graphics.BLOOM_ENABLED,
					DefaultValue: true,
					Description: "Enables a bloom post-process effect, requires support of 16-bit floating point framebuffers (HDR).",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Graphics.FORCE_VERTEX_SHADING,
					DefaultValue: true,
					Description: "Forces vertex shading for rendering. Heavily reduces quality, but drastically increases performance",
					Flags: CVarFlags.Archive
				)
			);
		}
	};
};