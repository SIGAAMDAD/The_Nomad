using Godot;
using Steamworks;
using System;
using System.Collections.Generic;

public unsafe partial class SteamVoiceChat : CanvasLayer {
	private class UserVoice {
		public float Volume = 0.0f;
		public bool Active = false;
		public bool Muted {
			get => Muted;
			set {
				if ( value ) {
					Active = false;
					Volume = 0.0f;
				}
			}
		}

		public UserVoice() {
			Muted = false;
		}
	};

	private static AudioStreamGeneratorPlayback Playback;
	private static bool RecordingVoice = false;

	private static readonly uint SAMPLE_RATE = 44100;
	private static readonly float VoiceThreshold = 0.05f;
	private static readonly float VoiceDecayRate = 0.1f;

	private byte[] Packet = new byte[ 1056 ];
	private byte[] RecordBuffer = new byte[ 1024 ];
	private int CaptureBusIndex = 0;
	private AudioEffectCapture CaptureEffect;

	private byte[] Output = new byte[ 44100 ];
	private byte[] DecodeBuffer = new byte[ 1024 ];
	private Godot.Vector2[] Frames = new Godot.Vector2[ 44100 ];

	private Dictionary<ulong, UserVoice> VoiceActivity;

	public static SteamVoiceChat Instance;

	public bool IsVoiceActive( CSteamID steamID ) {
		if ( !VoiceActivity.TryGetValue( (ulong)steamID, out UserVoice voice ) ) {
			return false;
		}
		return voice.Active;
	}
	public float GetVoiceActivity( CSteamID steamID ) {
		if ( !VoiceActivity.TryGetValue( (ulong)steamID, out UserVoice voice ) ) {
			return 0.0f;
		}
		return voice.Volume;
	}
	public void MuteUser( CSteamID steamID ) {
		if ( !VoiceActivity.TryGetValue( (ulong)steamID, out UserVoice voice ) ) {
			Console.PrintError( string.Format( "SteamVoiceChat.MuteUser: invalid userID {0}", steamID ) );
			return;
		}
		voice.Muted = true;
	}
	public void UnmuteUser( CSteamID steamID ) {
		if ( !VoiceActivity.TryGetValue( (ulong)steamID, out UserVoice voice ) ) {
			Console.PrintError( string.Format( "SteamVoiceChat.UnmuteUser: invalid userID {0}", steamID ) );
			return;
		}
		voice.Muted = false;
	}

	public override void _Ready() {
		base._Ready();

		Instance = this;

		AudioStreamPlayer AudioPlayer = GetNode<AudioStreamPlayer>( "Output" );
		AudioPlayer.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		Playback = (AudioStreamGeneratorPlayback)AudioPlayer.GetStreamPlayback();

		VoiceActivity = new Dictionary<ulong, UserVoice>();

		SteamLobby.Instance.LobbyJoined += ( lobbyId ) => {
			SteamUser.StartVoiceRecording();
			RecordingVoice = true;
			ProcessMode = ProcessModeEnum.Always;
		};
		SteamLobby.Instance.ClientLeftLobby += ( steamId ) => {
			if ( steamId == (ulong)SteamManager.GetSteamID() ) {
				VoiceActivity.Clear();

				SteamUser.StopVoiceRecording();
				RecordingVoice = false;
				ProcessMode = ProcessModeEnum.Disabled;
			}
		};
	}
	public override void _ExitTree() {
		base._ExitTree();

		if ( RecordingVoice ) {
			SteamUser.StopVoiceRecording();
		}
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		foreach ( var user in VoiceActivity ) {
			if ( user.Value.Muted ) {
				continue;
			}
			if ( user.Value.Volume > 0.0f ) {
				user.Value.Volume -= VoiceDecayRate * (float)delta;
				if ( user.Value.Volume < 0.0f ) {
					user.Value.Volume = 0.0f;
				}
			}
			user.Value.Active = false;
		}
		
		CaptureVoice();
	}

	private void CaptureVoice() {
		EVoiceResult result = SteamUser.GetAvailableVoice( out uint compressedSize );
		if ( result == EVoiceResult.k_EVoiceResultOK && compressedSize > 0 ) {
			if ( SteamUser.GetVoice( true, RecordBuffer, compressedSize, out uint bytesWritten ) == EVoiceResult.k_EVoiceResultOK ) {
				Packet[ 0 ] = (byte)SteamLobby.MessageType.VoiceChat;
				Packet[ 1 ] = (byte)( bytesWritten & 0xff );
				Packet[ 2 ] = (byte)( ( bytesWritten >> 8 ) & 0xff );
				Packet[ 3 ] = (byte)( ( bytesWritten >> 16 ) & 0xff );
				Packet[ 4 ] = (byte)( ( bytesWritten >> 24 ) & 0xff );

				Buffer.BlockCopy( RecordBuffer, 0, Packet, 5, (int)bytesWritten );

				SteamLobby.Instance.SendP2PPacket( Packet, Constants.k_nSteamNetworkingSend_UnreliableNoDelay );
			}
		}
	}

	public unsafe void ProcessIncomingVoice( ulong senderId, byte[] data ) {
		if ( !VoiceActivity.TryGetValue( senderId, out UserVoice voice ) ) {
			voice = new UserVoice();
			VoiceActivity.Add( senderId, voice );
		}
		if ( voice.Muted ) {
			return; // don't bother with decoding
		}

		uint bytesWritten = BitConverter.ToUInt32( data, 1 );
		Buffer.BlockCopy( data, 5, DecodeBuffer, 0, (int)bytesWritten );

		EVoiceResult result = SteamUser.DecompressVoice(
			DecodeBuffer, bytesWritten, Output, (uint)Output.Length,
			out uint written, 44100
		);

		if ( result == EVoiceResult.k_EVoiceResultOK ) {
			int sampleCount = (int)written / 2;
			float highestAmplitude = 0.0f;

			voice.Active = true;

			fixed ( byte* outputPtr = Output )
			fixed ( Godot.Vector2* framesPtr = Frames ) {
				/*
				if ( Avx2.IsSupported && sampleCount >= 16 ) {
					Vector256<float> scaleVec = Vector256.Create( 1.0f / 32768.0f );
					Vector256<int> signMask = Vector256.Create( unchecked((int)0xFFFF0000) );

					// Process 16 samples per iteration
					int i = 0;
					for ( ; i <= sampleCount - 16; i += 16 ) {
						// Load 32 bytes (16 shorts)
						Vector256<short> rawShorts = Avx.LoadVector256( (short*)( outputPtr + i * 2 ) );

						// Convert to two Vector256<int> (low and high)
						Vector256<int> intsLow = Avx2.ConvertToVector256Int32( rawShorts.GetLower() );
						Vector256<int> intsHigh = Avx2.ConvertToVector256Int32( rawShorts.GetUpper() );

						// Convert to float
						Vector256<float> floatsLow = Avx.ConvertToVector256Single( intsLow );
						Vector256<float> floatsHigh = Avx.ConvertToVector256Single( intsHigh );

						// Apply scaling
						floatsLow = Avx.Multiply( floatsLow, scaleVec );
						floatsHigh = Avx.Multiply( floatsHigh, scaleVec );

						// Duplicate samples for stereo
						Vector256<float> stereoLow1 = Avx.Permute2x128( floatsLow, floatsLow, 0x00 );
						Vector256<float> stereoLow2 = Avx.Permute2x128( floatsLow, floatsLow, 0x11 );
						Vector256<float> stereoHigh1 = Avx.Permute2x128( floatsHigh, floatsHigh, 0x00 );
						Vector256<float> stereoHigh2 = Avx.Permute2x128( floatsHigh, floatsHigh, 0x11 );

						// Interleave for stereo channels
						Vector256<float> interleaved1 = Avx.UnpackLow( stereoLow1, stereoLow1 );
						Vector256<float> interleaved2 = Avx.UnpackHigh( stereoLow1, stereoLow1 );
						Vector256<float> interleaved3 = Avx.UnpackLow( stereoLow2, stereoLow2 );
						Vector256<float> interleaved4 = Avx.UnpackHigh( stereoLow2, stereoLow2 );
						Vector256<float> interleaved5 = Avx.UnpackLow( stereoHigh1, stereoHigh1 );
						Vector256<float> interleaved6 = Avx.UnpackHigh( stereoHigh1, stereoHigh1 );
						Vector256<float> interleaved7 = Avx.UnpackLow( stereoHigh2, stereoHigh2 );
						Vector256<float> interleaved8 = Avx.UnpackHigh( stereoHigh2, stereoHigh2 );

						// Store results
						Avx.Store( (float*)( framesPtr + i ), interleaved1 );
						Avx.Store( (float*)( framesPtr + i + 4 ), interleaved2 );
						Avx.Store( (float*)( framesPtr + i + 8 ), interleaved3 );
						Avx.Store( (float*)( framesPtr + i + 12 ), interleaved4 );
						Avx.Store( (float*)( framesPtr + i + 16 ), interleaved5 );
						Avx.Store( (float*)( framesPtr + i + 20 ), interleaved6 );
						Avx.Store( (float*)( framesPtr + i + 24 ), interleaved7 );
						Avx.Store( (float*)( framesPtr + i + 28 ), interleaved8 );
					}

					// Process remaining samples with scalar fallback
					for ( ; i < sampleCount; i++ ) {
						short s = (short)( output[ i * 2 ] | ( output[ i * 2 + 1 ] << 8 ) );
						float amp = s * ( 1.0f / 32768.0f );
						framesPtr[ i ] = new Godot.Vector2( amp, amp );
					}
				} else if ( Sse3.IsSupported && sampleCount >= 8 ) {
					// SSE3 optimization for 8 samples per iteration
					Vector128<float> scaleVec = Vector128.Create( 1.0f / 32768.0f );
					int i = 0;

					for ( ; i <= sampleCount - 8; i += 8 ) {
						// Load 16 bytes (8 samples) directly from memory
						Vector128<short> samples = Sse2.LoadVector128( (short*)( outputPtr + i * 2 ) );

						// Convert to 32-bit integers in two batches
						Vector128<int> ints1 = Sse41.ConvertToVector128Int32( samples );
						Vector128<int> ints2 = Sse41.ConvertToVector128Int32( Sse2.ShiftRightLogical128BitLane( samples, 8 ) );

						// Convert to floats
						Vector128<float> floats1 = Sse2.ConvertToVector128Single( ints1 );
						Vector128<float> floats2 = Sse2.ConvertToVector128Single( ints2 );

						// Apply scaling
						floats1 = Sse.Multiply( floats1, scaleVec );
						floats2 = Sse.Multiply( floats2, scaleVec );

						// Create stereo pairs using efficient shuffling
						Vector128<float> stereo1 = Sse.Shuffle( floats1, floats1, 0x00 ); // [a, a, b, b]
						Vector128<float> stereo2 = Sse.Shuffle( floats1, floats1, 0x55 ); // [c, c, d, d]
						Vector128<float> stereo3 = Sse.Shuffle( floats2, floats2, 0x00 ); // [e, e, f, f]
						Vector128<float> stereo4 = Sse.Shuffle( floats2, floats2, 0x55 ); // [g, g, h, h]

						// Store results as interleaved stereo frames
						Sse.StoreLow( (float*)( framesPtr + i ), stereo1 );
						Sse.StoreHigh( (float*)( framesPtr + i + 2 ), stereo1 );
						Sse.StoreLow( (float*)( framesPtr + i + 4 ), stereo2 );
						Sse.StoreHigh( (float*)( framesPtr + i + 6 ), stereo2 );
						Sse.StoreLow( (float*)( framesPtr + i + 8 ), stereo3 );
						Sse.StoreHigh( (float*)( framesPtr + i + 10 ), stereo3 );
						Sse.StoreLow( (float*)( framesPtr + i + 12 ), stereo4 );
						Sse.StoreHigh( (float*)( framesPtr + i + 14 ), stereo4 );
					}

					// Process remaining samples
					for ( ; i < sampleCount; i++ ) {
						short s = (short)( output[ i * 2 ] | ( output[ i * 2 + 1 ] << 8 ) );
						float amp = s * ( 1.0f / 32768.0f );
						frames[ i ] = new Godot.Vector2( amp, amp );
					}
				} else
					*/
				{
					// scalar fallback when no SIMD support
					for ( int i = 0; i < sampleCount; i++ ) {
						short s = (short)( Output[ i * 2 ] | ( Output[ i * 2 + 1 ] << 8 ) );
						float amp = s * ( 1.0f / 32768.0f );
						Frames[ i ].X = amp;
						Frames[ i ].Y = amp;

						if ( amp > highestAmplitude ) {
							highestAmplitude = amp;
						}
					}
					if ( highestAmplitude > voice.Volume ) {
						voice.Volume = highestAmplitude;
					}
				}
			}

			Playback.CallDeferred( "PushBuffer", Frames );
		} else {
			Console.PrintError( $"[STEAM] Error decompressing voice audio packet: {result}" );
		}
	}
};