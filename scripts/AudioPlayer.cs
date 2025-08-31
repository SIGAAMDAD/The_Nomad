using System;
using System.Collections.Generic;
using Godot;
using Menus;

public partial class AudioPlayer : Node {
	public class Channel {
		public ulong Timestamp;
		public AudioStreamPlayer2D Emitter;
		public Action Finished;
	};

	private static Dictionary<Node2D, Channel> EmitterCache = null;
	private static Channel[] Channels = null;

	private void OnEmitterFinish( AudioStreamPlayer2D stream ) {
		stream.ProcessMode = ProcessModeEnum.Disabled;
	}

	private void OnChannelFinished( Channel channel ) {
		if ( channel.Finished != null ) {
			channel.Finished();
		}
		channel.Emitter.ProcessMode = ProcessModeEnum.Disabled;
	}
	public override void _Ready() {
		base._Ready();

		EmitterCache = new Dictionary<Node2D, Channel>( 256 );

		Channels = new Channel[ 64 ];
		for ( int i = 0; i < Channels.Length; i++ ) {
			Channels[i] = new Channel();
			Channels[i].Emitter = new AudioStreamPlayer2D();
			Channels[i].Emitter.VolumeDb = SettingsData.GetEffectsVolumeLinear();
			Channels[i].Emitter.MaxDistance = 400;
			Channels[i].Emitter.Attenuation = 0.0449f;
			Channels[i].Emitter.ProcessMode = ProcessModeEnum.Disabled;

			Channel channel = Channels[i];
			Channels[i].Emitter.Connect( "finished", Callable.From( () => { OnChannelFinished( channel ); } ) );
			Channels[i].Emitter.SetProcess( false );
			Channels[i].Emitter.SetProcessInternal( false );
			AddChild( Channels[i].Emitter );
		}
	}

	private static Channel AllocateChannel( AudioStream stream ) {
		Channel oldest = null;
		ulong current = Engine.GetProcessFrames();

		for ( int i = 0; i < Channels.Length; i++ ) {
			if ( Channels[i].Emitter.Stream == null ) {
				Channels[i].Emitter.Stream = stream;
				Channels[i].Timestamp = current;
				return Channels[i];
			}

			if ( Channels[i].Emitter.Playing ) {
				if ( oldest == null ) {
					oldest = Channels[i];
				} else if ( current - oldest.Timestamp > Channels[i].Timestamp ) {
					oldest = Channels[i];
				}
			} else {
				// we have a free event, take it
				Channels[i].Emitter.Stream = stream;
				Channels[i].Timestamp = current;
				return Channels[i];
			}
		}

		if ( oldest != null ) {
			oldest.Emitter.Stream = null;
			oldest.Timestamp = current;
		}

		oldest.Emitter.Stream = stream;
		return oldest;
	}
	public static void StopSound( Node2D from ) {
		if ( EmitterCache.TryGetValue( from, out Channel channel ) ) {
			channel.Emitter.Stop();
		}
	}
	private static void OnChannelFinished( Channel channel, Action finished ) {
		finished();
		channel.Emitter.Disconnect( "finished", Callable.From( () => { OnChannelFinished( channel, finished ); } ) );
	}
	public static void PlaySound( Node2D from, AudioStream stream, bool looping = false, float pitchScale = 1.0f, Action finished = null ) {
		if ( EmitterCache.TryGetValue( from, out Channel channel ) ) {
			if ( channel.Emitter.Playing ) {
//				channel.Emitter.Stop();
			}
		} else {
			EmitterCache.Add( from, null );
		}
		
		channel = AllocateChannel( stream );
		channel.Emitter.GlobalPosition = from.GlobalPosition;
		channel.Emitter.ProcessMode = ProcessModeEnum.Pausable;
		channel.Emitter.PitchScale = pitchScale;
		channel.Finished = finished;
		channel.Emitter.Set( "parameters/looping", looping );
		channel.Emitter.Reparent( from );
		channel.Emitter.Play();

		EmitterCache[ from ] = channel;
	}
};