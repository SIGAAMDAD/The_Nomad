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
using System.Runtime.InteropServices;
using Multiplayer;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using ResourceCache;

namespace Steam {
	/*
	===================================================================================

	SteamLobby

	===================================================================================
	*/

	public partial class SteamLobby : Node {
		public enum Visibility {
			Private,
			Public,
			FriendsOnly
		};

		public enum MessageType : byte {
			Handshake,
			ServerCommand,
			ClientData, // client to server
			ServerSync, // server to client
			GameData,

			ChatMessage_TeamOnly,
			ChatMessage_PlayerOnly,
			ChatMessage_FriendsOnly,

			VoiceChat,

			EncryptionKey,

			Count
		};

		public enum ChatMessageType : int {
			FriendsOnly,
			TeamOnly,
			PlayerOnly,
			None
		};

		// Message processing
		private struct IncomingMessage {
			public CSteamID Sender;
			public byte[] Data;
			public int Length;
			public MessageType Type;
		};

		public readonly struct NetworkNode {
			public readonly Node Node;
			public readonly System.Action Send;
			public readonly Action<System.IO.BinaryReader> Receive;

			public NetworkNode( Node node, System.Action send, System.Action<System.IO.BinaryReader> receive ) {
				Node = node;
				Send = send;
				Receive = receive;
			}
		};

		public readonly struct PlayerNetworkNode {
			public readonly Node Node;
			public readonly System.Action Send;
			public readonly Action<ulong, System.IO.BinaryReader> Receive;

			public PlayerNetworkNode( Node node, System.Action send, Action<ulong, System.IO.BinaryReader> receive ) {
				Node = node;
				Send = send;
				Receive = receive;
			}
		};

		private class BufferPool {
			private readonly ConcurrentBag<byte[]> Pool = new ConcurrentBag<byte[]>();
			private readonly int BufferSize;
			private readonly int MaxBuffers;

			public BufferPool( int nBufferSize = 1024, int nMaxBuffers = 128 ) {
				BufferSize = nBufferSize;
				MaxBuffers = nMaxBuffers;
			}

			public byte[] Rent() {
				if ( Pool.TryTake( out byte[] buffer ) ) {
					return buffer;
				}
				return new byte[ BufferSize ];
			}
			public void Return( byte[] buffer ) {
				if ( buffer != null && buffer.Length == BufferSize && Pool.Count < MaxBuffers ) {
					Array.Clear( buffer, 0, buffer.Length );
					Pool.Add( buffer );
				}
			}
		};

		public class SteamLobbySecurity {
			private const bool USE_STEAM_ENCRYPTION = true;

			private struct SecurityHeader {
				public uint Timestamp;
				public ushort Sequence;
				public byte Version;
			};
			private class ConnectionSecurity {
				public ushort LastInSeq = 0;
				public ushort LastOutSeq = 0;
				public uint LastTimestamp = 0;
				public int MessageCount = 0;
				public double LastResetTime = 0.0f;
			};

			private static Dictionary<CSteamID, ConnectionSecurity> SecurityStates = new Dictionary<CSteamID, ConnectionSecurity>();
			private static byte[] HMacKey;
			private static readonly object Lock = new object();

			public SteamLobbySecurity() {
				using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
				HMacKey = new byte[ 16 ];
				rng.GetBytes( HMacKey );
			}

			public static byte[] SecureOutgoingMessage( byte[] data, CSteamID target ) {

				return data;
			}
			public static byte[] ProcessIncomingMessage( byte[] secured, CSteamID senderId ) {
				if ( !SecurityStates.TryGetValue( senderId, out ConnectionSecurity state ) ) {
					state = new ConnectionSecurity();
					SecurityStates.Add( senderId, state );
				}

				// rate limiting
				if ( DateTime.UtcNow.TimeOfDay.TotalSeconds - state.LastResetTime > 1.0f ) {
					state.MessageCount = 0;
					state.LastResetTime = DateTime.UtcNow.TimeOfDay.TotalSeconds;
				}
				if ( ++state.MessageCount > 425 ) {
					return null; // 425 msg/sec limit
				}

				return secured;
			}
		};

		private struct VoteKick {
			public readonly CSteamID Target;
			public readonly CSteamID Initiator;
			public readonly float StartTime;
			public readonly string Reason;
			public bool Completed;
			public Dictionary<CSteamID, bool> Votes = new Dictionary<CSteamID, bool>( MAX_LOBBY_MEMBERS );

			public VoteKick( CSteamID target, CSteamID initiator, string reason ) {
				Target = target;
				Initiator = initiator;
				Reason = reason;
				StartTime = Time.GetTicksMsec() / 1000.0f;
				Completed = false;
			}
		};

		private static SteamLobby _Instance;
		public static SteamLobby Instance => _Instance;

		private static readonly uint PACKET_READ_LIMIT = 32;
		public static readonly int MAX_LOBBY_MEMBERS = 16;
		private static readonly float VOTE_KICK_DURATION = 30.0f;
		private static readonly float VOTE_KICK_COOLDOWN = 300.0f;

		private Chat ChatBar;

		private Callback<LobbyEnter_t> LobbyEnter;
		private Callback<LobbyChatMsg_t> LobbyChatMsg;
		private Callback<LobbyChatUpdate_t> LobbyChatUpdate;
		private Callback<SteamNetConnectionStatusChangedCallback_t> ConnectionStatusChanged;

		private CallResult<LobbyCreated_t> OnLobbyCreatedCallResult;
		private CallResult<LobbyEnter_t> OnLobbyEnterCallResult;
		private CallResult<LobbyMatchList_t> OnLobbyMatchListCallResult;

		public CSteamID[] LobbyMembers { get; private set; } = new CSteamID[ MAX_LOBBY_MEMBERS ];
		public int LobbyMemberCount { get; private set; } = 0;
		public CSteamID LobbyId { get; private set; } = CSteamID.Nil;
		public CSteamID LobbyOwnerId { get; private set; } = CSteamID.Nil;
		public bool IsHost { get; private set; } = false;

		private SteamVoiceChat VoiceChat;

		//
		// Connection management
		//
		public Dictionary<CSteamID, HSteamNetConnection> Connections = new Dictionary<CSteamID, HSteamNetConnection>();
		private Dictionary<CSteamID, HSteamNetConnection> PendingConnections = new Dictionary<CSteamID, HSteamNetConnection>();
		private Dictionary<HSteamNetConnection, CSteamID> ConnectionToSteamID = new Dictionary<HSteamNetConnection, CSteamID>();
		private HSteamListenSocket ListenSocket = HSteamListenSocket.Invalid;

		public List<CSteamID> LobbyList { get; private set; } = new List<CSteamID>();
		public int LobbyMaxMembers { get; private set; } = 0;
		public uint LobbyGameMode { get; private set; } = 0;
		public string LobbyName { get; private set; }
		public string LobbyMap { get; private set; }
		public Visibility LobbyVisibility { get; private set; } = Visibility.Public;

		private string LobbyFilterMap;
		private string LobbyFilterGameMode;

		private System.IO.MemoryStream PacketStream = new System.IO.MemoryStream( 2048 );
		private System.IO.BinaryReader PacketReader;

		private CSteamID ThisSteamID = CSteamID.Nil;
		private int ThisSteamIDIndex = 0;

		private Thread NetworkThread;
		private int NetworkRunning = 0;
		private readonly object NetworkLock = new object();

		private Dictionary<CSteamID, VoteKick> ActiveVoteKicks = new Dictionary<CSteamID, VoteKick>();
		private Dictionary<CSteamID, float> VoteKickCooldowns = new Dictionary<CSteamID, float>();

		private Dictionary<int, NetworkNode> NodeCache = new Dictionary<int, NetworkNode>();
		private Dictionary<string, PlayerNetworkNode> PlayerCache = new Dictionary<string, PlayerNetworkNode>();
		private HashSet<NetworkNode> PlayerList = new HashSet<NetworkNode>( MAX_LOBBY_MEMBERS );

		private BufferPool Pool = new BufferPool();
		private IntPtr[] MessagePool = new IntPtr[ PACKET_READ_LIMIT ];

		private static Dictionary<CSteamID, bool> PlayersReady = new Dictionary<CSteamID, bool>( MAX_LOBBY_MEMBERS );

		private Queue<IncomingMessage> MessageQueue = new Queue<IncomingMessage>();

		//
		// message batching
		//
		private Dictionary<CSteamID, System.IO.MemoryStream> Batches = new Dictionary<CSteamID, System.IO.MemoryStream>( MAX_LOBBY_MEMBERS );

		[Signal]
		public delegate void ChatMessageReceivedEventHandler( ulong senderSteamId, string message, int flags = 0 );
		[Signal]
		public delegate void ClientJoinedLobbyEventHandler( ulong steamId );
		[Signal]
		public delegate void ClientLeftLobbyEventHandler( ulong steamId );
		[Signal]
		public delegate void LobbyJoinedEventHandler( ulong lobbyId );
		[Signal]
		public delegate void LobbyCreatedEventHandler( ulong lobbyId );
		[Signal]
		public delegate void LobbyListUpdatedEventHandler();
		[Signal]
		public delegate void StartGameEventHandler();
		[Signal]
		public delegate void LobbyOwnerChangedEventHandler( ulong newOwnerId );
		[Signal]
		public delegate void HostResponsibilitiesTransferredEventHandler();
		[Signal]
		public delegate void VoteKickStartedEventHandler( ulong initiator, ulong target, string reason );
		[Signal]
		public delegate void VoteKickStatusEventHandler( ulong target, int yesVotes, int noVotes, float timeRemaining );
		[Signal]
		public delegate void VoteKickResultEventHandler( ulong target, bool result );
		[Signal]
		public delegate void LobbyConnectionStatusChangedEventHandler( int state );

		private static bool IsValidSendType( int sendType ) {
			return sendType >= Constants.k_nSteamNetworkingSend_Unreliable && sendType <= Constants.k_nSteamNetworkingSend_ReliableNoNagle;
		}
		private static bool IsValidPlayer( CSteamID steamID ) {
			for ( int i = 0; i < Instance.LobbyMemberCount; i++ ) {
				if ( Instance.LobbyMembers[ i ] == steamID ) {
					return true;
				}
			}
			return false;
		}
		private static bool IsValidPlayer( ulong steamID ) {
			for ( int i = 0; i < Instance.LobbyMemberCount; i++ ) {
				if ( Instance.LobbyMembers[ i ] == (CSteamID)steamID ) {
					return true;
				}
			}
			return false;
		}
		public static bool AllPlayersReady() {
			foreach ( var player in PlayersReady ) {
				if ( !player.Value ) {
					return false;
				}
			}
			return true;
		}

		public static HSteamNetConnection FindConnection( CSteamID steamID ) {
			if ( Instance.Connections.TryGetValue( steamID, out HSteamNetConnection hConnection ) ) {
				return hConnection;
			}
			if ( Instance.PendingConnections.TryGetValue( steamID, out hConnection ) ) {
				return hConnection;
			}
			foreach ( var user in Instance.ConnectionToSteamID ) {
				if ( user.Value == steamID ) {
					return user.Key;
				}
			}
			return HSteamNetConnection.Invalid;
		}
		private static void CopyTo( System.IO.Stream src, System.IO.Stream dest ) {
			byte[] bytes = new byte[ 2048 ];
			int count;

			while ( ( count = src.Read( bytes, 0, bytes.Length ) ) != 0 ) {
				dest.Write( bytes, 0, count );
			}
		}
		public static byte[] DecompressText( byte[] text ) {
			// decompress it
			using ( var msi = new System.IO.MemoryStream( text ) ) {
				using ( var mso = new System.IO.MemoryStream() ) {
					using ( var gs = new System.IO.Compression.GZipStream( mso, System.IO.Compression.CompressionMode.Decompress ) ) {
						CopyTo( msi, gs );
					}
					return mso.ToArray();
				}
			}
		}
		public static byte[] CompressText( string text ) {
			// decompress it
			using ( var msi = new System.IO.MemoryStream( System.Text.Encoding.UTF8.GetBytes( text ) ) ) {
				using ( var mso = new System.IO.MemoryStream() ) {
					using ( var gs = new System.IO.Compression.GZipStream( mso, System.IO.Compression.CompressionMode.Compress ) ) {
						CopyTo( msi, gs );
					}
					return mso.ToArray();
				}
			}
		}

		// ==================== Lobby Management ====================
		public void CreateLobby() {
			if ( LobbyId.IsValid() ) {
				Console.PrintError( $"SteamLobby.CreateLobby: LobbyId {LobbyId} isn't valid!" );
				return;
			}
			IsHost = true;

			ELobbyType lobbyType = ELobbyType.k_ELobbyTypePrivate;
			switch ( LobbyVisibility ) {
				case Visibility.Private:
					lobbyType = ELobbyType.k_ELobbyTypePrivate;
					break;
				case Visibility.Public:
					lobbyType = ELobbyType.k_ELobbyTypePublic;
					break;
				case Visibility.FriendsOnly:
					lobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
					break;
			}

			Console.PrintLine( "Initializing SteamLobby..." );
			SteamAPICall_t handle = SteamMatchmaking.CreateLobby( lobbyType, LobbyMaxMembers );
			OnLobbyCreatedCallResult.Set( handle );

			ChatBar = ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/chat_bar.tscn" ).Instantiate<Chat>();
			GetTree().Root.AddChild( ChatBar );
		}

		public void JoinLobby( CSteamID lobbyId ) {
			SteamMatchmaking.JoinLobby( lobbyId );
		}

		public int GetMemberIndex( CSteamID memberId ) {
			for ( int i = 0; i < LobbyMemberCount; i++ ) {
				if ( LobbyMembers[ i ] == memberId ) {
					return i;
				}
			}
			return -1;
		}

		public void LeaveLobby() {
			if ( !LobbyId.IsValid() ) {
				Console.PrintWarning( $"SteamLobby.LeaveLobby: lobby {LobbyId} isn't valid!" );
				return;
			}

			PlayersReady.Clear();

			Console.PrintLine( $"Leaving lobby {LobbyId}..." );

			if ( IsHost && LobbyMemberCount > 1 ) {
				for ( int i = 0; i < LobbyMemberCount; i++ ) {
					if ( LobbyMembers[ i ].IsValid() && LobbyMembers[ i ] != ThisSteamID ) {
						SteamMatchmaking.SetLobbyOwner( LobbyId, LobbyMembers[ i ] );
						break;
					}
				}

				// let steam process the request
				System.Threading.Thread.Sleep( 750 );
			}

			SteamMatchmaking.LeaveLobby( LobbyId );
			LobbyId = CSteamID.Nil;

			// Close all connections
			foreach ( var conn in Connections.Values ) {
				SteamNetworkingSockets.CloseConnection( conn, 0, "Leaving lobby", false );
			}
			foreach ( var conn in PendingConnections.Values ) {
				SteamNetworkingSockets.CloseConnection( conn, 0, "Leaving lobby", false );
			}

			Connections.Clear();
			ConnectionToSteamID.Clear();
			PendingConnections.Clear();

			if ( ListenSocket != HSteamListenSocket.Invalid ) {
				SteamNetworkingSockets.CloseListenSocket( ListenSocket );
				ListenSocket = HSteamListenSocket.Invalid;
			}

			//
			// reset lobby data
			//

			for ( int i = 0; i < LobbyMembers.Length; i++ ) {
				LobbyMembers[ i ] = CSteamID.Nil;
			}
			LobbyMemberCount = 0;

			NodeCache.Clear();
			PlayerCache.Clear();

			EmitSignalClientLeftLobby( (ulong)SteamUser.GetSteamID() );

			if ( ChatBar != null ) {
				GetTree().Root.RemoveChild( ChatBar );
				ChatBar.QueueFree();
			}
		}

		public void OpenLobbyList() {
			SteamMatchmaking.AddRequestLobbyListDistanceFilter( ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide );
			SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
			OnLobbyMatchListCallResult.Set( handle );
		}

		// ==================== Networking ====================
		private void CreateListenSocket() {
			if ( ListenSocket != HSteamListenSocket.Invalid ) {
				SteamNetworkingSockets.CloseListenSocket( ListenSocket );
			}

			SteamNetworkingConfigValue_t[] options = new SteamNetworkingConfigValue_t[] {
			new SteamNetworkingConfigValue_t {
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 64 * 1024 }
			},
			new SteamNetworkingConfigValue_t{
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendRateMin,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 64000 }
			},
			new SteamNetworkingConfigValue_t {
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_RecvBufferSize,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 64 * 1024 }
			},
			new SteamNetworkingConfigValue_t {
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_NagleTime,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 0 }
			},
			new SteamNetworkingConfigValue_t {
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 100000 }
			},
			new SteamNetworkingConfigValue_t {
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 1000000 }
			},
			new SteamNetworkingConfigValue_t {
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SymmetricConnect,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 1 }
			}
		};

			ListenSocket = SteamNetworkingSockets.CreateListenSocketP2P( 0, options.Length, options );

			if ( ListenSocket == HSteamListenSocket.Invalid ) {
				Console.PrintError( "[STEAM] Failed to create listen socket" );
			} else {
				Console.PrintLine( "[STEAM] Created listen socket successfully" );
			}
		}

		private void ConnectToLobbyMembers() {
			for ( int i = 0; i < LobbyMemberCount; i++ ) {
				if ( LobbyMembers[ i ] == ThisSteamID ) {
					continue;
				}

				// Skip if already connected or connecting
				if ( Connections.ContainsKey( LobbyMembers[ i ] ) ) {
					Console.PrintLine( $"Already connected to {SteamFriends.GetFriendPersonaName( LobbyMembers[ i ] )}" );
					continue;
				}
				if ( PendingConnections.ContainsKey( LobbyMembers[ i ] ) ) {
					Console.PrintLine( $"Already connecting to {SteamFriends.GetFriendPersonaName( LobbyMembers[ i ] )}" );
					continue;
				}

				SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
				identity.SetSteamID( LobbyMembers[ i ] );

				HSteamNetConnection conn = SteamNetworkingSockets.ConnectP2P( ref identity, 0, 0, null );

				if ( conn != HSteamNetConnection.Invalid ) {
					PendingConnections[ LobbyMembers[ i ] ] = conn;
					Console.PrintLine( $"[STEAM] Connecting to {SteamFriends.GetFriendPersonaName( LobbyMembers[ i ] )}" );
				} else {
					Console.PrintError( $"[STEAM] Failed to create connection to {SteamFriends.GetFriendPersonaName( LobbyMembers[ i ] )}" );
				}
			}
		}

		private void OnConnectionStatusChanged( SteamNetConnectionStatusChangedCallback_t status ) {
			Console.PrintLine( $"[STEAM CONNECTION] Status changed: {status.m_info.m_eState}" );

			try {
				CSteamID remoteId = status.m_info.m_identityRemote.GetSteamID();
				string remoteName = SteamFriends.GetFriendPersonaName( remoteId );
				Console.PrintLine( $"[STEAM CONNECTION] Remote: {remoteName} ({remoteId})" );

				switch ( status.m_info.m_eState ) {
					case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
						Console.PrintLine( $"[STEAM] Incoming connection request from {remoteName}" );

						// Only accept if this is an incoming connection (we didn't initiate it)
						if ( !PendingConnections.ContainsKey( remoteId ) ) {
							if ( SteamNetworkingSockets.AcceptConnection( status.m_hConn ) == EResult.k_EResultOK ) {
								Console.PrintLine( "[STEAM] Accepted incoming connection" );
							} else {
								Console.PrintError( "[STEAM] Failed to accept incoming connection" );
								SteamNetworkingSockets.CloseConnection( status.m_hConn, 0, "Connection accept failed", false );
							}
						} else {
							Console.PrintLine( "[STEAM] Ignoring outgoing connection request (we initiated it)" );
						}
						break;

					case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
						Console.PrintLine( $"[STEAM] Connected to {remoteName}" );

						Connections[ remoteId ] = status.m_hConn;
						ConnectionToSteamID[ status.m_hConn ] = remoteId;
						PendingConnections.Remove( remoteId );

						SendTargetPacket( remoteId, [ (byte)MessageType.Handshake ] );
						break;

					case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
					case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
						Console.PrintLine( $"[STEAM] Connection closed with {remoteName}" );
						Console.PrintLine( $"[STEAM] Reason: {status.m_info.m_szEndDebug}" );

						if ( ConnectionToSteamID.TryGetValue( status.m_hConn, out CSteamID steamId ) ) {
							Connections.Remove( steamId );
							ConnectionToSteamID.Remove( status.m_hConn );
							PendingConnections.Remove( steamId );

							EmitSignalClientLeftLobby( (ulong)steamId );
						}
						SteamNetworkingSockets.CloseConnection( status.m_hConn, 0, null, false );
						break;

					default:
						Console.PrintLine( $"[STEAM] Unhandled connection state: {status.m_info.m_eState}" );
						break;
				}
				EmitSignalLobbyConnectionStatusChanged( (int)status.m_info.m_eState );
			} catch ( Exception e ) {
				Console.PrintError( $"[STEAM] Error in connection callback: {e.Message}" );
			}
		}

		public void SendMessage( CSteamID target, byte[] data ) {
			if ( !Batches.TryGetValue( target, out var stream ) ) {
				stream = new System.IO.MemoryStream( 2048 );
				Batches.Add( target, stream );
			}

			stream.Write( BitConverter.GetBytes( data.Length ), 0, 4 );
			stream.Write( data, 0, data.Length );
		}
		private void SendBatches() {
			foreach ( var batch in Batches ) {
				if ( batch.Value.Length > 0 ) {
					SendTargetPacket( batch.Key, batch.Value.ToArray(), Constants.k_nSteamNetworkingSend_Unreliable, false, 0 );
					batch.Value.SetLength( 0 );
				}
			}
		}

		public static void SendVoicePacket( byte[] data, uint bytesWritten ) {
			if ( Instance == null ) {
				Console.PrintError( $"SteamLobby.SendVoicePacket: called before validation!" );
				return;
			}

			// steam is doing the compression for us, so no need to worry about sizes too much.
			if ( data == null || data.Length == 0 ) {
				Console.PrintWarning( $"SteamLobby.SendVoicePacket: invalid data packet given" );
				return;
			} else if ( bytesWritten == 0 || bytesWritten > SteamVoiceChat.MaxCompressedSize ) {
				Console.PrintWarning( $"SteamLobby.SendVoicePacket: data packet size mismatch" );
				return;
			}

			// CALM THYSELF!
			unsafe {

				// should we just use Marshal.Copy? nah
				fixed ( byte* pData = data ) {
					byte* pBuffer = stackalloc byte[ SteamVoiceChat.MaxPacketSize ];

					// so we don't have to constantly cast it
					int dataSize = (int)bytesWritten;

					*pBuffer = (byte)MessageType.VoiceChat;

					// reinterpret cast it to an int with a 1-byte offset
					*(int*)( pBuffer + sizeof( SteamLobby.MessageType ) ) = System.Net.IPAddress.HostToNetworkOrder( dataSize );

					// copy the rest at the offset after the type and packet size
					Buffer.MemoryCopy( pData, pBuffer + SteamVoiceChat.VoicePacketHeaderSize, SteamVoiceChat.MaxPacketSize, dataSize );

					Instance.SendPacket( pBuffer, SteamVoiceChat.VoicePacketHeaderSize + dataSize, Constants.k_nSteamNetworkingSend_UnreliableNoDelay );
				}
			}
		}
		public void SendTargetPacket( CSteamID target, byte[] data, int sendType = Constants.k_nSteamNetworkingSend_Reliable, bool safe = false, int channel = 0 ) {
			if ( !Connections.TryGetValue( target, out HSteamNetConnection conn ) ) {
				Console.PrintError( string.Format( "SendTargetPacket: not a valid connection id {0}", target ) );
				return;
			}

			unsafe {
				byte[] buffer = SteamLobbySecurity.SecureOutgoingMessage( data, target );
				fixed ( byte* pBuffer = buffer ) {
					EResult res = SteamNetworkingSockets.SendMessageToConnection(
						conn,
						(IntPtr)pBuffer,
						(uint)buffer.Length,
						sendType,
						out long _
					);
					if ( res != EResult.k_EResultOK ) {
						Console.PrintError( $"[STEAM] Error sending message to {target}: {res}" );
					}
				}
			}
		}

		private unsafe void SendPacket( byte* pData, int bufferSize, int sendType ) {
		}
		public void SendP2PPacket( byte[] data, int sendType = Constants.k_nSteamNetworkingSend_Reliable, int channel = 0 ) {
			if ( data == null ) {
				return;
			} else if ( IsValidSendType( sendType ) ) {
				Console.PrintError( $"SteamLobby.SendP2PPacket: sendType {sendType} is invalid" );
			}
			foreach ( var pair in Connections ) {
				if ( pair.Key != ThisSteamID ) {
					SendTargetPacket( pair.Key, data, sendType, false, channel );
				}
			}
		}

		private void ProcessIncomingMessage( byte[] data, int length, CSteamID sender ) {
			byte[] processed = SteamLobbySecurity.ProcessIncomingMessage( data, sender );
			if ( processed == null ) {
				return;
			}

			byte[] buffer = Pool.Rent();
			Buffer.BlockCopy( processed, 0, buffer, 0, buffer.Length );

			MessageQueue.Enqueue( new IncomingMessage {
				Sender = sender,
				Data = buffer,
				Length = buffer.Length,
				Type = (MessageType)buffer[ 0 ]
			} );
		}

		private void ProcessClientData( ulong senderId, int length, byte[] data ) {
			try {
				PacketStream.SetLength( 0 );
				PacketStream.Write( data, 0, length );
				PacketStream.Position = 1; // skip type byte

				if ( !PlayerCache.TryGetValue( senderId.ToString(), out PlayerNetworkNode node ) ) {
					return;
				}
				node.Receive( senderId, PacketReader );
			}
			finally {
				Pool.Return( data );
			}
		}
		private void ProcessServerSync( ulong senderId, int length, byte[] data ) {
			if ( data == null || data.Length == 0 ) {
				Console.PrintWarning( $"SteamLobby.ProcessServerSync: invalid data packet" );
				return;
			}
			if ( length == 0 || length > data.Length ) {
				Console.PrintWarning( $"SteamLobby.ProcessServerSync: invalid packet length" );
				return;
			}
			CSteamID steamID = (CSteamID)senderId;
			if ( !IsValidPlayer( steamID ) ) {
				Console.PrintWarning( $"SteamLobby.ProcessServerSync: invalid senderId (nil)" );
				return;
			}
			try {
				PacketStream.SetLength( 0 );
				PacketStream.Write( data, 0, length );
				PacketStream.Position = 1; // skip type byte

				CSteamID userId = LobbyMembers[ PacketReader.ReadByte() ];
				if ( userId == ThisSteamID ) {
					PlayerCache[ ThisSteamID.ToString() ].Receive( senderId, PacketReader );
				}
			} catch ( Exception ) {

			}
			finally {
				Pool.Return( data );
			}
		}
		private void ProcessGameData( ulong senderId, int length, byte[] data ) {
			PacketStream.SetLength( 0 );
			PacketStream.Write( data, 0, length );
			PacketStream.Position = 1; // skip type byte

			int hash = PacketReader.ReadInt32();
			if ( !NodeCache.TryGetValue( hash, out NetworkNode node ) ) {
				return;
			}

			try {
				node.Receive( PacketReader );
			}
			finally {
				Pool.Return( data );
			}
		}
		private void ProcessServerCommand( ulong senderId, byte[] data ) {
			if ( data == null || data.Length == 0 ) {
				Console.PrintError( $"SteamLobby.ProcessServerCommand: invalid data" );
				return;
			}
			CSteamID steamID = (CSteamID)senderId;
			if ( steamID == CSteamID.Nil || !IsValidPlayer( senderId ) ) {

			}

			PacketStream.SetLength( 0 );
			PacketStream.Write( data, 0, data.Length );
			PacketStream.Position = 1; // skip type byte

			try {
				ServerCommandType commandType = (ServerCommandType)PacketReader.ReadByte();
				Console.PrintLine( $"Received ServerCommand {commandType} from user {senderId}" );
				ServerCommandManager.ExecuteCommand( (CSteamID)senderId, commandType );
			}
			finally {
				Pool.Return( data );
			}
		}
		private void HandleIncomingMessage( IncomingMessage msg ) {
			switch ( msg.Type ) {
				case MessageType.ClientData:
					CallDeferred( MethodName.ProcessClientData, (ulong)msg.Sender, msg.Length, msg.Data );
					break;
				case MessageType.ServerSync:
					CallDeferred( MethodName.ProcessServerSync, (ulong)msg.Sender, msg.Length, msg.Data );
					break;
				case MessageType.GameData:
					CallDeferred( MethodName.ProcessGameData, (ulong)msg.Sender, msg.Length, msg.Data );
					break;
				case MessageType.VoiceChat:
					VoiceChat.ProcessIncomingVoice( (ulong)msg.Sender, msg.Data );
					break;
				case MessageType.Handshake:
					break;
				case MessageType.ServerCommand:
					CallDeferred( MethodName.ProcessServerCommand, (ulong)msg.Sender, msg.Data );
					break;
			}
		}
		private void HandleIncomingMessages() {
			int processed = 0;
			while ( MessageQueue.TryDequeue( out IncomingMessage msg ) && ++processed < 90 ) {
				HandleIncomingMessage( msg );
			}
		}

		private void PollIncomingMessages() {
			IntPtr[] messages = new nint[ PACKET_READ_LIMIT ];
			byte[] tempBuffer = Pool.Rent();

			foreach ( var conn in Connections.Values.Concat( PendingConnections.Values ) ) {
				int count = SteamNetworkingSockets.ReceiveMessagesOnConnection( conn, messages, messages.Length );

				for ( int i = 0; i < count; i++ ) {
					try {
						SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>( messages[ i ] );
						int bytesToCopy = message.m_cbSize;

						if ( System.Numerics.Vector.IsHardwareAccelerated && bytesToCopy >= System.Numerics.Vector<byte>.Count ) {
							int vectorSize = System.Numerics.Vector<byte>.Count;
							int offset = 0;
							unsafe {
								byte* src = (byte*)message.m_pData;
								fixed ( byte* dst = tempBuffer ) {
									while ( offset <= bytesToCopy - vectorSize ) {
										System.Numerics.Vector<byte> vector = System.Numerics.Vector.Load( src + offset );
										System.Numerics.Vector.Store( vector, dst + offset );
										offset += vectorSize;
									}
								}
								for ( int j = offset; j < bytesToCopy; j++ ) {
									tempBuffer[ j ] = src[ j ];
								}
							}
						} else {
							Marshal.Copy( message.m_pData, tempBuffer, 0, bytesToCopy );
						}
						ProcessIncomingMessage( tempBuffer, bytesToCopy, message.m_identityPeer.GetSteamID() );
					}
					finally {
						SteamNetworkingMessage_t.Release( messages[ i ] );
					}
				}
			}
		}

		/*
		===============
		OnLobbyCreated
		===============
		*/
		private void OnLobbyCreated( LobbyCreated_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_eResult != EResult.k_EResultOK ) {
				Console.PrintLine( $"[STEAM] Error creating lobby: {pCallback.m_eResult}" );
				return;
			}

			LobbyId = (CSteamID)pCallback.m_ulSteamIDLobby;
			IsHost = true;

			SteamMatchmaking.SetLobbyJoinable( LobbyId, true );
			SteamMatchmaking.SetLobbyData( LobbyId, "appid", SteamManager.GetAppID().ToString() );
			SteamMatchmaking.SetLobbyData( LobbyId, "gametype", GameConfiguration.GameMode.ToString() );
			SteamMatchmaking.SetLobbyData( LobbyId, "name", LobbyName );
			SteamMatchmaking.SetLobbyData( LobbyId, "map", LobbyMap );
			SteamMatchmaking.SetLobbyData( LobbyId, "gamemode", LobbyGameMode.ToString() );
			SteamMatchmaking.SetLobbyMemberLimit( LobbyId, LobbyMaxMembers );

			PlayersReady.EnsureCapacity( MAX_LOBBY_MEMBERS );
			PlayersReady.TryAdd( ThisSteamID, true );

			Console.PrintLine( $"Created lobby [{LobbyId}] Name: {LobbyName}, MaxMembers: {LobbyMaxMembers}, GameType: {GameConfiguration.GameMode}" );
			CreateListenSocket();
			EmitSignalLobbyCreated( (ulong)LobbyId );
		}

		/*
		===============
		OnLobbyMatchList
		===============
		*/
		private void OnLobbyMatchList( LobbyMatchList_t pCallback, bool bIOFailure ) {
			LobbyList.Clear();
			for ( int i = 0; i < pCallback.m_nLobbiesMatching; i++ ) {
				LobbyList.Add( SteamMatchmaking.GetLobbyByIndex( i ) );
			}
			EmitSignalLobbyListUpdated();
		}

		/*
		===============
		OnLobbyJoined
		===============
		*/
		private void OnLobbyJoined( LobbyEnter_t pCallback ) {
			OnLobbyJoined( pCallback, false );
		}
		
		/*
		===============
		OnLobbyJoined
		===============
		*/
		private void OnLobbyJoined( LobbyEnter_t pCallback, bool bIOFailure ) {
			if ( pCallback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess ) {
				Console.PrintError( $"[STEAM] Error joining lobby {pCallback.m_ulSteamIDLobby}: " +
					$"{(EChatRoomEnterResponse)pCallback.m_EChatRoomEnterResponse}" );
				return;
			}

			LobbyId = (CSteamID)pCallback.m_ulSteamIDLobby;
			LobbyOwnerId = SteamMatchmaking.GetLobbyOwner( LobbyId );

			Console.PrintLine( $"Joined lobby {pCallback.m_ulSteamIDLobby}" );
			LobbyName = SteamMatchmaking.GetLobbyData( LobbyId, "name" );

			switch ( SteamMatchmaking.GetLobbyData( LobbyId, "gametype" ) ) {
				case "Online":
					GameConfiguration.SetGameMode( GameMode.Online );
					break;
				case "Multiplayer":
					GameConfiguration.SetGameMode( GameMode.Multiplayer );
					LobbyMap = SteamMatchmaking.GetLobbyData( LobbyId, "map" );
					LobbyGameMode = Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );
					Console.PrintLine( $"Lobby map: {LobbyMap}" );
					break;
			}

			GetLobbyMembers();
			ConnectToLobbyMembers();
			EmitSignalLobbyJoined( (ulong)LobbyId );

			ChatBar = SceneCache.GetScene( "res://scenes/multiplayer/chat_bar.tscn" ).Instantiate<Chat>();
			GetTree().Root.AddChild( ChatBar );
		}

		/*
		===============
		OnLobbyChatMsg
		===============
		*/
		private void OnLobbyChatMsg( LobbyChatMsg_t pCallback ) {
			byte[] buffer = new byte[ 1024 ];

			int bufferLength = SteamMatchmaking.GetLobbyChatEntry(
				(CSteamID)pCallback.m_ulSteamIDLobby,
				(int)pCallback.m_iChatID,
				out CSteamID senderId,
				buffer,
				buffer.Length,
				out EChatEntryType entryType
			);

			if ( entryType == EChatEntryType.k_EChatEntryTypeChatMsg ) {
				string message = System.Text.Encoding.UTF8.GetString( buffer, 0, bufferLength );
				EmitSignalChatMessageReceived( (ulong)senderId, message, (int)ChatMessageType.None );
			}
		}

		/*
		===============
		TakeOverHostResponsibilites
		===============
		*/
		private void TakeOverHostResponsibilities() {
			CreateListenSocket();

			// TODO: gamestate changes?

			EmitSignalHostResponsibilitiesTransferred();
		}

		/*
		===============
		TransferLobbyOwnership
		===============
		*/
		private void TransferLobbyOwnership() {
			Console.PrintLine( "[STEAM] Lobby master has left, transferring ownership..." );

			LobbyOwnerId = SteamMatchmaking.GetLobbyOwner( LobbyId );
			IsHost = LobbyOwnerId == ThisSteamID;

			Console.PrintLine( string.Format( "[STEAM] Lobby master is now {0}.", SteamFriends.GetFriendPersonaName( LobbyOwnerId ) ) );
			EmitSignalLobbyOwnerChanged( (ulong)LobbyOwnerId );

			if ( IsHost ) {
				TakeOverHostResponsibilities();
			}

			// notify all other clients
			ServerCommandManager.SendCommand( ServerCommandType.OwnershipChanged );
		}

		/*
		===============
		OnLobbyChatUpdate
		===============
		*/
		private void OnLobbyChatUpdate( LobbyChatUpdate_t pCallback ) {
			string changerName = SteamFriends.GetFriendPersonaName( (CSteamID)pCallback.m_ulSteamIDMakingChange );
			var stateChange = (EChatMemberStateChange)pCallback.m_rgfChatMemberStateChange;

			switch ( stateChange ) {
				case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
					ChatUpdate( $"{changerName} has joined..." );
					EmitSignalClientJoinedLobby( pCallback.m_ulSteamIDMakingChange );
					break;
				case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
				case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
				case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
				case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
					ChatUpdate( $"{changerName} has left..." );
					EmitSignalClientLeftLobby( pCallback.m_ulSteamIDMakingChange );

					// check for ownership change
					if ( (CSteamID)pCallback.m_ulSteamIDUserChanged == LobbyOwnerId ) {
						TransferLobbyOwnership();
					}
					break;
			}
		}

		/*
		===============
		GetMemberPing
		===============
		*/
		public int GetMemberPing( CSteamID userID ) {
			if ( !Connections.TryGetValue( userID, out HSteamNetConnection conn ) && !PendingConnections.TryGetValue( userID, out conn ) ) {
				return int.MaxValue;
			}

			SteamNetConnectionRealTimeStatus_t status = new SteamNetConnectionRealTimeStatus_t();
			SteamNetConnectionRealTimeLaneStatus_t laneStatus = new SteamNetConnectionRealTimeLaneStatus_t();

			if ( SteamNetworkingSockets.GetConnectionRealTimeStatus( conn, ref status, 0, ref laneStatus ) != EResult.k_EResultOK ) {
				Console.PrintError( string.Format( "[STEAM] Error getting connection real-time status from user {0}", userID ) );
				return int.MaxValue;
			}

			return status.m_nPing;
		}

		/*
		===============
		GetLobbyMembers
		===============
		*/
		public void GetLobbyMembers() {
			LobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers( LobbyId );
			for ( int i = 0; i < LobbyMembers.Length; i++ ) {
				LobbyMembers[ i ] = CSteamID.Nil;
			}
			for ( int i = 0; i < LobbyMemberCount; i++ ) {
				LobbyMembers[ i ] = SteamMatchmaking.GetLobbyMemberByIndex( LobbyId, i );
				if ( LobbyMembers[ i ] == ThisSteamID ) {
					ThisSteamIDIndex = i;
				}
				Console.PrintLine( string.Format( "[STEAM] LobbyMember ID {0} at index {1}", LobbyMembers[ i ], i ) );
			}
		}

		/*
		===============
		MakeP2PHandshake
		===============
		*/
		private void MakeP2PHandshake() {
			SendP2PPacket( [ (byte)MessageType.Handshake ] );
		}

		/*
		===============
		ChatUpdate
		===============
		*/
		private void ChatUpdate( string status ) {
			Console.PrintLine( status );
		}

		/*
		===============
		CmdLobbyInfo
		===============
		*/
		private void CmdLobbyInfo() {
			Console.PrintLine( "[STEAM LOBBY METADATA]" );
			Console.PrintLine( $"Name: {LobbyName}" );
			Console.PrintLine( $"MaxMembers: {LobbyMaxMembers}" );
			Console.PrintLine( $"MemberCount: {LobbyMemberCount}" );
			Console.PrintLine( $"LobbyId: {LobbyId}" );
			Console.PrintLine( $"GameMode: {LobbyGameMode}" );
			Console.PrintLine( $"Map: {LobbyMap}" );
			Console.PrintLine( $"IsValid: {LobbyId.IsValid()}" );
			Console.PrintLine( $"IsLobby: {LobbyId.IsLobby()}" );
		}

		/*
		===============
		GetPlayer
		===============
		*/
		public NetworkPlayer GetPlayer( CSteamID userId ) {
			return PlayerCache[ userId.ToString() ].Node as NetworkPlayer;
		}

		/*
		===============
		AddPlayer
		===============
		*/
		public void AddPlayer( CSteamID userId, PlayerNetworkNode callbacks ) {
			Console.PrintLine( $"Added player with hash {userId} to network sync cache." );
			PlayerCache.TryAdd( userId.ToString(), callbacks );
		}

		/*
		===============
		RemovePlayer
		===============
		*/
		public void RemovePlayer( CSteamID userId ) {
			PlayerCache.Remove( userId.ToString() );
		}

		/*
		===============
		AddNetworkNode
		===============
		*/
		public void AddNetworkNode( NodePath node, NetworkNode callbacks ) {
			Console.PrintLine( $"Added node with hash {node.GetHashCode()} to network sync cache." );
			NodeCache.TryAdd( node.GetHashCode(), callbacks );
		}

		/*
		===============
		RemoveNetworkNode
		===============
		*/
		public void RemoveNetworkNode( NodePath node ) {
			if ( NodeCache.ContainsKey( node.GetHashCode() ) ) {
				NodeCache.Remove( node.GetHashCode() );
			}
		}

		/*
		===============
		GetNetworkNode
		===============
		*/
		public Node GetNetworkNode( NodePath node ) {
			if ( NodeCache.TryGetValue( node.GetHashCode(), out NetworkNode value ) ) {
				return value.Node;
			}
			Console.PrintError( $"SteamLobby.GetNetworkNode: invalid network node {node.GetHashCode()}!" );
			return null;
		}

		/*
		===============
		InitializeSteamNetworking
		===============
		*/
		private static void InitializeSteamNetworking() {
			try {
				// Initialize relay network access
				SteamNetworkingUtils.InitRelayNetworkAccess();
				Console.PrintLine( "[STEAM] Relay network access initialized" );

				// Set debug output for detailed diagnostics
				SteamNetworkingUtils.SetDebugOutputFunction(
					ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Verbose,
					( type, message ) => {
						Console.PrintLine( $"[STEAM NET] {type}: {message}" );
					}
				);

				// Set identity for local user
				SteamNetworkingIdentity localIdentity = new SteamNetworkingIdentity();
				localIdentity.SetSteamID( SteamUser.GetSteamID() );
				SteamNetworkingSockets.ResetIdentity( ref localIdentity );
				Console.PrintLine( $"[STEAM] Set local identity: {SteamUser.GetSteamID()}" );

				// Check network status
				ESteamNetworkingAvailability status = SteamNetworkingUtils.GetRelayNetworkStatus( out SteamRelayNetworkStatus_t relayStatus );
				Console.PrintLine( $"[STEAM] Relay network status: {status}" );
			} catch ( Exception e ) {
				Console.PrintError( $"[STEAM] Steam networking initialization failed: {e.Message}" );
			}
		}

		/*
		===============
		CastVote
		===============
		*/
		public void CastVote( CSteamID target, bool vote ) {
			if ( !ActiveVoteKicks.TryGetValue( target, out VoteKick voteKick ) ) {
				Console.PrintLine( string.Format( "No active votekick in progress for player {0}", target ) );
				return;
			}

			// can't vote for yourself
			if ( ThisSteamID == voteKick.Target ) {
				return;
			}

			if ( voteKick.Votes.ContainsKey( ThisSteamID ) ) {
				return;
			}

			voteKick.Votes[ ThisSteamID ] = vote;

			if ( vote ) {
				ServerCommandManager.SendCommand( ServerCommandType.VoteKickResponse_Yes );
			} else {
				ServerCommandManager.SendCommand( ServerCommandType.VoteKickResponse_No );
			}
		}

		/*
		===============
		StartVoteKick
		===============
		*/
		public void StartVoteKick( CSteamID target, string reason ) {
			if ( !LobbyId.IsValid() ) {
				return;
			}

			// can't vote yours truly out
			if ( target == SteamManager.VIP_ID ) {
				Console.PrintLine( "The AUDACITY of this BIATCH!" );
				return;
			}

			// check if we're spamming
			if ( VoteKickCooldowns.TryGetValue( ThisSteamID, out float lastVoteTime ) ) {
				float cooldownRemaining = lastVoteTime + VOTE_KICK_COOLDOWN - Time.GetTicksMsec() / 1000.0f;
				if ( cooldownRemaining > 0.0f ) {
					Console.PrintLine( "You cannot vote kick more than once per 5 minutes." );
					return;
				}
			}

			if ( ActiveVoteKicks.ContainsKey( target ) ) {
				Console.PrintLine( string.Format( "Votekick already activated for player {0}", target ) );
				return;
			}

			VoteKick vote = new VoteKick(
				target: target,
				initiator: ThisSteamID,
				reason: reason
			);

			vote.Votes[ ThisSteamID ] = true;

			ActiveVoteKicks[ target ] = vote;
			VoteKickCooldowns[ ThisSteamID ] = vote.StartTime;

			BroadcastVoteKickStart( vote );

			if ( IsHost ) {
				//			CheckVoteKickStatus( target );
			}
		}

		/*
		===============
		BroadcastVoteKickStart
		===============
		*/
		private void BroadcastVoteKickStart( VoteKick vote ) {
			ServerCommandManager.SendCommand( ServerCommandType.StartVoteKick );

			EmitSignalVoteKickStarted( (ulong)vote.Initiator, (ulong)vote.Target, vote.Reason );
		}

		/*
		===============
		CmdKickPlayer
		===============
		*/
		private void CmdKickPlayer( string username ) {
			if ( !IsHost ) {
				return; // no permissions
			}
		}

		/*
		===============
		CmdTransferOwnership
		===============
		*/
		private void CmdTransferOwnership( string username ) {
			if ( !IsHost ) {
				return; // no permissions
			}

			for ( int i = 0; i < LobbyMemberCount; i++ ) {
				if ( LobbyMembers[ i ].IsValid() && SteamFriends.GetFriendPersonaName( LobbyMembers[ i ] ) == username ) {
					Console.PrintLine( string.Format( "[STEAM] Transferring lobby ownership to {0}...", SteamFriends.GetFriendPersonaName( LobbyMembers[ i ] ) ) );

					IsHost = false;
					LobbyOwnerId = LobbyMembers[ i ];

					SteamMatchmaking.SetLobbyOwner( LobbyId, LobbyMembers[ i ] );
					break;
				}
			}
		}

		/*
		===============
		CmdNetworkingProfile
		===============
		*/
		private void CmdNetworkingProfile() {
			GetTree().Root.GetNode<NetworkingMonitor>( "NetworkingMonitor" ).Visible = !GetTree().Root.GetNode<NetworkingMonitor>( "NetworkingMonitor" ).Visible;
		}

		public void SetLobbyName( string name ) {
			ArgumentException.ThrowIfNullOrEmpty( name );
			LobbyName = name;
		}

		public void SetMaxMembers( int memberLimit ) {
			if ( memberLimit < 0 || memberLimit > MAX_LOBBY_MEMBERS ) {
				throw new ArgumentOutOfRangeException( $"memberLimit {memberLimit} is less than 0 or greater than MAX_LOBBY_MEMBERS ({MAX_LOBBY_MEMBERS})" );
			}
			LobbyMaxMembers = memberLimit;
		}

		public void SetMap( string map ) {
			ArgumentException.ThrowIfNullOrEmpty( map );
			LobbyMap = map;
		}

		public void SetGameMode( uint gameMode ) {
			if ( gameMode < (uint)Mode.GameMode.Bloodbath || gameMode >= (uint)Mode.GameMode.Count ) {
				throw new ArgumentOutOfRangeException( nameof( gameMode ) );
			}
			LobbyGameMode = gameMode;
		}

		public void SetHostStatus( bool isHost ) {
			IsHost = isHost;
		}

		/*
		===============
		_EnterTree
		===============
		*/
		public override void _EnterTree() {
			base._EnterTree();
			if ( _Instance != null ) {
				this.QueueFree();
			}
			_Instance = this;
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			InitializeSteamNetworking();

			// Register callbacks
			LobbyEnter = Callback<LobbyEnter_t>.Create( OnLobbyJoined );
			LobbyChatMsg = Callback<LobbyChatMsg_t>.Create( OnLobbyChatMsg );
			LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create( OnLobbyChatUpdate );
			ConnectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create( OnConnectionStatusChanged );

			OnLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create( OnLobbyCreated );
			OnLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create( OnLobbyJoined );
			OnLobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create( OnLobbyMatchList );

			SetPhysicsProcess( true );
			SetProcess( true );

			PacketReader = new System.IO.BinaryReader( PacketStream );

			// Start lobby discovery
			OpenLobbyList();

			ServerCommandManager.RegisterCommandCallback( ServerCommandType.OwnershipChanged, new Action<CSteamID>( ( senderId ) => {
				LobbyOwnerId = SteamMatchmaking.GetLobbyOwner( LobbyId );
				IsHost = LobbyOwnerId == ThisSteamID;
			} ) );

			// Start network thread
			NetworkRunning = 1;
			NetworkThread = new System.Threading.Thread( NetworkThreadProcess ) {
				Priority = System.Threading.ThreadPriority.Highest
			};

			VoiceChat = GetNode<SteamVoiceChat>( "/root/SteamVoiceChat" );

			// Add console command
			Console.AddCommand( "lobby_info", Callable.From( CmdLobbyInfo ), Array.Empty<string>(), 0, "prints lobby information." );
			Console.AddCommand( "network_prof", Callable.From( CmdNetworkingProfile ), Array.Empty<string>(), 0, "prints SteamNetworkingSockets performance information." );
			Console.AddCommand( "kick_player", Callable.From<string>( CmdKickPlayer ), [ "username" ], 1, "kicks a player." );
			Console.AddCommand( "transfer_ownership", Callable.From<string>( CmdTransferOwnership ), [ "username" ], 1, "transfers lobby ownership to specified player." );

			// Set local Steam ID
			ThisSteamID = SteamManager.GetSteamID();
			Console.PrintLine( $"[STEAM] Local Steam ID: {ThisSteamID}" );

			ServerCommandManager.RegisterCommandCallback( ServerCommandType.PlayerReady, ( senderId ) => { PlayersReady[ senderId ] = true; } );
		}

		/*
		===============
		NetworkThreadProcess
		===============
		*/
		private void NetworkThreadProcess() {
			const int FRAME_LIMIT = 60;
			const double FRAME_TIME_MS = 1000.0f / FRAME_LIMIT;
			Stopwatch frameTimer = new Stopwatch();

			while ( NetworkRunning == 1 ) {
				frameTimer.Restart();

				try {
					PollIncomingMessages();
					HandleIncomingMessages();

					SteamNetworkingSockets.RunCallbacks();
				} catch ( Exception e ) {
					Console.PrintError( string.Format( "[STEAM] Networking thread exception: {0}", e.Message ) );
				}

				double elapsed = frameTimer.Elapsed.TotalMilliseconds;
				double sleepTime = FRAME_TIME_MS - elapsed;
				if ( sleepTime > 0.0f ) {
					System.Threading.Thread.Sleep( (int)sleepTime );
				}
			}
		}

		/*
		===============
		_Process
		===============
		*/
		public override void _Process( double delta ) {
			// Send updates
			foreach ( var node in NodeCache.Values ) {
				node.Send?.Invoke();
			}
			foreach ( var player in PlayerCache.Values ) {
				player.Send?.Invoke();
			}
			SendBatches();
		}

		/*
		===============
		_Notification
		===============
		*/
		public override void _Notification( int what ) {
			base._Notification( what );

			if ( what == NotificationWMCloseRequest ) {
				System.Threading.Interlocked.Exchange( ref NetworkRunning, 1 );

				LeaveLobby();

				foreach ( var conn in Connections.Values ) {
					SteamNetworkingSockets.CloseConnection( conn, 0, "Shutdown", false );
				}

				if ( NetworkThread.IsAlive ) {
					NetworkThread.Join( 1000 );
				}
			}
		}
	};
};