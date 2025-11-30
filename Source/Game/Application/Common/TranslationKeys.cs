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

namespace Game.Application.Common {
	public static class TranslationKeys {
		public const string UiOff = "UI_OFF";
		public const string UiOn = "UI_ON";

		public static class Graphics {
			public const string QualityPreset = "SETTINGS_GRAPHICS_PRESET";
			public const string LightingQuality = "SETTINGS_LIGHTING_QUALITY";
			public const string EffectsQuality = "SETTINGS_EFFECTS_QUALITY";

			public const string QualityLow = "QUALITY_LOW";
			public const string QualityNormal = "QUALITY_NORMAL";
			public const string QualityHigh = "QUALITY_HIGH";
			public const string QualityUltra = "QUALITY_ULTRA";

			public const string ShadowFilterType = "SETTINGS_SHADOW_FILTER_TYPE";

			public const string ShadowFilterTypeOff = "SHADOW_FILTER_OFF";
			public const string ShadowFilterTypeHard = "SHADOW_FILTER_HARD";
			public const string ShadowFilterTypeSoft = "SHADOW_FILTER_SOFT";
		};
		public static class Display {
			public const string AspectRatio = "SETTINGS_ASPECT_RATIO";

			public const string AspectRatioAutomatic = "ASPECT_RATIO_AUTOMATIC";

			public const string WindowMode = "SETTINGS_WINDOWMODE";

			public const string WindowModeWindowed = "WINDOWMODE_WINDOWED";
			public const string WindowModeBorderlessWindowed = "WINDOWMODE_BORDERLESS_WINDOWED";
			public const string WindowModeFullscreen = "WINDOWMODE_FULLSCREEN";
			public const string WindowModeBorderlessFullscreen = "WINDOWMODE_BORDERLESS_FULLSCREEN";
			public const string WindowModeExclusiveFullscreen = "WINDOWMODE_EXCLUSIVE_FULLSCREEN";

			public const string AntiAliasing = "SETTINGS_ANTIALIASING";

			public const string AntiAliasingEdgeAA = "ANTIALIASING_EDGE_AA";
			public const string AntiAliasingScreenSpaceAA = "ANTIALIASING_SCREENSPPACE_AA";
			public const string AntiAliasingOff = "ANTIALIASING_OFF";
			public const string AntiAliasingTaa = "ANTIALIASING_TAA";
			public const string AntiAliasingFxaa = "ANTIALIASING_FXAA";
			public const string AntiAliasingSmaa = "ANTIALIASING_SMAA";
			public const string AntiAliasingMsaa2X = "ANTIALIASING_MSAA2X";
			public const string AntiAliasingMsaa4X = "ANTIALIASING_MSAA4X";
			public const string AntiAliasingMsaa8X = "ANTIALIASING_MSAA8X";
			public const string AntiAliasingFxaaAndTaa = "ANTIALIASING_FXAA_AND_TAA";

			public const string WindowResolution = "SETTINGS_WINDOW_RESOLUTION";

			public const string SeparateRenderingThread = "SETTINGS_SEPARATE_RENDERING_THREAD";
			public const string MaxFps = "SETTINGS_MAX_FPS";
		};
		public static class Audio {
			public const string SoundEffectsVolume = "SETTINGS_EFFECTS_VOLUME";
			public const string SoundEffectsOn = "SETTINGS_EFFECTS_ON";

			public const string MusicVolume = "SETTINGS_MUSIC_VOLUME";
			public const string MusicOn = "SETTINGS_MUSIC_ON";

			public const string AudioDriver = "SETTINGS_AUDIO_DRIVER_NAME";
			public const string OutputAudioDevice = "SETTINGS_OUTPUT_AUDIO_DEVICE";
		};
	};
};