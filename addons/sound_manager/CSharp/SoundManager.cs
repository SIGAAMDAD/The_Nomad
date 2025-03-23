/*
using Godot;
using Godot.Collections;

namespace NathanHoad {
	public partial class SoundManager : Node {
		private static Node instance;
		public static Node Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (Node)Engine.GetSingleton("SoundManager");
				}
				return instance;
			}
		}

		private static SoundEffectsPlayer SoundEffects = new SoundEffectsPlayer( new string[]{ "Sounds", "SFX" }, 8 );
		private static SoundEffectsPlayer UISoundEffects = new SoundEffectsPlayer( new string[]{ "UI", "Interface", "Sounds", "SFX" }, 8 ); 
		private static MusicPlayer Music = new MusicPlayer( new string[]{ "Music" }, 2 ); 

		public static ProcessModeEnum SoundProcessMode
		{
			set { SoundEffects.ProcessMode = value; }
			get { return SoundEffects.ProcessMode; }
		}
		public static ProcessModeEnum UISoundProcessMode
		{
			set { UISoundEffects.ProcessMode = value; }
			get { return UISoundEffects.ProcessMode; }
		}
		public static ProcessModeEnum MusicProcessMode
		{
			set { Music.ProcessMode = value; }
			get { return Music.ProcessMode; }
		}

		public SoundManager() {
			Engine.RegisterSingleton( "SoundManager", this );

			AddChild( SoundEffects );
			AddChild( UISoundEffects );
			AddChild( Music );

			SoundProcessMode = ProcessModeEnum.Pausable;
			UISoundProcessMode = ProcessModeEnum.Always;
			MusicProcessMode = ProcessModeEnum.Always;
		}

		private static void ShowSharedBusWarning() {
			if ( Music.Bus == "Master" || SoundEffects.Bus == "Master" || UISoundEffects.Bus == "Master" ) {
				GD.PushWarning( "Using the Master sound bus directly isn't recommended." );
			}
			if ( Music.Bus == SoundEffects.Bus || Music.Bus == UISoundEffects.Bus ) {
				GD.PushWarning( "Both music and sounds are using the same bus: " + Music.Bus );
			}
		}

		public static float GetSoundVolume() {
			return Mathf.DbToLinear( AudioServer.GetBusVolumeDb( AudioServer.GetBusIndex( SoundEffects.Bus ) ) );
		}
		public static void SetSoundVolume( float volumeBetween0And1 ) {
			ShowSharedBusWarning();
			AudioServer.SetBusVolumeDb( AudioServer.GetBusIndex( SoundEffects.Bus ), Mathf.LinearToDb( volumeBetween0And1 ) );
		}
		public static float GetUISoundVolume() {
			return Mathf.DbToLinear( AudioServer.GetBusVolumeDb( AudioServer.GetBusIndex( UISoundEffects.Bus ) ) );
		}
		public static void StopMusic( float fadeOutDuration = 0.0f ) {
			Music.Stop( fadeOutDuration );
		}
		public static AudioStreamPlayer PlayMusic( AudioStream resource, float volume = 0.0f, float crossfadeDuration = 0.0f, string overrideBus = "" ) {
			return Music.Play( resource, 0.0f, volume, crossfadeDuration, overrideBus );
		}
		public static bool IsMusicPlaying( AudioStream resource = null ) {
			return Music.IsPlaying( resource );
		}
	};
};
*/