/*
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

		if ( max > 0.0f ) {
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
*/

using Godot;
using Steamworks;
using System;
using System.Numerics;

public unsafe partial class SteamVoiceChat : CanvasLayer {
	private static AudioStreamGeneratorPlayback Playback;
	private static readonly uint SAMPLE_RATE = 48000;

	private byte[] Packet;
	private int CaptureBusIndex = 0;
	private AudioEffectCapture CaptureEffect;

	private float VoiceActivity = 0.0f;
	private const float VoiceThreshold = 0.05f;
	private const float VoiceDecayRate = 0.1f;

	public static SteamVoiceChat Instance;

	public bool IsVoiceActive( CSteamID steamID ) => false;
	public float GetVoiceActivity( CSteamID steamID ) => 0.0f;

	public override void _Ready() {
		base._Ready();

		Instance = this;

		AudioStreamPlayer AudioPlayer = GetNode<AudioStreamPlayer>( "Output" );
		AudioPlayer.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		Playback = (AudioStreamGeneratorPlayback)AudioPlayer.GetStreamPlayback();

		Packet = new byte[ 8192 ];

#if USE_STEAM_AUDIO
		SteamUser.StartVoiceRecording();
#else
		AudioStreamPlayer Input = GetNode<AudioStreamPlayer>( "Input" );
		Input.Stream = new AudioStreamMicrophone();
		Input.Play();

		CaptureBusIndex = AudioServer.GetBusIndex( "Microphone" );
		CaptureEffect = (AudioEffectCapture)AudioServer.GetBusEffect( CaptureBusIndex, 0 );
#endif
	}
	public override void _ExitTree() {
		base._ExitTree();

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
		EVoiceResult eResult;

		eResult = SteamUser.GetAvailableVoice( out uint compressedSize );
		if ( eResult != EVoiceResult.k_EVoiceResultOK ) {
//			Console.PrintError( string.Format( "[STEAM] Error recording voice: {0}", eResult ) );
			return;
		}

		byte[] buffer = new byte[ compressedSize ];

		eResult = SteamUser.GetVoice( true, buffer, compressedSize, out uint bytesWritten );
		if ( eResult != EVoiceResult.k_EVoiceResultOK ) {
			Console.PrintError( string.Format( "[STEAM] Error getting voice data: {0}", eResult ) );
			return;
		}

		Packet[ 0 ] = (byte)SteamLobby.MessageType.VoiceChat;
		Buffer.BlockCopy( buffer, 0, Packet, 1, (int)bytesWritten );

		GD.Print( "SENDING VOICE" );
		SteamLobby.Instance.SendP2PPacket( buffer, Constants.k_nSteamNetworkingSend_UnreliableNoDelay );
#else
		if ( CaptureEffect.CanGetBuffer( 512 ) ) {
			using var stream = new System.IO.MemoryStream( Packet );
			using var writer = new System.IO.BinaryWriter( stream );

			writer.Write( (byte)SteamLobby.MessageType.VoiceChat );
			Godot.Vector2[] frames = CaptureEffect.GetBuffer( 512 );
			writer.Write( frames.Length );
			for ( int i = 0; i < frames.Length; i++ ) {
				writer.Write( frames[ i ].X );
				writer.Write( frames[ i ].Y );
			}
			SteamLobby.Instance.SendP2PPacket( Packet, Constants.k_nSteamNetworkingSend_UnreliableNoDelay );
		}
#endif
	}
	public void ProcessIncomingVoice( ulong senderId, byte[] data ) {
#if USE_STEAM_AUDIO
		byte[] buffer = new byte[ data.Length - 1 ];
		Buffer.BlockCopy( data, 1, buffer, 0, buffer.Length );
		byte[] decompressed = new byte[ 8192 ];
		EVoiceResult result = SteamUser.DecompressVoice(
			buffer,
			(uint)buffer.Length,
			decompressed,
			(uint)decompressed.Length,
			out uint decompressedLength,
			SAMPLE_RATE
		);

		GD.Print( "GOT AUDIO" );

		if ( result == EVoiceResult.k_EVoiceResultOK && decompressedLength > 0 ) {
			PlayAudio( decompressed, decompressedLength );
		}
#else
		PlayAudio( data, data.Length );
#endif
	}

	private static void PlayAudio( byte[] data, int length ) {

		/*
				int sampleCount = length / 2;
				Godot.Vector2[] frames = new Godot.Vector2[ sampleCount ];

				for ( int i = 0; i < sampleCount; i++ ) {
					short rawSample = (short)( buffer[ i * 2 ] | ( buffer[ i * 2 + 1 ] ) << 8 );
					float sample = rawSample / 32768.0f;
					frames[ i ] = new Godot.Vector2( sample, sample );
				}
		*/
		using var stream = new System.IO.MemoryStream( data );
		using var reader = new System.IO.BinaryReader( stream );

		float[] buffer = new float[ data.Length - 1 / 4 ];
		Buffer.BlockCopy( data, 1, buffer, 0, data.Length - 1 );

		Godot.Vector2[] frames = new Godot.Vector2[ buffer.Length / 2 ];
		for ( int i = 0; i < frames.Length; i++ ) {
			frames[ i ] = new Godot.Vector2( buffer[ i * 2 ], data[ i * 2 + 1 ] );
		}

		Playback.PushBuffer( frames );
	}
};