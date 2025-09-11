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

namespace Menus.Settings {
	public struct SettingsConfig {
		//
		// video options
		//
		public WindowMode WindowMode;
		public DRSPreset DRSPreset;
		public AspectRatio AspectRatio;
		public WindowResolution Resolution;
		public ShadowQuality ShadowQuality;
		public ShadowFilterQuality ShadowFilterQuality;
		public ParticleQuality ParticleQuality;
		public LightingQuality LightingQuality;
		public AnimationQuality AnimationQuality;
		public VSyncMode VSyncMode;
		public AntiAliasing AntiAliasing;
		public int MaxFps;
		public int DRSTargetFrames;
		public bool BloomEnabled;
		public PerformanceOverlayPreset PerformanceOverlay;
		public bool ShowBlood;

		//
		// accessibility options
		//
		public float HapticStrength;
		public bool HapticEnabled;
		public bool QuicktimeAutocomplete;
		public ColorblindMode ColorblindMode;
		public AutoAimMode AutoAimMode;
		public bool DyslexiaMode;
		public float UIScale;
		public bool TextToSpeech;

		//
		// audio options
		//
		public bool EffectsOn;
		public float EffectsVolume;
		public bool MusicOn;
		public float MusicVolume;

		//
		// gameplay options
		//
		public bool EquipWeaponOnPickup;
		public bool HellbreakerEnabled;
		public bool HellbreakerRevanents;
		public bool EnableTutorials;
		public HUDPreset HUDPreset;

		//
		// network options
		//
		public bool EnableNetworking;
		public bool BountyHuntEnabled;
		public bool CODLobbies;

		/*
		===============
		SettingsConfig.Constructor
		===============
		*/
		public SettingsConfig( DefaultSettings settings ) {
			WindowMode = settings.WindowMode;
			AspectRatio = settings.AspectRatio;
			Resolution = settings.Resolution;
			ShadowQuality = settings.ShadowQuality;
			ShadowFilterQuality = settings.ShadowFilterQuality;
			LightingQuality = settings.LightingQuality;
			ParticleQuality = settings.ParticleQuality;
			AnimationQuality = settings.AnimationQuality;
			VSyncMode = settings.Vsync;
			AntiAliasing = settings.AntiAliasing;
			MaxFps = settings.MaxFps;
			BloomEnabled = settings.BloomEnabled;
			PerformanceOverlay = settings.PerformanceStats;
			ShowBlood = settings.ShowBlood;

			HapticStrength = settings.HapticStrength;
			HapticEnabled = settings.HapticFeedback;
			QuicktimeAutocomplete = settings.QuicktimeAutocomplete;
			ColorblindMode = settings.ColorblindMode;
			AutoAimMode = settings.AutoAim;
			DyslexiaMode = settings.DyslexiaMode;
			EnableTutorials = settings.EnableTutorials;
			HUDPreset = settings.HUDPreset;
			TextToSpeech = settings.TextToSpeech;
			UIScale = settings.UIScale;

			EffectsOn = settings.SoundEffectsOn;
			EffectsVolume = settings.SoundEffectsVolume;
			MusicOn = settings.MusicOn;
			MusicVolume = settings.MusicVolume;

			EquipWeaponOnPickup = settings.EquipWeaponOnPickup;
			HellbreakerEnabled = settings.Hellbreaker;
			HellbreakerRevanents = settings.HellbreakerRevanents;

			EnableNetworking = settings.NetworkingEnabled;
			CODLobbies = settings.CODLobbies;
			BountyHuntEnabled = settings.BountyHuntEnabled;
		}

		public readonly override bool Equals( object? obj ) => false;
		public readonly override int GetHashCode() => base.GetHashCode();

		// memcmp less painful?
		public static bool operator ==( SettingsConfig a, SettingsConfig b ) {
			if ( a.WindowMode != b.WindowMode ) {
				return false;
			}
			if ( a.VSyncMode != b.VSyncMode ) {
				return false;
			}
			if ( a.AntiAliasing != b.AntiAliasing ) {
				return false;
			}
			if ( a.BloomEnabled != b.BloomEnabled ) {
				return false;
			}
			if ( a.MaxFps != b.MaxFps ) {
				return false;
			}
			if ( a.ShowBlood != b.ShowBlood ) {
				return false;
			}
			if ( a.PerformanceOverlay != b.PerformanceOverlay ) {
				return false;
			}
			if ( a.Resolution != b.Resolution ) {
				return false;
			}
			if ( a.AnimationQuality != b.AnimationQuality ) {
				return false;
			}
			if ( a.ParticleQuality != b.ParticleQuality ) {
				return false;
			}
			if ( a.LightingQuality != b.LightingQuality ) {
				return false;
			}
			if ( a.ShadowQuality != b.ShadowQuality ) {
				return false;
			}
			if ( a.ShadowFilterQuality != b.ShadowFilterQuality ) {
				return false;
			}

			if ( a.EffectsOn != b.EffectsOn ) {
				return false;
			}
			if ( a.EffectsVolume != b.EffectsVolume ) {
				return false;
			}
			if ( a.MusicOn != b.MusicOn ) {
				return false;
			}
			if ( a.MusicVolume != b.MusicVolume ) {
				return false;
			}

			if ( a.HapticEnabled != b.HapticEnabled ) {
				return false;
			}
			if ( a.HapticStrength != b.HapticStrength ) {
				return false;
			}
			if ( a.ColorblindMode != b.ColorblindMode ) {
				return false;
			}
			if ( a.AutoAimMode != b.AutoAimMode ) {
				return false;
			}
			if ( a.DyslexiaMode != b.DyslexiaMode ) {
				return false;
			}
			if ( a.UIScale != b.UIScale ) {
				return false;
			}

			if ( a.EnableTutorials != b.EnableTutorials ) {
				return false;
			}
			if ( a.HUDPreset != b.HUDPreset ) {
				return false;
			}

			if ( a.EnableNetworking != b.EnableNetworking ) {
				return false;
			}
			if ( a.BountyHuntEnabled != b.BountyHuntEnabled ) {
				return false;
			}
			if ( a.CODLobbies != b.CODLobbies ) {
				return false;
			}
			return true;
		}
		public static bool operator !=( SettingsConfig a, SettingsConfig b ) {
			return !( a == b );
		}
	};
};