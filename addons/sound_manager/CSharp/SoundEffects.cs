using Godot;

namespace NathanHoad {
	public partial class SoundEffectsPlayer : AbstractAudioPlayerPool {
		public SoundEffectsPlayer( string[] possibleBusses = null, int poolSize = 8 )
			: base( possibleBusses, poolSize )
		{ }

		public AudioStreamPlayer Play( AudioStream resource, string overrideBus = "" ) {
			AudioStreamPlayer player = Prepare( resource, overrideBus );
			player.CallDeferred( "play" );
			return player;
		}
		public void Stop( AudioStream resource ) {
			for ( int i = 0; i < BusyPlayers.Count; i++ ) {
				if ( BusyPlayers[i].Stream == resource ) {
					BusyPlayers[i].CallDeferred( "stop" );
				}
			}
		}
	};
};