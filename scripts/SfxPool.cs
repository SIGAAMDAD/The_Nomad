using Godot;

public partial class SfxPool : Node {
	private class Channel {
		public ulong TimeStamp = 0;
		public AudioStreamPlayer2D StreamPlayer = null;

		public Channel() {
		}
	};
	private class Emitter {
		public Channel Channel;
		public Emitter Next;
		public Emitter Prev;

		public Emitter() {
		}
	};

	// TODO: make this configurable
	private const int MaxChannels = 128;

	private static Channel[] ChannelList;
//	private static Emitter[] EmitterList;
//	private static Emitter ActiveChannels;

	public override void _Ready() {
		ChannelList = new Channel[ MaxChannels ];
//		EmitterList = new Emitter[ MaxChannels ];

		for ( int i = 0; i < MaxChannels; i++ ) {
//			EmitterList[i] = new Emitter();

			ChannelList[i] = new Channel();
			ChannelList[i].StreamPlayer = new AudioStreamPlayer2D();
			AddChild( ChannelList[i].StreamPlayer );
		}

		/*
		ActiveChannels = EmitterList[0];
		ActiveChannels.Next =
		ActiveChannels.Prev =
			ActiveChannels;
			*/

//		ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
	}

	private static Channel AllocateChannel() {
		ulong current = Engine.GetProcessFrames();
		Channel oldest = null;

		for ( int i = 0; i < MaxChannels; i++ ) {
			if ( ChannelList[i].StreamPlayer.Playing ) {
				oldest = ChannelList[i];
				if ( oldest == null ) {
					oldest = ChannelList[i];
				} else if ( current - oldest.TimeStamp > ChannelList[i].TimeStamp ) {
					oldest = ChannelList[i];
				}
			} else {
				// we have a free event, take it
				ChannelList[i].TimeStamp = current;
				ChannelList[i].StreamPlayer.SetProcess( true );
				ChannelList[i].StreamPlayer.SetProcessInternal( true );
				return ChannelList[i];
			}
		}
		if ( oldest != null ) {
//			oldest.StreamPlayer.Stop();
			oldest.TimeStamp = current;
		}
		oldest.StreamPlayer.SetProcess( true );
		oldest.StreamPlayer.SetProcessInternal( true );

		return oldest;
	}

	public static void PlaySfx( AudioStream stream, Godot.Vector2 position, float pitch = 1.0f, float volumeDb = 10.0f ) {
		Channel channel = AllocateChannel();
		channel.StreamPlayer.Stream = stream;
		channel.StreamPlayer.GlobalPosition = position;
		channel.StreamPlayer.PitchScale = pitch;
		channel.StreamPlayer.VolumeDb = volumeDb;
		channel.StreamPlayer.Play();
	}

	public override void _Process( double delta ) {
		base._Process( delta );

		if ( ( Engine.GetProcessFrames() % 30 ) != 0 ) {
			return;
		}
		return;

		for ( int i = 0; i < MaxChannels; i++ ) {
			if ( !ChannelList[i].StreamPlayer.Playing ) {
				ChannelList[i].StreamPlayer.Stream = null;
				ChannelList[i].StreamPlayer.SetProcess( false );
				ChannelList[i].StreamPlayer.SetProcessInternal( false );
			}
		}
	}
};