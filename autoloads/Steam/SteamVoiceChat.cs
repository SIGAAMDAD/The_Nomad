using Godot;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Steamworks;
using System;

public unsafe partial class SteamVoiceChat : CanvasLayer {
	private const int SAMPLE_RATE = 16000;
	private const int FRAME_SIZE = 480;
	private const float COMPRESSION_SCALE = 32767.0f;
	private const float DECOMPRESSION_SCALE = 1.0f / 32768.0f;

	private AudioEffectCapture CaptureEffect;
	private int CaptureBusIndex;
	private float[] CaptureBuffer = new float[ FRAME_SIZE * 2 ];

	private AudioStreamPlayer AudioPlayer;
	private AudioStreamGeneratorPlayback Playback;
	private Queue<Godot.Vector2> AudioQueue = new Queue<Godot.Vector2>();
	private float[] PlaybackBuffer = new float[ FRAME_SIZE * 2 ];

	private static readonly int VECTOR_FLOAT_COUNT = Vector<float>.Count;
	private static readonly Vector<float> ScaleVec = new Vector<float>( COMPRESSION_SCALE );
	private static readonly Vector<float> InvScaleVec = new Vector<float>( DECOMPRESSION_SCALE );
	private static readonly Vector<float> ClampMin = new Vector<float>( -1.0f );
	private static readonly Vector<float> ClampMax = new Vector<float>( 1.0f );

	private float VoiceActivity = 0.0f;
	private const float VoiceThreshold = 0.0000000000000000000000000000000000000000000000005f;
	private const float VoiceDecayRate = 0.01f;

	private Dictionary<CSteamID, HBoxContainer> VoiceActiveIcons = new Dictionary<CSteamID, HBoxContainer>();
	private Dictionary<CSteamID, float> MemberVoiceActivity = new Dictionary<CSteamID, float>();

	public static SteamVoiceChat Instance;

	public float GetVoiceActivity() => VoiceActivity;

	public override void _Ready() {
		base._Ready();

		AudioPlayer = new AudioStreamPlayer();
		AddChild( AudioPlayer );

		var Generator = new AudioStreamGenerator();
		Generator.MixRate = SAMPLE_RATE;
		Generator.BufferLength = 0.1f;
		AudioPlayer.Stream = Generator;
		AudioPlayer.Play();

		Playback = (AudioStreamGeneratorPlayback)AudioPlayer.GetStreamPlayback();

		CaptureBusIndex = AudioServer.BusCount;
		AudioServer.AddBus( CaptureBusIndex );
		AudioServer.SetBusName( CaptureBusIndex, "VoiceCapture" );
		AudioServer.SetBusMute( CaptureBusIndex, false );

		CaptureEffect = new AudioEffectCapture();
		AudioServer.AddBusEffect( CaptureBusIndex, CaptureEffect );
		AudioServer.SetBusEffectEnabled( CaptureBusIndex, 0, true );

		HBoxContainer cloner = GetNode<HBoxContainer>( "ActiveVoicesStack/Cloner" );
		SteamLobby.Instance.ClientJoinedLobby += ( ulong steamId ) => {
			HBoxContainer container = cloner.Duplicate() as HBoxContainer;
			( container.GetChild( 1 ) as Label ).Text = SteamFriends.GetFriendPersonaName( (CSteamID)steamId );
			GetNode<VBoxContainer>( "ActiveVoicesStack" ).AddChild( container );
			VoiceActiveIcons.TryAdd( (CSteamID)steamId, container );
		};
		SteamLobby.Instance.ClientLeftLobby += ( ulong steamId ) => {
			if ( VoiceActiveIcons.TryGetValue( (CSteamID)steamId, out HBoxContainer value ) ) {
				GetNode<VBoxContainer>( "ActiveVoicesStack" ).RemoveChild( value );
				VoiceActiveIcons.Remove( (CSteamID)steamId );
			}
		};

		for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
			HBoxContainer container = cloner.Duplicate() as HBoxContainer;
			( container.GetChild( 1 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[ i ] );
			GetNode<VBoxContainer>( "ActiveVoicesStack" ).AddChild( container );
			VoiceActiveIcons.TryAdd( SteamLobby.Instance.LobbyMembers[ i ], container );
		}

		Instance = this;
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		foreach ( var voiceIcon in VoiceActiveIcons ) {
			voiceIcon.Value.Hide();
		}

		CaptureVoice();
	}

	public bool IsVoiceActive( CSteamID userId ) {
		if ( !VoiceActiveIcons.TryGetValue( userId, out HBoxContainer value ) ) {
			return false;
		}
		return value.Visible;
	}
	public float GetVoiceActivity( CSteamID userId ) {
		if ( !MemberVoiceActivity.TryGetValue( userId, out float value ) ) {
			return 0.0f;
		}
		return value;
	}

	private void CaptureVoice() {
		if ( AudioServer.IsBusMute( CaptureBusIndex ) ) {
			return;
		}

		int available = CaptureEffect.GetFramesAvailable();
		if ( available < FRAME_SIZE ) {
			return;
		}

		Godot.Vector2[] frames = CaptureEffect.GetBuffer( FRAME_SIZE );
		float max = 0.0f;

		for ( int i = 0; i < FRAME_SIZE; i++ ) {
			CaptureBuffer[ i * 2 ] = frames[ i ].X;
			CaptureBuffer[ i * 2 + 1 ] = frames[ i ].Y;
			float amplitude = ( Mathf.Abs( frames[ i ].X ) + Mathf.Abs( frames[ i ].Y ) ) / 2.0f;
			if ( amplitude > max ) {
				max = amplitude;
			}
		}

		VoiceActivity = max > VoiceActivity ? max : Mathf.Max( VoiceActivity - VoiceDecayRate * (float)GetProcessDeltaTime(), 0.0f );

		if ( VoiceActivity > 0.0f ) {
			GD.Print( "Sending" );
			SendVoiceData();
		}
	}

	public void ProcessIncomingVoice( ulong senderId, byte[] data ) {
		if ( data.Length == FRAME_SIZE * 4 ) {
			ProcessAudioPacket( senderId, data );
			VoiceActiveIcons[ (CSteamID)senderId ].Show();
		}
	}
	[MethodImpl( MethodImplOptions.AggressiveOptimization )]
	private void ProcessAudioPacket( ulong senderId, byte[] data ) {
		fixed ( byte *src = data )
		fixed ( float *dst = PlaybackBuffer ) {
			short *pcm = (short *)src;
			Span<int> tmp = stackalloc int[ VECTOR_FLOAT_COUNT ];

			for ( int i = 0; i < data.Length / 2; i += VECTOR_FLOAT_COUNT ) {
				for ( int j = 0; j < VECTOR_FLOAT_COUNT; j++ ) {
					tmp[ j ] = pcm[ i + j ];
				}

				Vector<float> sampleVec = Vector.ConvertToSingle( new Vector<int>( tmp ) );
				sampleVec *= InvScaleVec;
				sampleVec.CopyTo( new System.Span<float>( dst + i, VECTOR_FLOAT_COUNT ) );
			}
		}

		lock ( AudioQueue ) {
			for ( int i = 0; i < FRAME_SIZE; i++ ) {
				AudioQueue.Enqueue( new Godot.Vector2( PlaybackBuffer[ i * 2 ], PlaybackBuffer[ i * 2 + 1 ] ) );
			}
		}

		int available = Playback.GetFramesAvailable();
		if ( available <= 0 ) {
			return;
		}

		Godot.Vector2[] buffer = new Godot.Vector2[ available ];
		int count = Math.Min( available, AudioQueue.Count );
		float max = 0.0f;

		lock ( AudioQueue ) {
			for ( int i = 0; i < count; i++ ) {
				buffer[ i ] = AudioQueue.Dequeue();
				float amplitude = ( Mathf.Abs( buffer[ i ].X ) + Mathf.Abs( buffer[ i ].Y ) ) / 2.0f;
				if ( amplitude > max ) {
					max = amplitude;
				}
			}
		}
		float activity = MemberVoiceActivity[ (CSteamID)senderId ];
		activity = max > activity ? max : Mathf.Max( activity - VoiceDecayRate * (float)GetProcessDeltaTime(), 0.0f );
		MemberVoiceActivity[ (CSteamID)senderId ] = activity;

		for ( int i = count; i < available; i++ ) {
			buffer[ i ] = Godot.Vector2.Zero;
		}
		Playback.PushBuffer( buffer );
	}

	[MethodImpl( MethodImplOptions.AggressiveOptimization )]
	private void SendVoiceData() {
		byte[] pcmData = new byte[ FRAME_SIZE * 4 ];
		fixed ( float *src = CaptureBuffer )
		fixed ( byte *dst = pcmData ) {
			short *pcm = (short *)dst;

			for ( int i = 0; i < CaptureBuffer.Length; i += VECTOR_FLOAT_COUNT ) {
				Vector<float> sampleVec = new Vector<float>( new System.ReadOnlySpan<byte>( src + i, VECTOR_FLOAT_COUNT ) );

				sampleVec = Vector.Min( Vector.Max( sampleVec, ClampMin ), ClampMax );
				sampleVec *= ScaleVec;

				Vector<int> intVec = Vector.ConvertToInt32( sampleVec );
				for ( int j = 0; i < VECTOR_FLOAT_COUNT; j++ ) {
					pcm[ i + j ] = (short)intVec[ j ];
				}
			}
		}

		SteamLobby.Instance.SendP2PPacket( pcmData, Constants.k_nSteamNetworkingSend_UnreliableNoDelay );
	}
};

/*
using Godot;
using Steamworks;
using System;

public partial class SteamVoiceChat : Node {
	private static AudioStreamGeneratorPlayback Playback;
	private static readonly uint SAMPLE_RATE = 48000;

	private byte[] Packet;
	private int CaptureBusIndex = 0;
	private AudioEffectCapture CaptureEffect;

	private float VoiceActivity = 0.0f;
	private const float VoiceThreshold = 0.05f;
	private const float VoiceDecayRate = 0.1f;

	public override void _Ready() {
		base._Ready();

		var AudioPlayer = new AudioStreamPlayer();
		AddChild( AudioPlayer );

		var Generator = new AudioStreamGenerator();
		Generator.MixRate = SAMPLE_RATE;
		Generator.BufferLength = 0.1f; // small buffer for low latency

		AudioPlayer.Stream = Generator;
		AudioPlayer.Play();

		Playback = (AudioStreamGeneratorPlayback)AudioPlayer.GetStreamPlayback();

		Packet = new byte[ 8192 ];

#if USE_STEAM_AUDIO
		SteamUser.StartVoiceRecording();
#else
		CaptureBusIndex = AudioServer.BusCount;
		AudioServer.AddBus( CaptureBusIndex );
		AudioServer.SetBusName( CaptureBusIndex, "Voice" );
		AudioServer.SetBusSend( CaptureBusIndex, "Master" );

		CaptureEffect = new AudioEffectCapture();
		AudioServer.AddBusEffect( CaptureBusIndex, CaptureEffect );
		AudioServer.SetBusEffectEnabled( CaptureBusIndex, 0, true );
#endif
	}
	public override void _EnterTree() {
		base._EnterTree();

#if USE_STEAM_AUDIO
		SteamUser.StopVoiceRecording();
#endif
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		CaptureVoice();
	}

	private void CaptureVoice() {
#if USE_STEAM_AUDIO
		if ( SteamUser.GetAvailableVoice( out uint compressedSize ) != EVoiceResult.k_EVoiceResultOK ) {

		}

		byte[] buffer = new byte[ compressedSize ];
		if ( SteamUser.GetVoice( true, buffer, compressedSize, out uint bytesWritten ) != EVoiceResult.k_EVoiceResultOK ) {
			return;
		}

		Packet[ 0 ] = (byte)SteamLobby.MessageType.VoiceChat;
		Buffer.BlockCopy( buffer, 0, Packet, 1, (int)bytesWritten );

		SteamLobby.Instance.SendP2PPacket( buffer, Constants.k_nSteamNetworkingSend_UnreliableNoDelay );
#else
		if ( AudioServer.IsBusMute( CaptureBusIndex ) ) {
			return;
		}

		int available = CaptureEffect.GetFramesAvailable();
		if ( available <= 0 ) {
			return;
		}

		Godot.Vector2[] frames = CaptureEffect.GetBuffer( available );
		float maxAmplitude = 0.0f;

		for ( int i = 0; i < frames.Length; i++ ) {
			float amplitude = ( Mathf.Abs( frames[ i ].X ) + Mathf.Abs( frames[ i ].Y ) ) / 2.0f;
			if ( amplitude > maxAmplitude ) {
				maxAmplitude = amplitude;
			}
		}

		VoiceActivity = maxAmplitude > VoiceActivity ? maxAmplitude : Mathf.Max( VoiceActivity - VoiceDecayRate * (float)GetProcessDeltaTime(), 0 );

		if ( VoiceActivity > VoiceThreshold ) {
			byte[] data = new byte[ frames.Length * 4 ]; // 4 bytes per frame, 2 channels 16-bit
			for ( int i = 0; i < frames.Length; i++ ) {
				short left = (short)( frames[ i ].X * short.MaxValue );
				short right = (short)( frames[ i ].Y * short.MaxValue );

				BitConverter.GetBytes( left ).CopyTo( data, i * 4 );
				BitConverter.GetBytes( right ).CopyTo( data, i * 4 + 2 );
			}
			SteamLobby.Instance.SendP2PPacket( data, Constants.k_nSteamNetworkingSend_UnreliableNoDelay );
		}
#endif
	}
	public static void ProcessIncomingVoice( ulong senderId, byte[] data ) {
#if USE_STEAM_AUDIO
		byte[] decompressed = new byte[ 8192 ];
		EVoiceResult result = SteamUser.DecompressVoice(
			data,
			(uint)data.Length,
			decompressed,
			(uint)decompressed.Length,
			out uint decompressedLength,
			SAMPLE_RATE
		);

		if ( result == EVoiceResult.k_EVoiceResultOK && decompressedLength > 0 ) {
			PlayAudio( decompressed, decompressedLength );
		}
#else
		int available = Playback.GetFramesAvailable();
		if ( available <= 0 ) {
			return;
		}

		Godot.Vector2[] frames = new Godot.Vector2[ data.Length / 4 ];
		for ( int i = 0; i < frames.Length; i++ ) {
			if ( i > available ) {
				break;
			}
			short left = BitConverter.ToInt16( data, i * 4 );
			short right = BitConverter.ToInt16( data, i * 4 + 2 );

			frames[ i ] = new Godot.Vector2(
				left / (float)short.MaxValue,
				right / (float)short.MaxValue
			);
		}
		Playback.PushBuffer( frames );
#endif
	}

	private static void PlayAudio( byte[] data, uint length ) {
		int sampleCount = (int)length / 2;
		Godot.Vector2[] frames = new Godot.Vector2[ sampleCount ];

		for ( int i = 0; i < sampleCount; i++ ) {
			short rawSample = (short)( data[ i * 2 ] | ( data[ i * 2 + 1 ] ) << 8 );
			float sample = rawSample / 32768.0f;
			frames[ i ] = new Godot.Vector2( sample, sample );
		}

		Playback.PushBuffer( frames );
	}
};
*/