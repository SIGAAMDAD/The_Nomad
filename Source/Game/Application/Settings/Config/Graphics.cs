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

using Godot;
using CVars;

namespace Settings.Config {
	/*
	===================================================================================
	
	Graphics
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class Graphics {
		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> BakedLights = new CVar<bool>(
			name: "r.BakedLights",
			defaultValue: false,
			description: "Forces the usage of Sprite2D lights with Additive CanvasModulate, if this is toggled on, shadows are disabled as PointLight2D nodes aren't used.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<int> ShadowAtlasSize = new CVar<int>(
			name: "r.ShadowAtlasSize",
			defaultValue: 2048,
			description: "Sets godot's 2D shadow atlas size, will always be rounded to a power of two. Higher values have an increased effect on performance.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<float> ShadowFilterSmooth = new CVar<float>(
			name: "r.ShadowFilterSmooth",
			defaultValue: 0.0f,
			description: "Sets in-game 2D shadow filtering smoothness",
			flags: CVarFlags.Archive,
			value => value >= 0.0f && value <= 1.0f
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<Light2D.ShadowFilterEnum> ShadowFilterType = new CVar<Light2D.ShadowFilterEnum>(
			name: "r.ShadowFilterType",
			defaultValue: Light2D.ShadowFilterEnum.None,
			description: "Sets the in-game 2D shadow filtering quality. Higher values have a heavy impact on performance.",
			flags: CVarFlags.Archive,
			value => value >= Light2D.ShadowFilterEnum.None && value <= Light2D.ShadowFilterEnum.Pcf5
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<ParticleQuality> ParticleQuality = new CVar<ParticleQuality>(
			name: "r.ParticleQuality",
			defaultValue: Settings.ParticleQuality.Low,
			description: "Sets the game's quality of particles.",
			flags: CVarFlags.Archive,
			value => value >= Settings.ParticleQuality.Low && value < Settings.ParticleQuality.Count
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<AnimationQuality> AnimationQuality = new CVar<AnimationQuality>(
			name: "r.AnimationQuality",
			defaultValue: Settings.AnimationQuality.Low,
			description: "Sets the quality of in-game animations. Performance isn't impacted heavily by this.",
			flags: CVarFlags.Archive,
			value => value >= Settings.AnimationQuality.Low && value < Settings.AnimationQuality.Count
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> PhysicallyBasedRendering = new CVar<bool>(
			name: "r.PhysicallyBasedRendering",
			defaultValue: false,
			description: "Forces usage of the burley based (Disney/pixar animation studio) lighting model over lambert. Has an impact on performance.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> BloomEnabled = new CVar<bool>(
			name: "r.BloomEnabled",
			defaultValue: true,
			description: "Enables a bloom post-process effect. Requires support of 16-bit floating point framebuffers (HDR).",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> ForceVertexShading = new CVar<bool>(
			name: "r.ForceVertexShading",
			defaultValue: true,
			description: "Forces vertex shading for rendering. Heavily reduces quality, but drastically increases performance",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> FootstepsEnabled = new CVar<bool>(
			name: "r.FootstepsEnabled",
			defaultValue: true,
			description: "Enables in-game footstep generation. Has a very small impact on performance.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<QualitySetting> Preset = new CVar<QualitySetting>(
			name: "r.Preset",
			defaultValue: QualitySetting.Normal,
			description: "Premade templates for graphical quality adjustments.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<LightingQuality> LightingPreset = new CVar<LightingQuality>(
			name: "r.LightingPreset",
			defaultValue: LightingQuality.Normal,
			description: "",
			flags: CVarFlags.Archive
		);
	};
};