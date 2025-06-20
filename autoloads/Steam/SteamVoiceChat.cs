using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Numerics;

public unsafe partial class SteamVoiceChat : CanvasLayer {
	private static AudioStreamGeneratorPlayback Playback;
	private static readonly uint SAMPLE_RATE = 48000;

	private byte[] Packet = new byte[ 1024 ];
	private byte[] RecordBuffer = new byte[ 768 ];
	private int CaptureBusIndex = 0;
	private AudioEffectCapture CaptureEffect;

	private byte[] output = new byte[ 44100 ];
	private byte[] DecodeBuffer = new byte[ 1024 ];

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