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
using Microsoft.Extensions.Configuration.Ini;
using System;
using System.Collections.Generic;

namespace Menus.Settings {
	/*
	===================================================================================
	
	ConfigFileReader
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public readonly struct ConfigFileReader : IDisposable {
		private readonly IDictionary<string, string?>? IniData = null;

		/*
		===============
		ConfigFileReader
		===============
		*/
		/// <summary>
		/// Reads configuration values from configPath
		/// </summary>
		/// <remarks>
		/// If <paramref name="configPath"/> doesn't exist in the filesystem, the DefaultSettings values will
		/// be loaded instead
		/// </remarks>
		/// <param name="configPath">The path to load values from</param>

		public ConfigFileReader( string configPath ) {
			ArgumentException.ThrowIfNullOrEmpty( configPath );

			Console.PrintLine( $"Loading configuration data from {configPath}..." );

			LoadDefaultValues();

			string path = ProjectSettings.GlobalizePath( configPath );
			try {
				using var stream = new System.IO.FileStream( path, System.IO.FileMode.Open );

				// load the values
				IniData = IniStreamConfigurationProvider.Read( stream );
				LoadVideoSettings();
				LoadAudioSettings();
				LoadAccessibilitySettings();
				LoadNetworkingSettings();
			} catch ( System.IO.FileNotFoundException ) {
				Console.PrintLine( $"Configuration file {configPath} doesn't exist, applying default values..." );
			} catch ( Exception e ) {
				Console.PrintLine( $"Error loading file {configPath} (exception caught {e.Message}), applying default values..." );
			}
		}

		/*
		===============
		Dispose
		===============
		*/
		public void Dispose() {
		}

		/*
		===============
		LoadDefaultValues
		===============
		*/
		/// <summary>
		/// Loads default configuration values
		/// </summary>
		/// <exception cref="Exception"></exception>
		private static void LoadDefaultValues() {
			try {
				using DefaultSettings? settings = ResourceLoader.Load<DefaultSettings>( "res://resources/DefaultSettings.tres" )
					?? throw new Exception( "Error loading res://resources/DefaultSettings.tres!" );

				ArgumentNullException.ThrowIfNull( settings );

				SettingsData.SetWindowMode( settings.WindowMode );
				SettingsData.SetResolution( settings.Resolution );
				SettingsData.SetVSync( settings.Vsync );
				SettingsData.SetAntiAliasing( settings.AntiAliasing );
				SettingsData.SetMaxFps( settings.MaxFps );
				SettingsData.SetShadowQuality( settings.ShadowQuality );
				SettingsData.SetShadowFilterQuality( settings.ShadowFilterQuality );
				SettingsData.SetParticleQuality( settings.ParticleQuality );
				SettingsData.SetLightingQuality( settings.LightingQuality );
				SettingsData.SetBloomEnabled( settings.BloomEnabled );
				SettingsData.SetShowBlood( settings.ShowBlood );
				SettingsData.SetPerformanceOverlay( settings.PerformanceStats );

				SettingsData.SetHapticStrength( settings.HapticStrength );
				SettingsData.SetHapticEnabled( settings.HapticFeedback );
				SettingsData.SetColorblindMode( settings.ColorblindMode );
				SettingsData.SetAutoAimMode( settings.AutoAim );
				SettingsData.SetDyslexiaMode( settings.DyslexiaMode );
				SettingsData.SetTutorialsEnabled( settings.EnableTutorials );
				SettingsData.SetHUDPreset( settings.HUDPreset );
				SettingsData.SetTextToSpeech( settings.TextToSpeech );

				SettingsData.SetEffectsOn( settings.SoundEffectsOn );
				SettingsData.SetEffectsVolume( settings.SoundEffectsVolume );
				SettingsData.SetMusicOn( settings.MusicOn );
				SettingsData.SetMusicVolume( settings.MusicVolume );

				SettingsData.SetNetworkingEnabled( settings.NetworkingEnabled );
				SettingsData.SetBountyHuntEnabled( settings.BountyHuntEnabled );
				SettingsData.SetCODLobbyEnabled( settings.CODLobbies );
			} catch ( InvalidCastException ) {
				throw new Exception( "resource file res://resources/DefaultSettings.tres isn't a valid DefaultSettings object!" );
			}
		}

		/*
		===============
		LoadConfigValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		private void LoadConfigValue( string name, out int value ) {
			ArgumentNullException.ThrowIfNull( IniData );
			value = IniData.TryGetValue( name, out string? data ) ? Convert.ToInt32( data ) : 0;
		}

		/*
		===============
		LoadConfigValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		private void LoadConfigValue( string name, out float value ) {
			ArgumentNullException.ThrowIfNull( IniData );
			value = IniData.TryGetValue( name, out string? data ) ? Convert.ToSingle( data ) : 0.0f;
		}

		/*
		===============
		LoadConfigValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		private void LoadConfigValue( string name, out bool value ) {
			ArgumentNullException.ThrowIfNull( IniData );
			value = IniData.TryGetValue( name, out string? data ) ? Convert.ToBoolean( data.ToInt() ) : false;
		}

		/*
		===============
		LoadVideoSettings
		===============
		*/
		private void LoadVideoSettings() {
			LoadConfigValue( "Video:WindowMode", out int windowMode );
			SettingsData.SetWindowMode( (global::WindowMode)windowMode );

			LoadConfigValue( "Video:Resolution", out int resolution );
			SettingsData.SetResolution( (global::WindowResolution)resolution );

			LoadConfigValue( "Video:MaxFps", out int maxFps );
			SettingsData.SetMaxFps( maxFps );

			LoadConfigValue( "Video:PerformanceOverlay", out int performanceOverlay );
			SettingsData.SetPerformanceOverlay( (global::PerformanceOverlayPreset)performanceOverlay );

			LoadConfigValue( "Video:ParticleQuality", out int particleQuality );
			SettingsData.SetParticleQuality( (global::ParticleQuality)particleQuality );

			LoadConfigValue( "Video:ShadowQuality", out int shadowQuality );
			SettingsData.SetShadowQuality( (global::ShadowQuality)shadowQuality );

			LoadConfigValue( "Video:ShadowFilterQuality", out int shadowFilterQuality );
			SettingsData.SetShadowFilterQuality( (global::ShadowFilterQuality)shadowFilterQuality );

			LoadConfigValue( "Video:LightingQuality", out int lightingQuality );
			SettingsData.SetLightingQuality( (global::LightingQuality)lightingQuality );

			LoadConfigValue( "Video:AnimationQuality", out int animationQuality );
			SettingsData.SetAnimationQuality( (global::AnimationQuality)animationQuality );

			LoadConfigValue( "Video:AntiAliasing", out int antiAliasing );
			SettingsData.SetAntiAliasing( (global::AntiAliasing)antiAliasing );

			LoadConfigValue( "Video:VSync", out int vsync );
			SettingsData.SetVSync( (global::VSyncMode)vsync );

			LoadConfigValue( "Video:Bloom", out bool bloom );
			SettingsData.SetBloomEnabled( bloom );

			LoadConfigValue( "Video:ShowBlood", out bool showBlood );
			SettingsData.SetShowBlood( showBlood );
		}

		/*
		===============
		LoadAudioSettings
		===============
		*/
		private void LoadAudioSettings() {
			LoadConfigValue( "Audio:SFXEnabled", out bool sfxEnabled );
			SettingsData.SetEffectsOn( sfxEnabled );

			LoadConfigValue( "Audio:SFXVolume", out float sfxVolume );
			SettingsData.SetEffectsVolume( sfxVolume );

			LoadConfigValue( "Audio:MusicEnabled", out bool musicEnabled );
			SettingsData.SetMusicOn( musicEnabled );

			LoadConfigValue( "Audio:MusicVolume", out float musicVolume );
			SettingsData.SetMusicVolume( musicVolume );
		}

		/*
		================
		LoadAccessibilitySettings
		===============
		*/
		private void LoadAccessibilitySettings() {
			LoadConfigValue( "Accessibility:ColorblindMode", out int colorblindMode );
			SettingsData.SetColorblindMode( (global::ColorblindMode)colorblindMode );

			LoadConfigValue( "Accessibility:HapticStrength", out float hapticStrength );
			SettingsData.SetHapticStrength( hapticStrength );

			LoadConfigValue( "Accessibility:HapticEnabled", out bool hapticEnabled );
			SettingsData.SetHapticEnabled( hapticEnabled );

			LoadConfigValue( "Accessibility:AutoAimMode", out int autoAimMode );
			SettingsData.SetAutoAimMode( (global::AutoAimMode)autoAimMode );

			LoadConfigValue( "Accessibility:DyslexiaMode", out bool dyslexiaMode );
			SettingsData.SetDyslexiaMode( dyslexiaMode );

			LoadConfigValue( "Accessibility:TutorialsEnabled", out bool tutorialsEnabled );
			SettingsData.SetTutorialsEnabled( tutorialsEnabled );

			LoadConfigValue( "Accessibility:TextToSpeech", out bool textToSpeech );
			SettingsData.SetTextToSpeech( textToSpeech );

			LoadConfigValue( "Accessibility:HUDPreset", out int hudPreset );
			SettingsData.SetHUDPreset( (global::HUDPreset)hudPreset );
		}

		/*
		================
		LoadNetworkingSettings
		===============
		*/
		private void LoadNetworkingSettings() {
			LoadConfigValue( "Networking:EnableNetworking", out bool enableNetworking );
			SettingsData.SetNetworkingEnabled( enableNetworking );

			LoadConfigValue( "Networking:CODLobbies", out bool codLobbies );
			SettingsData.SetCODLobbyEnabled( codLobbies );

			LoadConfigValue( "Networking:BountyHuntEnabled", out bool bountyHuntEnabled );
			SettingsData.SetBountyHuntEnabled( bountyHuntEnabled );
		}
	};
};