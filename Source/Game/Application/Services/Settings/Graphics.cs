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

using NomadCore.Enums.ConsoleSystem;
using NomadCore.Interfaces;
using NomadCore.Systems.ConsoleSystem.CVars.Common;
using NomadCore.Utilities;
using Godot;
using System.Collections.Generic;
using NomadCore.Abstractions.Services;
using NomadCore.Interfaces.ConsoleSystem;
using System;
using Game.Domain.Enums.Settings;

namespace Game.Domain.DomainServices.Settings {
	/*
	===================================================================================
	
	Graphics
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class Graphics : ConfigHandler, IGameService {
		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> BakedLights;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<int> ShadowAtlasSize;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<float> ShadowFilterSmooth;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<Light2D.ShadowFilterEnum> ShadowFilterType;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<ParticleQuality> ParticleQuality;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<AnimationQuality> AnimationQuality;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> PhysicallyBasedRendering;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> BloomEnabled;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> ForceVertexShading;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> FootstepsEnabled;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<QualitySetting> Preset;

		public override Dictionary<ICVar, object>? Cvars => _cvars.Value;
		private readonly Lazy<Dictionary<ICVar, object>?> _cvars;

		/*
		===============
		Graphics
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public Graphics( ICVarSystemService cvarSystem ) {
			BakedLights = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.BakedLights",
					defaultValue: false,
					description: "Forces the usage of Sprite2D lights with Additive CanvasModulate, if this is toggled on, shadows are disabled as PointLight2D nodes aren't used.",
					flags: CVarFlags.Archive
				)
			);

			ShadowAtlasSize = (CVar<int>)cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "r.ShadowAtlasSize",
					defaultValue: 2048,
					description: "Sets godot's 2D shadow atlas size, will always be rounded to a power of two. Higher values have an increased effect on performance.",
					flags: CVarFlags.Archive
				)
			);

			ShadowFilterSmooth = (CVar<float>)cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "r.ShadowFilterSmooth",
					defaultValue: 0.0f,
					description: "Sets in-game 2D shadow filtering smoothness.",
					flags: CVarFlags.Archive,
					validator: value => value >= 0.0f && value <= 1.0f
				)
			);

			ShadowFilterType = (CVar<Light2D.ShadowFilterEnum>)cvarSystem.Register(
				new CVarCreateInfo<Light2D.ShadowFilterEnum>(
					name: "r.ShadowFilterType",
					defaultValue: Light2D.ShadowFilterEnum.None,
					description: "Sets the in-game 2D shadow filtering quality, higher values have a heavy impact on performance.",
					flags: CVarFlags.Archive,
					validator: value => value >= Light2D.ShadowFilterEnum.None && value <= Light2D.ShadowFilterEnum.Pcf5
				)
			);

			ParticleQuality = (CVar<ParticleQuality>)cvarSystem.Register(
				new CVarCreateInfo<ParticleQuality>(
					name: "r.ParticleQuality",
					defaultValue: Enums.Settings.ParticleQuality.Low,
					description: "Sets the game's quality of particles.",
					flags: CVarFlags.Archive,
					validator: value => value >= Enums.Settings.ParticleQuality.Low && value < Enums.Settings.ParticleQuality.Count
				)
			);

			AnimationQuality = (CVar<AnimationQuality>)cvarSystem.Register(
				new CVarCreateInfo<AnimationQuality>(
					name: "r.AnimationQuality",
					defaultValue: Enums.Settings.AnimationQuality.Low,
					description: "Sets the quality of in-game animations, performance isn't impacted heavily by this (CPU only).",
					flags: CVarFlags.Archive,
					validator: value => value >= Enums.Settings.AnimationQuality.Low && value < Enums.Settings.AnimationQuality.Count
				)
			);

			PhysicallyBasedRendering = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.PhysicallyBasedRendering",
					defaultValue: false,
					description: "Forces usage of the burley based (Disney/pixar animation studio) lighting model over lambert. Has an impact on performance.",
					flags: CVarFlags.Archive
				)
			);

			BloomEnabled = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.BloomEnabled",
					defaultValue: true,
					description: "Enables a bloom post-process effect, requires support of 16-bit floating point framebuffers (HDR).",
					flags: CVarFlags.Archive
				)
			);

			ForceVertexShading = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.ForceVertexShading",
					defaultValue: true,
					description: "Forces vertex shading for rendering. Heavily reduces quality, but drastically increases performance",
					flags: CVarFlags.Archive
				)
			);

			FootstepsEnabled = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.FootstepsEnabled",
					defaultValue: true,
					description: "Enables in-game footstep generation. Has a very small impact on performance (CPU only).",
					flags: CVarFlags.Archive
				)
			);

			Preset = (CVar<QualitySetting>)cvarSystem.Register(
				new CVarCreateInfo<QualitySetting>(
					name: "r.Preset",
					defaultValue: QualitySetting.Normal,
					description: "Premade templates for graphical quality adjustments.",
					flags: CVarFlags.Archive
				)
			);

			_cvars = new Lazy<Dictionary<ICVar, object>?>(
				() => new Dictionary<ICVar, object>() {
					{ BakedLights, false },
					{ BloomEnabled, true },
					{ ForceVertexShading, false },
					{ PhysicallyBasedRendering, true },
					{ AnimationQuality, Enums.Settings.AnimationQuality.Medium },
					{ ParticleQuality, Enums.Settings.ParticleQuality.Low },
					{ ShadowAtlasSize, 2048 },
					{ ShadowFilterType, ShadowFilterQuality.Low },
					{ ShadowFilterSmooth, 0.10f },
					{ FootstepsEnabled, true },
					{ Preset, QualitySetting.Normal }
				}
			);
		}
	};
};