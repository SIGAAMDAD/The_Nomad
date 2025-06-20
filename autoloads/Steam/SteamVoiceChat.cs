using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Numerics;

public unsafe partial class SteamVoiceChat : CanvasLayer {
	private static AudioStreamGeneratorPlayback Playback;
	private static readonly uint SAMPLE_RATE = 48000;

	private byte[] Packet;
	private byte[] RecordBuffer;
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

		Packet = new byte[ 24 * 1024 ];
		RecordBuffer = new byte[ 24 * 1024 ];

		SteamUser.StartVoiceRecording();
		SteamFriends.SetInGameVoiceSpeaking( SteamManager.GetSteamID(), true );
	}
	public override void _ExitTree() {
		base._ExitTree();

		SteamUser.StopVoiceRecording();
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		CaptureVoice();
	}

	private void CaptureVoice() {
		EVoiceResult result = SteamUser.GetAvailableVoice( out uint compressedSize );
		if ( result == EVoiceResult.k_EVoiceResultOK && compressedSize > 0 ) {
			byte[] packet = new byte[ compressedSize + sizeof( uint ) + 1 ];
			if ( SteamUser.GetVoice( true, RecordBuffer, compressedSize, out uint bytesWritten ) == EVoiceResult.k_EVoiceResultOK ) {
				packet[ 0 ] = (byte)SteamLobby.MessageType.VoiceChat;
				packet[ 1 ] = (byte)( bytesWritten & 0xff );
				packet[ 2 ] = (byte)( ( bytesWritten >> 8 ) & 0xff );
				packet[ 3 ] = (byte)( ( bytesWritten >> 16 ) & 0xff );
				packet[ 4 ] = (byte)( ( bytesWritten >> 24 ) & 0xff );

				Buffer.BlockCopy( RecordBuffer, 0, packet, 5, (int)bytesWritten );

				SteamLobby.Instance.SendP2PPacket( packet, Constants.k_nSteamNetworkingSend_Reliable );
			}
		}
	}

	private byte[] output = new byte[ 44100 ];
	private byte[] DecodeBuffer = new byte[ 64 * 1024 ];
	public void ProcessIncomingVoice( ulong senderId, byte[] data ) {
		uint bytesWritten = BitConverter.ToUInt32( data, 1 );
		Buffer.BlockCopy( data, 5, DecodeBuffer, 0, (int)bytesWritten );

		EVoiceResult result = SteamUser.DecompressVoice( DecodeBuffer, bytesWritten, output, (uint)output.Length, out uint written,
			44100 );
		if ( result == EVoiceResult.k_EVoiceResultOK ) {
			Godot.Vector2[] frames = new Godot.Vector2[ written / 2 ];
			for ( int i = 0; i < frames.Length; i++ ) {
				int rawValue = output[ i * 2 ] | ( output[ i * 2 + 1 ] << 8 );
				rawValue = ( rawValue + 32768 ) & 0xffff;
				float amplitude = ( rawValue - 32768 ) / 32768.0f;
				frames[ i ] = new Godot.Vector2( amplitude, amplitude );
			}
			Playback.PushBuffer( frames );
		} else {
			Console.PrintError( string.Format( "[STEAM] Error decompressing voice audio packet: {0}", result ) );
		}
	}
};