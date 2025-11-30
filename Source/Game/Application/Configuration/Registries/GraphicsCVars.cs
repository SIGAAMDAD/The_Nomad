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
using NomadCore.Abstractions.Services;
using NomadCore.Enums.ConsoleSystem;
using NomadCore.Utilities;

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
					name: "r.BakedLights",
					defaultValue: false,
					description: "Forces the usage of Sprite2D lights with Additive CanvasModulate, if this is toggled on, shadows are disabled as PointLight2D nodes aren't used.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<ShadowAtlasSize>(
					name: "r.ShadowAtlasSize",
					defaultValue: ShadowAtlasSize.Default,
					description: "Sets godot's 2D shadow atlas size, will always be rounded to a power of two. Higher values have an increased effect on performance.",
					flags: CVarFlags.Archive,
					validator: value => value >= ShadowAtlasSize.Size1024 && value < ShadowAtlasSize.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "r.ShadowFilterSmooth",
					defaultValue: 0.0f,
					description: "Sets in-game 2D shadow filtering smoothness.",
					flags: CVarFlags.Archive,
					validator: value => value >= 0.0f && value <= 1.0f
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<ShadowFilterQuality>(
					name: "r.ShadowFilterType",
					defaultValue: ShadowFilterQuality.Default,
					description: "Sets the in-game 2D shadow filtering quality, higher values have a heavy impact on performance.",
					flags: CVarFlags.Archive,
					validator: value => value >= ShadowFilterQuality.Off && value < ShadowFilterQuality.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<ParticleQuality>(
					name: "r.ParticleQuality",
					defaultValue: ParticleQuality.Low,
					description: "Sets the game's quality of particles.",
					flags: CVarFlags.Archive,
					validator: value => value >= ParticleQuality.Low && value < ParticleQuality.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<AnimationQuality>(
					name: "r.AnimationQuality",
					defaultValue: AnimationQuality.Low,
					description: "Sets the quality of in-game animations, performance isn't impacted heavily by this (CPU only).",
					flags: CVarFlags.Archive,
					validator: value => value >= AnimationQuality.Low && value < AnimationQuality.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.PhysicallyBasedRendering",
					defaultValue: false,
					description: "Forces usage of the burley based (Disney/pixar animation studio) lighting model over lambert. Has an impact on performance.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.BloomEnabled",
					defaultValue: true,
					description: "Enables a bloom post-process effect, requires support of 16-bit floating point framebuffers (HDR).",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.ForceVertexShading",
					defaultValue: true,
					description: "Forces vertex shading for rendering. Heavily reduces quality, but drastically increases performance",
					flags: CVarFlags.Archive
				)
			);
		}
	};
};