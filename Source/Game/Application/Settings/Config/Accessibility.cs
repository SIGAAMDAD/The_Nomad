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

using CVars;

namespace Settings.Config {
	/*
	===================================================================================
	
	Accessibility
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class Accessibility {
		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<float> HapticStrength = new CVar<float>(
			name: "input.HapticStrength",
			defaultValue: 50.0f,
			description: "Sets the intensity of haptic feedback effects.",
			flags: CVarFlags.Archive,
			value => value >= 0.0f && value <= 100.0f
		);

		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<bool> HapticEnabled = new CVar<bool>(
			name: "input.HapticEnabled",
			defaultValue: true,
			description: "Enables haptic feedback effects.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<ColorblindMode> ColorblindMode = new CVar<ColorblindMode>(
			name: "accessibility.ColorblindMode",
			defaultValue: Settings.ColorblindMode.None,
			description: "Sets the colorblind mode for the game, enabling makes different elements be set to varying color values.",
			flags: CVarFlags.Archive,
			value => value >= Settings.ColorblindMode.None && value < Settings.ColorblindMode.Count
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> DyslexiaMode = new CVar<bool>(
			name: "accessibility.DyslexiaMode",
			defaultValue: false,
			description: "Switches all fonts in the game to the OpenDyslexia font.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<float> UIScale = new CVar<float>(
			name: "accessibility.UIScale",
			defaultValue: 1.0f,
			description: "Sets the scaling of in-game User Interface elements.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<AutoAimMode> AutoAimMode = new CVar<AutoAimMode>(
			name: "accessibility.AutoAimMode",
			defaultValue: Settings.AutoAimMode.Off,
			description: "",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> TextToSpeech = new CVar<bool>(
			name: "accessibility.TextToSpeech",
			defaultValue: false,
			description: "Enables narration for in-game User Interface elements.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<int> TtsVoiceIndex = new CVar<int>(
			name: "accessibility.TtsVoiceIndex",
			defaultValue: 0,
			description: "Sets the index for which text to speech voice to use.",
			flags: CVarFlags.Archive
		);
	};
};