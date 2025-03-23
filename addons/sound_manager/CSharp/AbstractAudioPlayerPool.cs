using Godot;
using System.Collections.Generic;
using System.Data.Common;

namespace NathanHoad {
	public partial class AbstractAudioPlayerPool : Node {
		[Export]
		public StringName[] DefaultBusses;
		[Export]
		public int DefaultPoolSize = 8;

		protected List<AudioStreamPlayer> AvailablePlayers = new List<AudioStreamPlayer>();
		protected List<AudioStreamPlayer> BusyPlayers = new List<AudioStreamPlayer>();
		public StringName Bus = "Master";

		protected Dictionary<AudioStreamPlayer, Tween> Tweens = new Dictionary<AudioStreamPlayer, Tween>();

		public AbstractAudioPlayerPool( string[] possibleBusses = null, int poolSize = 8 ) {
			Bus = GetPossbileBus( possibleBusses );
			for ( int i = 0; i < poolSize; i++ ) {
				IncreasePool();
			}
		}

		public StringName GetPossbileBus( string[] possibileBusses ) {
			for ( int i = 0; i < possibileBusses.Length; i++ ) {
				StringName[] cases = {
					possibileBusses[i],
					possibileBusses[i].ToLower(),
					possibileBusses[i].ToCamelCase(),
					possibileBusses[i].ToPascalCase(),
					possibileBusses[i].ToSnakeCase()
				};
				for ( int c = 0; c < cases.Length; c++ ) {
					if ( AudioServer.GetBusIndex( cases[c] ) > -1 ) {
						return cases[c];
					}
				}
			}
			return "Master";
		}

		public AudioStreamPlayer Prepare( AudioStream resource, string overrideBus = "" ) {
			AudioStreamPlayer player = null;

			if ( resource is AudioStreamRandomizer ) {
				player = GetPlayerWithResource( resource );
			}
			if ( player == null ) {
				player = GetAvailablePlayer();
			}

			player.Stream = resource;
			player.Bus = overrideBus != "" ? overrideBus : Bus;
			player.VolumeDb = Mathf.LinearToDb( 1.0f );
			player.PitchScale = 1.0f;

			return player;
		}
		public AudioStreamPlayer GetAvailablePlayer() {
			if ( AvailablePlayers.Count == 0 ) {
				IncreasePool();
			}
			AudioStreamPlayer player = AvailablePlayers[0];
			AvailablePlayers.RemoveAt( 0 );
			BusyPlayers.Add( player );
			return player;
		}
		public AudioStreamPlayer GetPlayerWithResource( AudioStream resource ) {
			for ( int i = 0; i < BusyPlayers.Count; i++ ) {
				if ( BusyPlayers[i].Stream == resource ) {
					return BusyPlayers[i];
				}
			}
			for ( int i = 0; i < AvailablePlayers.Count; i++ ) {
				if ( AvailablePlayers[i].Stream == resource ) {
					return AvailablePlayers[i];
				}
			}
			return null;
		}
		public AudioStreamPlayer GetBusPlayerWithResource( AudioStream resource ) {
			for ( int i = 0; i < BusyPlayers.Count; i++ ) {
				if ( BusyPlayers[i].Stream.GetRid() == resource.GetRid() ) {
					return  BusyPlayers[i];
				}
			}
			return null;
		}
		private void MarkPlayerAsAvailable( AudioStreamPlayer player ) {
			if ( BusyPlayers.Contains( player ) ) {
				BusyPlayers.Remove( player );
			}
			if ( AvailablePlayers.Count >= DefaultPoolSize ) {
				player.QueueFree();
			} else if ( !AvailablePlayers.Contains( player ) ) {
				AvailablePlayers.Add( player );
			}
		}
		public void IncreasePool() {
			// see if we can reclaim a rogue busy player
			for ( int i = 0; i < BusyPlayers.Count; i++ ) {
				if ( !BusyPlayers[i].Playing ) {
					MarkPlayerAsAvailable( BusyPlayers[i] );
					return;
				}
			}

			// otherwise add a new player
			AudioStreamPlayer player = new AudioStreamPlayer();
			AddChild( player );
			AvailablePlayers.Add( player );
			player.Bus = Bus;
			player.Connect( "finished", Callable.From( () => { OnPlayerFinished( player ); } ) );
		}
		public AudioStreamPlayer FadeVolume( AudioStreamPlayer player, float fromVolume, float toVolume, float duration ) {
			// remove any tweens that might already be on this player
			RemoveTween( player );

			// start a new tween
			Tween tween = GetTree().Root.CreateTween();

			player.VolumeDb = fromVolume;
			if ( fromVolume > toVolume ) {
				// fade out
				tween.TweenProperty( player, "volume_db", toVolume, duration ).SetTrans( Tween.TransitionType.Circ ).SetEase( Tween.EaseType.In );
			} else {
				// fade in
				tween.TweenProperty( player, "volume_db", toVolume, duration ).SetTrans( Tween.TransitionType.Circ ).SetEase( Tween.EaseType.Out );
			}

			Tweens.Add( player, tween );
			tween.Connect( "finished", Callable.From( () => { OnFadeCompleted( player, tween, fromVolume, toVolume, duration ); } ) );

			return player;
		}

#region Helpers
		private void RemoveTween( AudioStreamPlayer player ) {
			if ( Tweens.ContainsKey( player ) ) {
				Tween fade = Tweens[ player ];
				fade.Kill();
				Tweens.Remove( player );
			}
		}
#endregion

#region Signals
		private void OnPlayerFinished( AudioStreamPlayer player ) {
			MarkPlayerAsAvailable( player );
		}
		private void OnFadeCompleted( AudioStreamPlayer player, Tween tween, float fromVolume, float toVolume, float duration ) {
			RemoveTween( player );

			// if we just faded out then our player is now available
			if ( toVolume <= -79.0f ) {
				player.Stop();
				MarkPlayerAsAvailable( player );
			}
		}
#endregion
	};
};