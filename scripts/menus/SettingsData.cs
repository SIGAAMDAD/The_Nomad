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
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

namespace Menus {
	/*
	===================================================================================

	SettingsData

	===================================================================================
	*/

	public partial class SettingsData : Node {
		/// <summary>
		/// The hard-coded path to the configuration file, this should not change
		/// </summary>
		private static readonly string ConfigSaveFile = "user://settings.ini";

		//
		// video options
		//
		public static WindowMode WindowMode { get; private set; }
		public static DRSPreset DRSPreset { get; private set; }
		public static AspectRatio AspectRatio { get; private set; }
		public static WindowResolution Resolution { get; private set; }
		public static ShadowQuality ShadowQuality { get; private set; }
		public static ShadowFilterQuality ShadowFilterQuality { get; private set; }
		public static ParticleQuality ParticleQuality { get; private set; }
		public static LightingQuality LightingQuality { get; private set; }
		public static AnimationQuality AnimationQuality { get; private set; }
		public static VSyncMode VSyncMode { get; private set; }
		public static AntiAliasing AntiAliasing { get; private set; }
		public static int MaxFps { get; private set; }
		public static int DRSTargetFrames { get; private set; }
		public static bool BloomEnabled { get; private set; }
		public static PerformanceOverlayPreset PerformanceOverlay { get; private set; }
		public static bool ShowBlood { get; private set; }

		//
		// accessibility options
		//
		public static float HapticStrength { get; private set; }
		public static bool HapticEnabled { get; private set; }
		public static bool QuicktimeAutocomplete { get; private set; }
		public static ColorblindMode ColorblindMode { get; private set; }
		public static AutoAimMode AutoAimMode { get; private set; }
		public static bool DyslexiaMode { get; private set; }
		public static float UIScale { get; private set; }
		public static bool TextToSpeech { get; private set; }
		public static int TtsVoiceIndex { get; private set; }
		public static bool EnableTutorials { get; private set; }
		public static HUDPreset HUDPreset { get; private set; }

		//
		// audio options
		//
		public static bool EffectsOn { get; private set; }
		public static float EffectsVolume { get; private set; }
		public static bool MusicOn { get; private set; }
		public static float MusicVolume { get; private set; }

		//
		// gameplay options
		//
		public static bool EquipWeaponOnPickup { get; private set; }
		private static bool HellbreakerEnabled;
		private static bool HellbreakerRevanents;

		//
		// network options
		//
		public static bool EnableNetworking;
		public static bool CODLobbies { get; private set; }
		public static bool BountyHuntEnabled { get; private set; }

		private static float EffectsVolumeDb_Flat = 0.0f;
		private static float MusicVolumeDb_Flat = 0.0f;
		public static float ShadowFilterSmooth { get; private set; } = 0.0f;
		public static Light2D.ShadowFilterEnum ShadowFilterType { get; private set; } = Light2D.ShadowFilterEnum.None;

		private static Godot.Collections.Array<Resource> MappingContexts;
		public static RefCounted Remapper { get; private set; }
		public static Resource RemappingConfig { get; private set; }
		public static Godot.Collections.Array<RefCounted> RemappableItems { get; private set; }
		public static RefCounted MappingFormatter { get; private set; }

		public static int LastSaveSlot { get; private set; } = 0;

		public static event Action SettingsChanged;

		public static SettingsData Instance = null;

		/*
		===============
		GetRemapper
		===============
		*/
		/// <summary>
		/// For rebind_button.gd
		/// </summary>
		/// <returns></returns>
		public RefCounted GetRemapper() {
			return Remapper;
		}

		/*
		===============
		GetMappingFormatter
		===============
		*/
		/// <summary>
		/// For rebind_button.gd
		/// </summary>
		/// <returns></returns>
		public RefCounted GetMappingFormatter() {
			return MappingFormatter;
		}

		/*
		===============
		UpdateRemappingCOnfig
		===============
		*/
		public static void UpdateRemappingConfig() {
			RemappingConfig = (Resource)Remapper.Call( "get_mapping_config" );
			Instance.GetNode( "/root/GUIDE" ).Call( "set_remapping_config", RemappingConfig );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetNetworkingEnabled( bool networking ) => EnableNetworking = networking;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetBountyHuntEnabled( bool huntEnabled ) => BountyHuntEnabled = huntEnabled;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetCODLobbyEnabled( bool lobbyEnabled ) => CODLobbies = lobbyEnabled;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetWindowMode( WindowMode mode ) => WindowMode = mode;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetDRSPreset( DRSPreset preset ) => DRSPreset = preset;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetDRSTargetFrames( int frames ) => DRSTargetFrames = frames;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetResolution( WindowResolution resolution ) => Resolution = resolution;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetMaxFps( int maxFps ) {
			MaxFps = maxFps;
			Engine.MaxFps = MaxFps;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetLightingQuality( LightingQuality quality ) => LightingQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetShadowQuality( ShadowQuality quality ) => ShadowQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetShadowFilterQuality( ShadowFilterQuality quality ) => ShadowFilterQuality = quality;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetShadowFilterType( Light2D.ShadowFilterEnum filterType ) => ShadowFilterType = filterType;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetShadowFilterSmooth( float filterSmooth ) => ShadowFilterSmooth = filterSmooth;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetParticleQuality( ParticleQuality quality ) => ParticleQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetAnimationQuality( AnimationQuality quality ) => AnimationQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetVSync( VSyncMode vsync ) => VSyncMode = vsync;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetAntiAliasing( AntiAliasing mode ) => AntiAliasing = mode;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetBloomEnabled( bool bloomEnabled ) => BloomEnabled = bloomEnabled;

		/*
		===============
		SetPerformanceOverlay
		===============
		*/
		public static void SetPerformanceOverlay( PerformanceOverlayPreset overlayPreset ) {
			PerformanceOverlay = overlayPreset;

			if ( overlayPreset == PerformanceOverlayPreset.Hidden ) {
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ProcessModeEnum.Disabled;
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = false;

				Instance.GetNode<CanvasLayer>( "/root/DebugMenu" ).Set( "style", 0 );
			} else if ( overlayPreset == PerformanceOverlayPreset.FpsOnly ) {
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ProcessModeEnum.Always;
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = true;

				Instance.GetNode<CanvasLayer>( "/root/DebugMenu" ).Set( "style", 0 );
			} else if ( overlayPreset == PerformanceOverlayPreset.Partial ) {
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ProcessModeEnum.Disabled;
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = false;

				Instance.GetNode<CanvasLayer>( "/root/DebugMenu" ).Set( "style", 1 );
			} else if ( overlayPreset == PerformanceOverlayPreset.Full ) {
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ProcessModeEnum.Disabled;
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = false;

				Instance.GetNode<CanvasLayer>( "/root/DebugMenu" ).Set( "style", 2 );
			}
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetShowBlood( bool showBlood ) => ShowBlood = showBlood;

		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetEffectsOn( bool effectsOn ) => EffectsOn = effectsOn;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static float GetEffectsVolumeLinear() => EffectsVolumeDb_Flat;
		public static void SetEffectsVolume( float effectsVolume ) {
			EffectsVolume = effectsVolume;
			EffectsVolumeDb_Flat = Mathf.LinearToDb( EffectsVolume * 0.01f );
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetMusicOn( bool musicOn ) => MusicOn = musicOn;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static float GetMusicVolumeLinear() => MusicVolumeDb_Flat;

		/*
		===============
		SetMusicVolume
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetMusicVolume( float musicVolume ) {
			MusicVolume = musicVolume;
			MusicVolumeDb_Flat = Mathf.LinearToDb( MusicVolume * 0.01f );
		}

		/*
		===============
		SetHapticEnabled
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetHapticEnabled( bool hapticEnabled ) {
			HapticEnabled = hapticEnabled;
		}

		/*
		===============
		SetHapticStrength
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetHapticStrength( float hapticStrength ) {
			HapticStrength = hapticStrength;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetAutoAimMode( AutoAimMode mode ) => AutoAimMode = mode;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetColorblindMode( ColorblindMode mode ) => ColorblindMode = mode;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetTutorialsEnabled( bool tutorialsEnabled ) => EnableTutorials = tutorialsEnabled;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetDyslexiaMode( bool dyslexiaMode ) => DyslexiaMode = dyslexiaMode;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetUIScale( float scale ) => UIScale = scale;
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetTextToSpeech( bool textToSpeech ) => TextToSpeech = textToSpeech;

		/*
		===============
		SetTtsVoiceIndex
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetTtsVoiceIndex( int voiceIndex ) {
			TtsVoiceIndex = voiceIndex;
		}

		/*
		===============
		SetHUDPreset
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetHUDPreset( HUDPreset preset ) {
			HUDPreset = preset;
		}

		/*
		===============
		SetEquipWeaponPickup
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetEquipWeaponOnPickup( bool equipWeapon ) {
			EquipWeaponOnPickup = equipWeapon;
		}

		/*
		===============
		SetSaveSlot
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetSaveSlot( int slot ) {
			LastSaveSlot = slot;
		}

		/*
		===============
		Save
		===============
		*/
		/// <summary>
		/// Writes all configuration values to disk
		/// </summary>
		public static void Save() {
			using var configWriter = new Settings.ConfigFileWriter( ConfigSaveFile );
			SettingsChanged.Invoke();
		}

		/*
		===============
		ApplyVideoSettings
		===============
		*/
		public static void ApplyVideoSettings() {
			using var updater = new Settings.VideoSettingsUpdater( Instance );
		}

		/*
		================
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			Instance = this;

			Console.PrintLine( "Loading game configuration..." );

			using RefCounted bindingSetup = (RefCounted)ResourceLoader.Load<GDScript>( "res://scripts/menus/settings_bindings.gd" ).New();

			Remapper = (RefCounted)bindingSetup.Get( "_remapper" );
			RemappingConfig = (Resource)bindingSetup.Get( "_remapping_config" );
			RemappableItems = (Godot.Collections.Array<RefCounted>)bindingSetup.Get( "_remappable_items" );
			MappingFormatter = (RefCounted)bindingSetup.Get( "_mapping_formatter" );

			// load configuration file
			using var configReader = new Settings.ConfigFileReader( ConfigSaveFile );

			// apply the settings
			ApplyVideoSettings();

			Console.PrintLine( "...Finished applying settings" );
		}
	};
};