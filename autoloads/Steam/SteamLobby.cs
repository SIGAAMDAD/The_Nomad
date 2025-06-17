using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Multiplayer;
using System.Threading.Tasks.Dataflow;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;
using GDExtension.Wrappers;

public partial class SteamLobby : Node {
	public enum Visibility {
		Private,
		Public,
		FriendsOnly
	};

	public enum MessageType : byte {
		Handshake,
		ServerCommand,
		ClientData,
		GameData,

		ChatMessage_TeamOnly,
		ChatMessage_PlayerOnly,
		ChatMessage_FriendsOnly,

		EncryptionKey,

		Count
	};

	public enum ChatMessageType : int {
		FriendsOnly,
		TeamOnly,
		PlayerOnly,
		None
	};

	public readonly struct NetworkNode {
		public readonly Node Node;
		public readonly Action Send;
		public readonly Action<System.IO.BinaryReader> Receive;

		public NetworkNode( Node node, Action send, Action<System.IO.BinaryReader> receive ) {
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
			lock ( Lock ) {
				if ( !SecurityStates.TryGetValue( target, out var state ) ) {
					SecurityStates.Add( target, new ConnectionSecurity() );
				}
				return data;
				/*

				var header = new SecurityHeader {
					Version = 1,
					Sequence = ++state.LastOutSeq,
					Timestamp = SteamUtils.GetServerRealTime()
				};

				byte[] secured = new byte[ 7 + data.Length ];
				using ( var stream = new System.IO.MemoryStream( secured ) ) {
					using ( var writer = new System.IO.BinaryWriter( stream ) ) {
						writer.Write( header.Version );
						writer.Write( header.Sequence );
						writer.Write( header.Timestamp );
						writer.Write( data );
					}
				}
				return secured;
				*/
			}
		}
		public static byte[] ProcessIncomingMessage( byte[] secured, CSteamID senderId ) {
			lock ( Lock ) {
				if ( !SecurityStates.TryGetValue( senderId, out ConnectionSecurity state ) ) {
					state = new ConnectionSecurity();
					SecurityStates.Add( senderId, state );
				}

				// rate limiting
				if ( DateTime.UtcNow.TimeOfDay.TotalSeconds - state.LastResetTime > 1.0f ) {
					state.MessageCount = 0;
					state.LastResetTime = DateTime.UtcNow.TimeOfDay.TotalSeconds;
				}
				if ( ++state.MessageCount > 350 ) {
					return null; // 350 msg/sec limit
				}

				return secured;

				// sanity checks
				/*
				if ( secured.Length < 7 ) {
					return null;
				}
				var header = new SecurityHeader {
					Version = secured[ 0 ],
					Sequence = BitConverter.ToUInt16( secured, 1 ),
					Timestamp = BitConverter.ToUInt32( secured, 3 )
				};

				if ( header.Sequence <= state.LastInSeq && state.LastInSeq - header.Sequence < 50 ) {
					return null;
				}
				state.LastInSeq = header.Sequence;

				uint currentTime = SteamUtils.GetServerRealTime();
				if ( Math.Abs( (long)currentTime - header.Timestamp ) > 120 ) {
					return null;
				}

				byte[] data = new byte[ secured.Length - 7 ];
				Buffer.BlockCopy( secured, 7, data, 0, data.Length );
				return data;
				*/
			}
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

	private const uint PACKET_READ_LIMIT = 32;
	public static readonly int MAX_LOBBY_MEMBERS = 16;

	private Chat ChatBar;

	private Callback<LobbyEnter_t> LobbyEnter;
	private Callback<LobbyChatMsg_t> LobbyChatMsg;
	private Callback<LobbyChatUpdate_t> LobbyChatUpdate;
	private Callback<SteamNetConnectionStatusChangedCallback_t> ConnectionStatusChanged;

	private CallResult<LobbyCreated_t> OnLobbyCreatedCallResult;
	private CallResult<LobbyEnter_t> OnLobbyEnterCallResult;
	private CallResult<LobbyMatchList_t> OnLobbyMatchListCallResult;

	public CSteamID[] LobbyMembers = new CSteamID[ MAX_LOBBY_MEMBERS ];
	public int LobbyMemberCount = 0;
	private CSteamID LobbyId = CSteamID.Nil;
	private CSteamID LobbyOwnerId = CSteamID.Nil;
	private bool IsHost = false;

	// Connection management
	private Dictionary<CSteamID, HSteamNetConnection> Connections = new Dictionary<CSteamID, HSteamNetConnection>();
	private Dictionary<CSteamID, HSteamNetConnection> PendingConnections = new Dictionary<CSteamID, HSteamNetConnection>();
	private Dictionary<HSteamNetConnection, CSteamID> ConnectionToSteamID = new Dictionary<HSteamNetConnection, CSteamID>();
	private HSteamListenSocket ListenSocket = HSteamListenSocket.Invalid;
	private readonly object ConnectionLock = new object();

	private List<CSteamID> LobbyList = new List<CSteamID>();
	private int LobbyMaxMembers = 0;
	private uint LobbyGameMode = 0;
	private string LobbyName;
	private string LobbyMap;
	private Visibility LobbyVisibility = Visibility.Public;

	private string LobbyFilterMap;
	private string LobbyFilterGameMode;

	private IntPtr CachedWritePacket;
	private byte[] CachedPacket = null;

	private CSteamID ThisSteamID = CSteamID.Nil;
	private int ThisSteamIDIndex = 0;

	private System.Threading.Thread NetworkThread;
	private int NetworkRunning = 0;
	private readonly object NetworkLock = new object();

	private Dictionary<CSteamID, VoteKick> ActiveVoteKicks = new Dictionary<CSteamID, VoteKick>();
	private Dictionary<CSteamID, float> VoteKickCooldowns = new Dictionary<CSteamID, float>();
	private static readonly float VOTE_KICK_DURATION = 30.0f;
	private static readonly float VOTE_KICK_COOLDOWN = 300.0f;

	private Dictionary<int, NetworkNode> NodeCache = new Dictionary<int, NetworkNode>();
	private Dictionary<string, NetworkNode> PlayerCache = new Dictionary<string, NetworkNode>();
	private HashSet<NetworkNode> PlayerList = new HashSet<NetworkNode>( MAX_LOBBY_MEMBERS );

	private BufferPool Pool = new BufferPool();
	private IntPtr[] MessagePool = new IntPtr[ PACKET_READ_LIMIT ];

	//
	// message batching
	//
	private Dictionary<CSteamID, System.IO.MemoryStream> Batches = new Dictionary<CSteamID, System.IO.MemoryStream>( MAX_LOBBY_MEMBERS );
	private readonly object BatchLock = new object();

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

	// Message processing
	private struct IncomingMessage {
		public CSteamID Sender;
		public byte[] Data;
		public MessageType Type;
	};
	private Queue<IncomingMessage> MessageQueue = new Queue<IncomingMessage>();

	public static Dictionary<CSteamID, HSteamNetConnection> GetConnections() => Instance.Connections;

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
		};

		Console.PrintLine( "Initializing SteamLobby..." );
		SteamAPICall_t handle = SteamMatchmaking.CreateLobby( lobbyType, LobbyMaxMembers );
		OnLobbyCreatedCallResult.Set( handle );

		ChatBar = ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/chat_bar.tscn" ).Instantiate<Chat>();
		GetTree().Root.AddChild( ChatBar );
	}

	public void JoinLobby( CSteamID lobbyId ) {
		SteamMatchmaking.JoinLobby( lobbyId );
		ChatBar = ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/chat_bar.tscn" ).Instantiate<Chat>();
		GetTree().Root.AddChild( ChatBar );
	}

	public void LeaveLobby() {
		if ( !LobbyId.IsValid() ) {
			return;
		}

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
		lock ( ConnectionLock ) {
			foreach ( var conn in Connections.Values ) {
				SteamNetworkingSockets.CloseConnection( conn, 0, "Leaving lobby", false );
			}
			foreach ( var conn in PendingConnections.Values ) {
				SteamNetworkingSockets.CloseConnection( conn, 0, "Leaving lobby", false );
			}

			Connections.Clear();
			ConnectionToSteamID.Clear();
			PendingConnections.Clear();
		}
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

		if ( ChatBar != null && ChatBar.IsInsideTree() ) {
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
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 2500 }
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

			lock ( ConnectionLock ) {
				// Skip if already connected or connecting
				if ( Connections.ContainsKey( LobbyMembers[ i ] ) ) {
					Console.PrintLine( $"Already connected to {SteamFriends.GetFriendPersonaName( LobbyMembers[ i ] )}" );
					continue;
				}
				if ( PendingConnections.ContainsKey( LobbyMembers[ i ] ) ) {
					Console.PrintLine( $"Already connecting to {SteamFriends.GetFriendPersonaName( LobbyMembers[ i ] )}" );
					continue;
				}
			}

			SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
			identity.SetSteamID( LobbyMembers[ i ] );

			HSteamNetConnection conn = SteamNetworkingSockets.ConnectP2P( ref identity, 0, 0, null );

			if ( conn != HSteamNetConnection.Invalid ) {
				lock ( ConnectionLock ) {
					PendingConnections[ LobbyMembers[ i ] ] = conn;
				}
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
				bool isOutgoing = false;
				lock ( ConnectionLock ) {
					isOutgoing = PendingConnections.ContainsKey( remoteId );
				}

				if ( !isOutgoing ) {
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

				lock ( ConnectionLock ) {
					Connections[ remoteId ] = status.m_hConn;
					ConnectionToSteamID[ status.m_hConn ] = remoteId;
					PendingConnections.Remove( remoteId );
				}

				// Send handshake to new connection
				SendTargetPacket( remoteId, [ (byte)MessageType.Handshake ] );
				break;

			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
				Console.PrintLine( $"[STEAM] Connection closed with {remoteName}" );
				Console.PrintLine( $"[STEAM] Reason: {status.m_info.m_szEndDebug}" );

				lock ( ConnectionLock ) {
					if ( ConnectionToSteamID.TryGetValue( status.m_hConn, out CSteamID steamId ) ) {
						Connections.Remove( steamId );
						ConnectionToSteamID.Remove( status.m_hConn );
						PendingConnections.Remove( steamId );

						// Notify about disconnected player
						EmitSignalClientLeftLobby( (ulong)steamId );
					}
				}
				SteamNetworkingSockets.CloseConnection( status.m_hConn, 0, null, false );
				break;

			default:
				Console.PrintLine( $"[STEAM] Unhandled connection state: {status.m_info.m_eState}" );
				break;
			}
		} catch ( Exception e ) {
			Console.PrintError( $"[STEAM] Error in connection callback: {e.Message}" );
		}
	}

	public void SendMessage( CSteamID target, byte[] data ) {
		lock ( BatchLock ) {
			if ( !Batches.TryGetValue( target, out var stream ) ) {
				stream = new System.IO.MemoryStream( 2048 );
				Batches.Add( target, stream );
			}

			stream.Write( BitConverter.GetBytes( data.Length ), 0, 4 );
			stream.Write( data, 0, data.Length );
		}
	}
	private void SendBatches() {
		lock ( BatchLock ) {
			foreach ( var batch in Batches ) {
				if ( batch.Value.Length > 0 ) {
					SendTargetPacket( batch.Key, batch.Value.ToArray(), Constants.k_nSteamNetworkingSend_Unreliable );
					batch.Value.SetLength( 0 );
				}
			}
		}
	}
	public void SendTargetPacket( CSteamID target, byte[] data, int sendType = Constants.k_nSteamNetworkingSend_Reliable ) {
		if ( Connections.TryGetValue( target, out HSteamNetConnection conn ) ) {
			byte[] buffer = Pool.Rent();

			try {
				byte[] secured = SteamLobbySecurity.SecureOutgoingMessage( data, target );
				int bytesToCopy = Math.Min( secured.Length, buffer.Length );
				Buffer.BlockCopy( secured, 0, buffer, 0, bytesToCopy );

				unsafe {
					fixed ( byte* pBuffer = buffer ) {
						EResult res = SteamNetworkingSockets.SendMessageToConnection(
							conn,
							(IntPtr)pBuffer,
							(uint)bytesToCopy,
							sendType,
							out long _
						);
						if ( res != EResult.k_EResultOK ) {
							Console.PrintError( $"[STEAM] Error sending message to {target}: {res}" );
						}
					}
				}
			}
			finally {
				Pool.Return( buffer );
			}
		}
	}

	public void SendP2PPacket( byte[] data, int nSendType = Constants.k_nSteamNetworkingSend_Reliable ) {
		lock ( ConnectionLock ) {
			foreach ( var pair in Connections ) {
				if ( pair.Key != ThisSteamID ) {
					SendTargetPacket( pair.Key, data, nSendType );
				}
			}
		}
	}

	private void ProcessIncomingMessage( byte[] data, CSteamID sender ) {
		byte[] processed = SteamLobbySecurity.ProcessIncomingMessage( data, sender );
		if ( processed == null ) {
			return;
		}
		byte[] queueBuffer = Pool.Rent();
		int copyLength = Math.Min( processed.Length, queueBuffer.Length );
		Buffer.BlockCopy( processed, 0, queueBuffer, 0, copyLength );

		MessageQueue.Enqueue( new IncomingMessage {
			Sender = sender,
			Data = queueBuffer,
			Type = (MessageType)queueBuffer[0]
		} );
		/*
		using ( var stream = new System.IO.MemoryStream( data ) ) {
			using ( var reader = new System.IO.BinaryReader( stream ) ) {
				MessageType type = (MessageType)reader.ReadByte();
				MessageQueue.Enqueue( new IncomingMessage {
					Sender = sender,
					Data = processed,
					Type = type
				} );
			}
		}
		*/
	}

	private void HandleIncomingMessages() {
		while ( MessageQueue.Count > 0 ) {
			var msg = MessageQueue.Dequeue();
			using ( var stream = new System.IO.MemoryStream( msg.Data ) )
			using ( var reader = new System.IO.BinaryReader( stream ) ) {
				reader.ReadByte(); // Skip type byte
				switch ( msg.Type ) {
				case MessageType.ClientData:
					if ( PlayerCache.TryGetValue( msg.Sender.ToString(), out NetworkNode node ) ) {
						node.Receive( reader );
					}
					break;
				case MessageType.GameData:
					int hash = reader.ReadInt32();
					if ( NodeCache.TryGetValue( hash, out NetworkNode gameNode ) ) {
						gameNode.Receive( reader );
					}
					break;
				case MessageType.Handshake:
					Console.PrintLine( $"{SteamFriends.GetFriendPersonaName( msg.Sender )} sent a handshake packet." );
					break;
				case MessageType.ServerCommand:
					ServerCommandType nCommandType = (ServerCommandType)reader.ReadUInt32();
					Console.PrintLine( $"Received server command: {nCommandType}" );
					ServerCommandManager.ExecuteCommand( msg.Sender, nCommandType );
					break;
				};
			}
		}
	}

	private void PollIncomingMessages() {
		byte[] receivedBuffer = Pool.Rent();

		try {
			IntPtr[] messages = new IntPtr[ PACKET_READ_LIMIT ];

			foreach ( var conn in Connections.Values.Concat( PendingConnections.Values ) ) {
				int count = SteamNetworkingSockets.ReceiveMessagesOnConnection( conn, messages, messages.Length );
				for ( int i = 0; i < count; i++ ) {
					try {
						SteamNetworkingMessage_t message = (SteamNetworkingMessage_t)Marshal.PtrToStructure(
							messages[ i ],
							typeof( SteamNetworkingMessage_t )
						);

						int bytesToCopy = Math.Min( message.m_cbSize, receivedBuffer.Length );
						Marshal.Copy( message.m_pData, receivedBuffer, 0, bytesToCopy );

						CSteamID sender = message.m_identityPeer.GetSteamID();
						ProcessIncomingMessage( receivedBuffer, sender );
					}
					finally {
						SteamNetworkingMessage_t.Release( messages[ i ] );
					}
				}
			}
		}
		finally {
			Pool.Return( receivedBuffer );
		}
		/*
		// Create copies to avoid locking during processing
		List<HSteamNetConnection> activeConnections = new List<HSteamNetConnection>();
		List<HSteamNetConnection> pendingConns = new List<HSteamNetConnection>();

		lock ( ConnectionLock ) {
			activeConnections.AddRange( Connections.Values );
			pendingConns.AddRange( PendingConnections.Values );
		}

		// Process active connections
		for ( int c = 0; c < activeConnections.Count; c++ ) {
			try {
				int count = SteamNetworkingSockets.ReceiveMessagesOnConnection( activeConnections[ c ], MessagePool, MessagePool.Length );
				//				if ( count > 0 ) Console.PrintLine( $"[STEAM] Received {count} messages" );

				for ( int i = 0; i < count; i++ ) {
					try {
						SteamNetworkingMessage_t message = (SteamNetworkingMessage_t)Marshal.PtrToStructure(
							MessagePool[ i ],
							typeof( SteamNetworkingMessage_t )
						);

						Marshal.Copy( message.m_pData, CachedPacket, 0, message.m_cbSize );

						CSteamID sender = message.m_identityPeer.GetSteamID();
						byte[] cleanData = SteamLobbySecurity.ProcessIncomingMessage( CachedPacket, sender );
						ProcessIncomingMessage( cleanData, sender );
					}
					finally {
						SteamNetworkingMessage_t.Release( MessagePool[ i ] );
					}
				}
			} catch ( Exception e ) {
				Console.PrintError( $"[STEAM] Error receiving messages: {e.Message}" );
			}
		}

		// Process pending connections
		for ( int c = 0; c < pendingConns.Count; c++ ) {
			try {
				int count = SteamNetworkingSockets.ReceiveMessagesOnConnection( pendingConns[ c ], MessagePool, MessagePool.Length );
				//			if ( count > 0 ) Console.PrintLine( $"[STEAM] Received {count} pending messages" );

				for ( int i = 0; i < count; i++ ) {
					try {
						SteamNetworkingMessage_t message = (SteamNetworkingMessage_t)Marshal.PtrToStructure(
							MessagePool[ i ],
							typeof( SteamNetworkingMessage_t )
						);

						Marshal.Copy( message.m_pData, CachedPacket, 0, message.m_cbSize );

						CSteamID sender = message.m_identityPeer.GetSteamID();
						ProcessIncomingMessage( CachedPacket, sender );
					}
					finally {
						SteamNetworkingMessage_t.Release( MessagePool[ i ] );
					}
				}
			} catch ( Exception e ) {
				Console.PrintError( $"[STEAM] Error receiving pending messages: {e.Message}" );
			}
		}

		// Run SteamNetworkingSockets callbacks
		SteamNetworkingSockets.RunCallbacks();
		*/
	}

	#region Steam Callbacks
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

		Console.PrintLine( $"Created lobby [{LobbyId}] Name: {LobbyName}, MaxMembers: {LobbyMaxMembers}, GameType: {GameConfiguration.GameMode}" );
		CreateListenSocket();
		EmitSignalLobbyCreated( (ulong)LobbyId );
	}

	private void OnLobbyMatchList( LobbyMatchList_t pCallback, bool bIOFailure ) {
		LobbyList.Clear();
		for ( int i = 0; i < pCallback.m_nLobbiesMatching; i++ ) {
			LobbyList.Add( SteamMatchmaking.GetLobbyByIndex( i ) );
		}
		EmitSignalLobbyListUpdated();
	}

	private void OnLobbyJoined( LobbyEnter_t pCallback ) {
		OnLobbyJoined( pCallback, false );
	}
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
			GameConfiguration.GameMode = GameMode.Online;
			break;
		case "Multiplayer":
			GameConfiguration.GameMode = GameMode.Multiplayer;
			LobbyMap = SteamMatchmaking.GetLobbyData( LobbyId, "map" );
			LobbyGameMode = Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );
			Console.PrintLine( $"Lobby map: {LobbyMap}" );
			break;
		};

		GetLobbyMembers();
		ConnectToLobbyMembers();
		EmitSignalLobbyJoined( (ulong)LobbyId );
	}

	private void OnLobbyChatMsg( LobbyChatMsg_t pCallback ) {
		byte[] szBuffer = new byte[ 1024 ];

		int nBufferLength = SteamMatchmaking.GetLobbyChatEntry(
			(CSteamID)pCallback.m_ulSteamIDLobby,
			(int)pCallback.m_iChatID,
			out CSteamID senderId,
			szBuffer,
			szBuffer.Length,
			out EChatEntryType entryType
		);

		if ( entryType == EChatEntryType.k_EChatEntryTypeChatMsg ) {
			string message = System.Text.Encoding.UTF8.GetString( szBuffer, 0, nBufferLength );
			EmitSignalChatMessageReceived( (ulong)senderId, message, (int)ChatMessageType.None );
		}
	}

	private void TakeOverHostResponsibilities() {
		CreateListenSocket();

		// TODO: gamestate changes?

		EmitSignalHostResponsibilitiesTransferred();
	}
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
		;
	}
	#endregion

	#region Utility Methods
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
		}
	}

	private void MakeP2PHandshake() {
		SendP2PPacket( [ (byte)MessageType.Handshake ] );
	}

	private void ChatUpdate( string status ) {
		Console.PrintLine( status );
	}

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
	#endregion

	// ==================== Public API ====================
	public NetworkPlayer GetPlayer( CSteamID userId ) {
		return PlayerCache[ userId.ToString() ].Node as NetworkPlayer;
	}

	public void AddPlayer( CSteamID userId, NetworkNode callbacks ) {
		Console.PrintLine( $"Added player with hash {userId} to network sync cache." );
		PlayerCache.TryAdd( userId.ToString(), callbacks );
		PlayerList.Add( callbacks );
	}

	public void RemovePlayer( CSteamID userId ) {
		if ( PlayerList.Contains( PlayerCache[ userId.ToString() ] ) ) {
			PlayerList.Remove( PlayerCache[ userId.ToString() ] );
		}
		PlayerCache.Remove( userId.ToString() );
	}

	public void AddNetworkNode( NodePath node, NetworkNode callbacks ) {
		Console.PrintLine( $"Added node with hash {node.GetHashCode()} to network sync cache." );
		NodeCache.TryAdd( node.GetHashCode(), callbacks );
	}

	public void RemoveNetworkNode( NodePath node ) {
		if ( NodeCache.ContainsKey( node.GetHashCode() ) ) {
			NodeCache.Remove( node.GetHashCode() );
		}
	}

	public Node GetNetworkNode( NodePath node ) {
		if ( NodeCache.TryGetValue( node.GetHashCode(), out NetworkNode value ) ) {
			return value.Node;
		}
		Console.PrintError( $"SteamLobby.GetNetworkNode: invalid network node {node.GetHashCode()}!" );
		return null;
	}

	public CSteamID GetHost() => LobbyOwnerId;
	public CSteamID GetLobbyID() => LobbyId;
	public bool IsOwner() => IsHost;
	public uint GetGameMode() => LobbyGameMode;
	public string GetMap() => SteamMatchmaking.GetLobbyData( LobbyId, "map" );
	public List<CSteamID> GetLobbyList() => LobbyList;

	public void SetLobbyName( string name ) => LobbyName = name;
	public void SetMaxMembers( int nMaxMembers ) => LobbyMaxMembers = nMaxMembers;
	public void SetGameMode( uint nGameMode ) => LobbyGameMode = nGameMode;
	public void SetMap( string mapname ) => LobbyMap = mapname;
	public void SetHostStatus( bool bHost ) => IsHost = bHost;

	// Initialize Steam networking
	private void InitializeSteamNetworking() {
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

	private void BroadcastVoteKickStart( VoteKick vote ) {
		ServerCommandManager.SendCommand( ServerCommandType.StartVoteKick );

		EmitSignalVoteKickStarted( (ulong)vote.Initiator, (ulong)vote.Target, vote.Reason );
	}

	private void CmdKickPlayer( string username ) {
		if ( !IsHost ) {
			return; // no permissions
		}
	}
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
	private void CmdNetworkingProfile() {
		GetTree().Root.GetNode<NetworkingMonitor>( "NetworkingMonitor" ).Visible = !GetTree().Root.GetNode<NetworkingMonitor>( "NetworkingMonitor" ).Visible;
	}

	public override void _EnterTree() {
		base._EnterTree();
		if ( _Instance != null ) {
			this.QueueFree();
		}
		_Instance = this;
	}
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

		CachedWritePacket = Marshal.AllocHGlobal( 2048 );

		// Initialize packet buffers
		CachedPacket = new byte[ 2048 ];
		SetPhysicsProcess( true );
		SetProcess( true );

		// Start lobby discovery
		OpenLobbyList();

		ServerCommandManager.RegisterCommandCallback( ServerCommandType.OwnershipChanged, new Action<CSteamID>( ( senderId ) => {
			LobbyOwnerId = SteamMatchmaking.GetLobbyOwner( LobbyId );
			IsHost = LobbyOwnerId == ThisSteamID;
		} ) );

		// Start network thread
		NetworkThread = new System.Threading.Thread( () => {
			while ( System.Threading.Interlocked.CompareExchange( ref NetworkRunning, 0, 0 ) == 0 ) {
				lock ( NetworkLock ) {
					try {
						PollIncomingMessages();
					} catch ( Exception e ) {
						Console.PrintError( $"[NETWORK THREAD] Error: {e.Message}" );
					}
				}
				System.Threading.Thread.Sleep( 20 );
			}
		} );
		//		NetworkThread.Start();

		// Add console command
		Console.AddCommand( "lobby_info", Callable.From( CmdLobbyInfo ), Array.Empty<string>(), 0, "prints lobby information." );
		Console.AddCommand( "network_prof", Callable.From( CmdNetworkingProfile ), Array.Empty<string>(), 0, "prints SteamNetworkingSockets performance information." );
		Console.AddCommand( "kick_player", Callable.From<string>( CmdKickPlayer ), [ "username" ], 1, "kicks a player." );
		Console.AddCommand( "transfer_ownership", Callable.From<string>( CmdTransferOwnership ), [ "username" ], 1, "transfers lobby ownership to specified player." );

		// Set local Steam ID
		ThisSteamID = SteamManager.GetSteamID();
		Console.PrintLine( $"[STEAM] Local Steam ID: {ThisSteamID}" );
	}
	public override void _Process( double delta ) {
		PollIncomingMessages();

		HandleIncomingMessages();

		// Send updates
		foreach ( var node in NodeCache.Values ) {
			node.Send?.Invoke();
		}
		foreach ( var player in PlayerCache.Values ) {
			player.Send?.Invoke();
		}
		SendBatches();
	}

	public override void _Notification( int what ) {
		base._Notification( what );

		if ( what == NotificationWMCloseRequest ) {
			System.Threading.Interlocked.Exchange( ref NetworkRunning, 1 );
			
			LeaveLobby();

			foreach ( var conn in Connections.Values ) {
				SteamNetworkingSockets.CloseConnection( conn, 0, "Shutdown", false );
			}

				NetworkThread?.Join( 1000 );
			Marshal.FreeHGlobal( CachedWritePacket );
		}
	}
};