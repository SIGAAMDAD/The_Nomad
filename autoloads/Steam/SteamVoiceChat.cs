/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using Menus;

namespace Steam {
	/*
	===================================================================================

	SteamVoiceChat

	handles proximity voice chat

	===================================================================================
	*/

	public partial class SteamVoiceChat : CanvasLayer {
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

		public static readonly int MaxCompressedSize = 1024;
		public static readonly int MaxPacketSize = 1029;
		public static readonly int VoicePacketHeaderSize = sizeof( SteamLobby.MessageType ) + sizeof( int );
		private static readonly uint SAMPLE_RATE = 44100;
		private static readonly float VoiceThreshold = 0.05f;
		private static readonly float VoiceDecayRate = 0.1f;

		private static AudioStreamGeneratorPlayback? Playback;
		private static bool RecordingVoice = false;

		private byte[] Packet = new byte[ MaxPacketSize ];
		private byte[] RecordBuffer = new byte[ MaxCompressedSize ];
		private int CaptureBusIndex = 0;
		private AudioEffectCapture? CaptureEffect;

		private byte[] Output = new byte[ SAMPLE_RATE ];
		private byte[] DecodeBuffer = new byte[ 1024 ];
		private Godot.Vector2[] Frames = new Godot.Vector2[ 44100 ];

		private Dictionary<ulong, UserVoice> VoiceActivity;

		public static SteamVoiceChat Instance { get; private set; }

		public bool IsVoiceActive( CSteamID steamID ) {
			if ( !VoiceActivity.TryGetValue( (ulong)steamID, out UserVoice voice ) ) {
				Console.PrintError( $"SteamVoiceChat.IsVoiceActive: invalid userID {steamID}" );
				return false;
			}
			return voice.Active;
		}
		public float GetVoiceActivity( CSteamID steamID ) {
			if ( !VoiceActivity.TryGetValue( (ulong)steamID, out UserVoice voice ) ) {
				Console.PrintError( $"SteamVoiceChat.GetVoiceActivity: invalid userID {steamID}" );
				return 0.0f;
			}
			return voice.Volume;
		}
		public void MuteUser( CSteamID steamID ) {
			if ( !VoiceActivity.TryGetValue( (ulong)steamID, out UserVoice voice ) ) {
				Console.PrintError( $"SteamVoiceChat.MuteUser: invalid userID {steamID}" );
				return;
			}
			voice.Muted = true;
		}
		public void UnmuteUser( CSteamID steamID ) {
			if ( !VoiceActivity.TryGetValue( (ulong)steamID, out UserVoice voice ) ) {
				Console.PrintError( $"SteamVoiceChat.UnmuteUser: invalid userID {steamID}" );
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
					SteamLobby.SendVoicePacket( RecordBuffer, bytesWritten );
				}
			}
		}

		public unsafe void ProcessIncomingVoice( ulong senderId, byte[] data ) {
			if ( data == null || data.Length == 0 || data.Length > MaxPacketSize ) {
				Console.PrintError( $"SteamVoiceChat.ProcessIncomingVoice: invalid data packet!" );
				return;
			}
			if ( !VoiceActivity.TryGetValue( senderId, out UserVoice voice ) ) {
				voice = new UserVoice();
				VoiceActivity.Add( senderId, voice );
			}
			if ( voice.Muted ) {
				return; // don't bother with decoding
			}

			EVoiceResult result;
			uint written;
			fixed ( byte* pData = data )
			fixed ( byte* pOutput = DecodeBuffer ) {
				int bytesWritten = *(int*)( pData + 1 );
				Buffer.MemoryCopy( pData + 5, pOutput, DecodeBuffer.Length, bytesWritten );
				result = SteamUser.DecompressVoice(
					DecodeBuffer, (uint)bytesWritten, Output, (uint)Output.Length,
					out written, 44100
				);
			}

			if ( result == EVoiceResult.k_EVoiceResultOK ) {
				int sampleCount = (int)written / 2;
				float highestAmplitude = 0.0f;

				voice.Active = true;

				// we need the data as fast as possible, so pin that shit
				fixed ( byte* outputPtr = Output )
				fixed ( Godot.Vector2* framesPtr = Frames ) {
					for ( int i = 0; i < sampleCount; i++ ) {
						short s = (short)( outputPtr[ i * 2 ] | ( outputPtr[ i * 2 + 1 ] << 8 ) );
						float amp = s * ( 1.0f / 32768.0f );
						framesPtr[ i ].X = amp;
						framesPtr[ i ].Y = amp;

						if ( amp > highestAmplitude ) {
							highestAmplitude = amp;
						}
					}
					if ( highestAmplitude > voice.Volume ) {
						voice.Volume = highestAmplitude;
					}
				}

				Playback.CallDeferred( AudioStreamGeneratorPlayback.MethodName.PushBuffer, Frames );
			} else {
				Console.PrintError( $"[STEAM] Error decompressing voice audio packet: {result}" );
			}
		}
	};
};