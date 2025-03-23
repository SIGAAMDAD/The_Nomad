using Godot;

namespace NathanHoad {
	public partial class MusicPlayer : AbstractAudioPlayerPool {
		public MusicPlayer( string[] possibleBusses = null, int poolSize = 8 )
			: base( possibleBusses, poolSize )
		{ }

		private System.Collections.Generic.List<string> TrackHistory = new System.Collections.Generic.List<string>();

		public AudioStreamPlayer Play( AudioStream resource, float position = 0.0f, float volume = 0.0f, float crossfadeDuration = 0.0f, string overrideBus = "" ) {
			Stop( crossfadeDuration * 2.0f );

			AudioStreamPlayer player = GetBusPlayerWithResource( resource );

			// if the player already exists then just make sure the volume is right
			if ( player != null ) {
				FadeVolume( player, player.VolumeDb, volume, crossfadeDuration );
				return player;
			}

			// otherwise we need to prep another player and handle its introduction
			player = Prepare( resource, overrideBus );
			FadeVolume( player, -80.0f, volume, crossfadeDuration );

			// remember this track name
			TrackHistory.Insert( 0, resource.ResourcePath );
			if ( TrackHistory.Count > 50 ) {
				TrackHistory.RemoveAt( 50 );
			}

			player.CallDeferred( "play", position );
			return player;
		}

		public bool IsPlaying( AudioStream resource ) {
			if ( resource != null ) {
				return GetBusPlayerWithResource( resource ) != null;
			}
			return BusyPlayers.Count > 0;
		}
		public void Stop( float fadeOutDuration = 0.0f ) {
			for ( int i = 0; i < BusyPlayers.Count; i++ ) {
				if ( fadeOutDuration <= 0.0f ) {
					fadeOutDuration = 0.01f;
				}
				FadeVolume( BusyPlayers[i], BusyPlayers[i].VolumeDb, -80.0f, fadeOutDuration );
			}
		}
		public void Pause( AudioStream resource = null ) {
			if ( resource != null ) {
				AudioStreamPlayer player = GetBusPlayerWithResource( resource );
				if ( IsInstanceValid( player ) ) {
					player.StreamPaused = true;
				}
			} else {
				for ( int i = 0; i < BusyPlayers.Count; i++ ) {
					BusyPlayers[i].StreamPaused = true;
				}
			}
		}
		public void Resume( AudioStream resource = null ) {
			if ( resource != null ) {
				AudioStreamPlayer player = GetBusPlayerWithResource( resource );
				if ( IsInstanceValid( player ) ) {
					player.StreamPaused = false;
				}
			} else {
				for ( int i = 0; i < BusyPlayers.Count; i++ ) {
					BusyPlayers[i].StreamPaused = false;
				}
			}
		}
		public bool IsTrackPlaying( Rid resource ) {
			for ( int i = 0; i < BusyPlayers.Count; i++ ) {
				if ( BusyPlayers[i].Stream.GetRid() == resource ) {
					return true;
				}
			}
			return false;
		}
		public System.Collections.Generic.List<AudioStream> GetCurrentlyPlaying() {
			System.Collections.Generic.List<AudioStream> tracks = new System.Collections.Generic.List<AudioStream>();
			for ( int i = 0; i < BusyPlayers.Count; i++ ) {
				tracks.Add( BusyPlayers[i].Stream );
			}
			return tracks;
		}
		public System.Collections.Generic.List<string> GetCurrentPlayingTracks() {
			System.Collections.Generic.List<string> tracks = new System.Collections.Generic.List<string>();
			for ( int i = 0; i < BusyPlayers.Count; i++ ) {
				tracks.Add( BusyPlayers[i].Stream.ResourcePath );
			}
			return tracks;
		}
	};
};