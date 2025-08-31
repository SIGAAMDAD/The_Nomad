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
using System;

namespace Menus.Settings {
	/*
	===================================================================================
	
	ConfigFileWriter
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public readonly struct ConfigFileWriter : IDisposable {
		/*
		===============
		ConfigFileWriter
		===============
		*/
		/// <summary>
		/// Writes configuration values to <paramref name="configPath"/>
		/// </summary>
		/// <param name="configPath">The path to write the configuration values to</param>
		public ConfigFileWriter( string configPath ) {
			ArgumentException.ThrowIfNullOrEmpty( configPath );

			Console.PrintLine( $"Saving configuration data to {configPath}..." );

			string path = ProjectSettings.GlobalizePath( configPath );
			Error result;

			result = DirAccess.RemoveAbsolute( path );
			if ( result != Error.Ok ) {

			}

			{
				using var stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
				using var writer = new System.IO.StreamWriter( stream );

				SaveVideoSettings( writer );
				SaveAudioSettings( writer );
				SaveAccessibilitySettings( writer );
				SaveNetworkingSettings( writer );

				writer.WriteLine( "[Internal]" );
				writer.WriteLine( $"LastSaveSlot={SettingsData.LastSaveSlot}" );

				writer.Flush();
			}

			result = ResourceSaver.Save( SettingsData.RemappingConfig, "user://input_context.tres" );
			if ( result != Error.Ok ) {

			}

			SettingsData.Instance.EmitSignal( SettingsData.SignalName.SettingsChanged );
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
		SaveVideoSettings
		===============
		*/
		/// <summary>
		/// Writes video settings to <paramref name="writer"/> stream
		/// </summary>
		/// <param name="writer">The file stream to write to</param>
		private static void SaveVideoSettings( System.IO.StreamWriter? writer ) {
			ArgumentNullException.ThrowIfNull( writer );

			writer.WriteLine( "[Video]" );
			writer.WriteLine( $"WindowMode={(int)SettingsData.WindowMode}" );
			writer.WriteLine( $"Resolution={(int)SettingsData.Resolution}" );
			writer.WriteLine( $"AspectRatio={(int)SettingsData.AspectRatio}" );
			writer.WriteLine( $"MaxFps={SettingsData.MaxFps}" );
			writer.WriteLine( $"PerformanceOverlay={(int)SettingsData.PerformanceOverlay}" );
			writer.WriteLine( $"ParticleQuality={(int)SettingsData.ParticleQuality}" );
			writer.WriteLine( $"ShadowQuality={(int)SettingsData.ShadowQuality}" );
			writer.WriteLine( $"ShadowFilterQuality={(int)SettingsData.ShadowFilterQuality}" );
			writer.WriteLine( $"LightingQuality={(int)SettingsData.LightingQuality}" );
			writer.WriteLine( $"AnimationQuality={(int)SettingsData.AnimationQuality}" );
			writer.WriteLine( $"AntiAliasing={(int)SettingsData.AntiAliasing}" );
			writer.WriteLine( $"VSync={(int)SettingsData.VSyncMode}" );
			writer.WriteLine( $"Bloom={Convert.ToInt32( SettingsData.BloomEnabled )}" );
			writer.WriteLine( $"ShowBlood={Convert.ToInt32( SettingsData.ShowBlood )}" );
			writer.WriteLine();
		}

		/*
		===============
		SaveAudioSettings
		===============
		*/
		/// <summary>
		/// Writes audio settings to <paramref name="writer"/> stream
		/// </summary>
		/// <param name="writer">The file stream to write to</param>
		private static void SaveAudioSettings( System.IO.StreamWriter? writer ) {
			ArgumentNullException.ThrowIfNull( writer );

			writer.WriteLine( "[Audio]" );
			writer.WriteLine( $"SFXEnabled={Convert.ToInt32( SettingsData.EffectsOn )}" );
			writer.WriteLine( $"SFXVolume={SettingsData.EffectsVolume}" );
			writer.WriteLine( $"MusicEnabled={Convert.ToInt32( SettingsData.MusicOn )}" );
			writer.WriteLine( $"MusicVolume={SettingsData.MusicVolume}" );
			writer.WriteLine();
		}

		/*
		================
		SaveAccessibilitySettings
		===============
		*/
		/// <summary>
		/// Writes accessibility settings to <paramref name="writer"/> stream
		/// </summary>
		/// <param name="writer">The file stream to write to</param>
		private static void SaveAccessibilitySettings( System.IO.StreamWriter? writer ) {
			ArgumentNullException.ThrowIfNull( writer );

			writer.WriteLine( "[Accessibility]" );
			writer.WriteLine( $"ColorblindMode={SettingsData.ColorblindMode}" );
			writer.WriteLine( $"HapticStrength={SettingsData.HapticStrength}" );
			writer.WriteLine( $"HapticEnabled={Convert.ToInt32( SettingsData.HapticEnabled )}" );
			writer.WriteLine( $"AutoAimMode={(uint)SettingsData.AutoAimMode}" );
			writer.WriteLine( $"DyslexiaMode={Convert.ToInt32( SettingsData.DyslexiaMode )}" );
			writer.WriteLine( $"QuicktimeAutocomplete={Convert.ToInt32( SettingsData.QuicktimeAutocomplete )}" );
			writer.WriteLine( $"TutorialsEnabled={Convert.ToInt32( SettingsData.EnableTutorials )}" );
			writer.WriteLine( $"TextToSpeech={Convert.ToInt32( SettingsData.TextToSpeech )}" );
			writer.WriteLine( $"TtsVoiceIndex={SettingsData.TtsVoiceIndex}" );
			writer.WriteLine( $"EnableTutorials={Convert.ToInt32( SettingsData.EnableTutorials )}" );
			writer.WriteLine( $"HUDPreset={(uint)SettingsData.HUDPreset}" );
			writer.WriteLine();
		}

		/*
		================
		SaveNetworkingSettings
		===============
		*/
		/// <summary>
		/// Writes networking settings to <paramref name="writer"/> stream
		/// </summary>
		/// <param name="writer">The file stream to write to</param>
		private static void SaveNetworkingSettings( System.IO.StreamWriter writer ) {
			writer.WriteLine( "[Networking]" );
			writer.WriteLine( $"EnableNetworking={Convert.ToInt32( SettingsData.EnableNetworking )}" );
			writer.WriteLine( $"CODLobbies={Convert.ToInt32( SettingsData.CODLobbies )}" );
			writer.WriteLine( $"BountyHuntEnabled={Convert.ToInt32( SettingsData.BountyHuntEnabled )}" );
			writer.WriteLine();
		}
	};
};