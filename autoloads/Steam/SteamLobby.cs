/*
using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using Multiplayer;

public partial class SteamLobby : Node {
	public enum Visibility {
		Private,
		Public,
		FriendsOnly
	};
	
	public enum MessageType {
		Handshake,
		
		// server commands
		ServerCommand,

		// hot cache player data
		ClientData,

		// world data, node paths that will never change
		GameData,

		Count
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

	private static SteamLobby _Instance;
	public static SteamLobby Instance => _Instance;
	
	private const uint PACKET_READ_LIMIT = 32;
	public static readonly int MAX_LOBBY_MEMBERS = 16;

	private Chat ChatBar;
	
	private Callback<LobbyEnter_t> LobbyEnter;
	private Callback<LobbyChatMsg_t> LobbyChatMsg;
	private Callback<LobbyChatUpdate_t> LobbyChatUpdate;
	private Callback<P2PSessionRequest_t> P2PSessionRequest;
	private Callback<P2PSessionConnectFail_t> P2PSessionConnectFail;

	private CallResult<LobbyCreated_t> OnLobbyCreatedCallResult;
	private CallResult<LobbyEnter_t> OnLobbyEnterCallResult;
	private CallResult<LobbyMatchList_t> OnLobbyMatchListCallResult;
	
	public CSteamID[] LobbyMembers = new CSteamID[ MAX_LOBBY_MEMBERS ];
	public int LobbyMemberCount = 0;
	private CSteamID LobbyId = CSteamID.Nil;
	private CSteamID LobbyOwnerId = CSteamID.Nil;
	private bool IsHost = false;

	private HServerListRequest ServerListRequest;
	private ISteamMatchmakingServerListResponse ServerListResponse;
	private ISteamMatchmakingPingResponse PingResponse;
	private ISteamMatchmakingPlayersResponse PlayersResponse;
	private ISteamMatchmakingRulesResponse RulesResponse;

	private List<CSteamID> LobbyList = new List<CSteamID>();
	private int LobbyMaxMembers = 0;
	private uint LobbyGameMode = 0;
	private string LobbyName;
	private string LobbyMap;
	private Visibility LobbyVisibility = Visibility.Public;

	private string LobbyFilterMap;
	private string LobbyFilterGameMode;

	private byte[] CachedPacket = null;
	private System.IO.MemoryStream PacketStream = null;
	private System.IO.BinaryReader PacketReader = null;
	
	private CSteamID ThisSteamID = CSteamID.Nil;
	private int ThisSteamIDIndex = 0;

	private System.Threading.Thread NetworkThread;
	private int NetworkRunning = 0;
	private readonly object NetworkLock = new object();

	private Dictionary<int, NetworkNode> NodeCache = new Dictionary<int, NetworkNode>();
	private Dictionary<string, NetworkNode> PlayerCache = new Dictionary<string, NetworkNode>();
	private List<NetworkNode> PlayerList = new List<NetworkNode>( MAX_LOBBY_MEMBERS );

	[Signal]
	public delegate void ChatMessageReceivedEventHandler( ulong senderSteamId, string message );
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

	public NetworkPlayer GetPlayer( CSteamID userId ) {
		return PlayerCache[ userId.ToString() ].Node as NetworkPlayer;
	}
	public void AddPlayer( CSteamID userId, NetworkNode callbacks ) {
		Console.PrintLine( "Added player with hash " + userId.ToString() + " to network sync cache." );
		PlayerCache.TryAdd( userId.ToString(), callbacks );
		PlayerList.Add( callbacks );
	}
	public void RemovePlayer( CSteamID userId ) {
		PlayerList.Remove( PlayerCache[ userId.ToString() ] );
		PlayerCache.Remove( userId.ToString() );
	}
	public void AddNetworkNode( NodePath node, NetworkNode callbacks ) {
		Console.PrintLine( "Added node with hash " + node.GetHashCode() + " to network sync cache." );
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
		Console.PrintError( string.Format( "SteamLobby.GetNetworkNode: invalid network node {0}!", node.GetHashCode() ) );
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

	private void ChatUpdate( string status ) {
		Console.PrintLine( "status" );
	}
	private void OnLobbyChatUpdate( LobbyChatUpdate_t pCallback ) {
		string changerName = SteamFriends.GetFriendPersonaName( (CSteamID)pCallback.m_ulSteamIDMakingChange );

		switch ( (EChatMemberStateChange)pCallback.m_rgfChatMemberStateChange ) {
		case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
			ChatUpdate( string.Format( "{0} has joined...", changerName ) );
			EmitSignal( "ClientJoinedLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
			ChatUpdate( string.Format( "{0} has faded away...", changerName ) );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
			ChatUpdate( string.Format( "{0} tweaked out so hard they left the fever dream...", changerName ) );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
			ChatUpdate( string.Format( "{0} was rejected...", changerName ) );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
			ChatUpdate( string.Format( "{0} was excommunicated...", changerName ) );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		};
	}
	private void OnLobbyCreated( LobbyCreated_t pCallback, bool bIOFailure ) {
		if ( pCallback.m_eResult != EResult.k_EResultOK ) {
			Console.PrintLine( string.Format( "[STEAM] Error creating lobby: {0}", pCallback.m_eResult.ToString() ) );
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
		
		Console.PrintLine(
			string.Format( "Created lobby [{0}] Name: {1}, MaxMembers: {2}, GameType: {3}",
				LobbyId.ToString(), LobbyName, LobbyMaxMembers, GameConfiguration.GameMode.ToString()
			)
		);

		if ( !SteamNetworking.AllowP2PPacketRelay( true ) ) {
			Console.PrintError( "[STEAM] couldn't enable p2p packet relay!" );
			return;
		}
	}
	private void OnLobbyMatchList( LobbyMatchList_t pCallback, bool bIOFailure ) {
		LobbyList.Clear();
		for ( int i = 0; i < pCallback.m_nLobbiesMatching; i++ ) {
			LobbyList.Add( SteamMatchmaking.GetLobbyByIndex( i ) );
		}

		EmitSignal( "LobbyListUpdated" );
	}
	private void MakeP2PHandkshake() {
		SendP2PPacket( [ (byte)MessageType.Handshake ] );
	}
	public void GetLobbyMembers() {
		LobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers( LobbyId );
		for ( int i = 0; i < LobbyMembers.Length; i++ ) {
			LobbyMembers[i] = CSteamID.Nil;
		}
		for ( int i = 0; i < LobbyMemberCount; i++ ) {
			LobbyMembers[i] = SteamMatchmaking.GetLobbyMemberByIndex( LobbyId, i );
			if ( LobbyMembers[i] == ThisSteamID ) {
				ThisSteamIDIndex = i;
			}
		}
	}

	public void OpenLobbyList() {
		SteamMatchmaking.AddRequestLobbyListDistanceFilter( ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide );

		MatchMakingKeyValuePair_t[] filters = [];
		ServerListRequest = SteamMatchmakingServers.RequestInternetServerList( SteamManager.GetAppID(), filters, 0, ServerListResponse );

		SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
		OnLobbyMatchListCallResult.Set( handle );
	}

	public void CreateLobby() {
		if ( LobbyId.IsValid() ) {
			Console.PrintError( string.Format( "SteamLobby.CreateLobby: LobbyId {0} isn't valid!", LobbyId.ToString() ) );
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

		Console.PrintLine( "Leaving lobby " + LobbyId + "..." );
		SteamMatchmaking.LeaveLobby( LobbyId );
		LobbyId = CSteamID.Nil;

		for ( int i = 0; i < LobbyMemberCount; i++ ) {
			P2PSessionState_t sessionState;
			if ( !SteamNetworking.GetP2PSessionState( LobbyMembers[i], out sessionState ) ) {
				continue;
			}
			if ( sessionState.m_bConnectionActive == 1 ) {
				SteamNetworking.CloseP2PSessionWithUser( LobbyMembers[i] );
			}
		}
		for ( int i = 0; i < LobbyMembers.Length; i++ ) {
			LobbyMembers[i] = CSteamID.Nil;
		}

		NodeCache.Clear();
		PlayerCache.Clear();

		EmitSignal( "ClientLeftLobby", (ulong)SteamUser.GetSteamID() );

		GetTree().Root.RemoveChild( ChatBar );
		ChatBar.QueueFree();
	}

	public void SendTargetPacket( CSteamID target, byte[] data ) {
		int channel = 0;
		
		SteamNetworking.SendP2PPacket( target, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable, channel );
	}
	public void SendP2PPacket( byte[] data ) {
		int channel = 0;
		
		for ( int i = 0; i < LobbyMemberCount; i++ ) {
			if ( i != ThisSteamIDIndex ) {
				SteamNetworking.SendP2PPacket( LobbyMembers[ i ], data, (uint)data.Length,
					EP2PSend.k_EP2PSendReliable, channel
				);
			}
		}
	}

	private void ReadP2Packet( uint packetSize ) {
		SteamNetworking.ReadP2PPacket( CachedPacket, packetSize, out packetSize, out CSteamID senderId );
		PacketStream.Seek( 0, System.IO.SeekOrigin.Begin );

		switch ( (MessageType)PacketReader.ReadByte() ) {
		case MessageType.ClientData: {
			if ( PlayerCache.TryGetValue( senderId.ToString(), out NetworkNode node ) ) {
				node.Receive( PacketReader );
			}
			break; }
		case MessageType.GameData:
			NodeCache[ PacketReader.ReadInt32() ].Receive( PacketReader );
			break;
		case MessageType.Handshake:
			Console.PrintLine( SteamFriends.GetFriendPersonaName( senderId ) + " sent a handshake packet." );
			break;
		case MessageType.ServerCommand: {
			ServerCommandType nCommandType = (ServerCommandType)PacketReader.ReadUInt32();
			Console.PrintLine( string.Format( "Received server command: {0}", nCommandType.ToString() ) );
			ServerCommandManager.ExecuteCommand( senderId, nCommandType );
			break; }
		};
	}
	private void ReadPackets( uint readCount = 0 ) {
		if ( readCount >= PACKET_READ_LIMIT ) {
			return;
		}

		if ( SteamNetworking.IsP2PPacketAvailable( out uint packetSize ) ) {
			ReadP2Packet( packetSize );
			ReadPackets( readCount + 1 );
		}
	}
	public void ReadAllPackets() {
		ReadPackets();
	}
	
	private void OnLobbyJoined( LobbyEnter_t pCallback, bool bIOFailure ) {
		Console.PrintLine( "...Joined" );
		if ( pCallback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess ) {
			Console.PrintError(
				string.Format( "[STEAM] Error joining lobby {0}: {1}",
					pCallback.m_ulSteamIDLobby,
					( (EChatRoomEnterResponse)pCallback.m_EChatRoomEnterResponse ).ToString()
				)
			);
			return;
		}

		LobbyId = (CSteamID)pCallback.m_ulSteamIDLobby;
		LobbyOwnerId = SteamMatchmaking.GetLobbyOwner( LobbyId );

		Console.PrintLine( string.Format( "Joined lobby {0}.", pCallback.m_ulSteamIDLobby ) );

		LobbyName = SteamMatchmaking.GetLobbyData( LobbyId, "name" );
		switch ( SteamMatchmaking.GetLobbyData( LobbyId, "gametype" ) ) {
		case "Online":
			GameConfiguration.GameMode = GameMode.Online;
			break;
		case "Multiplayer":
			GameConfiguration.GameMode = GameMode.Multiplayer;
			LobbyMap = SteamMatchmaking.GetLobbyData( LobbyId, "map" );
			LobbyGameMode = Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );
			Console.PrintLine( string.Format( "Lobby map: {0}", LobbyMap ) );
			break;
		};

		// more for debugging...
		Console.PrintLine( "Sending p2p handshake..." );
		
		GetLobbyMembers();
		MakeP2PHandkshake();
		
		LobbyBrowser.Instance.OnLobbyJoined( (ulong)LobbyId );

		ChatBar = ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/chat_bar.tscn" ).Instantiate<Chat>();
		GetTree().Root.AddChild( ChatBar );
	}

	private void OnLobbyJoined( LobbyEnter_t pCallback ) {
		if ( pCallback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess ) {
			Console.PrintError(
				string.Format( "[STEAM] Error joining lobby {0}: {1}", pCallback.m_ulSteamIDLobby,
					( (EChatRoomEnterResponse)pCallback.m_EChatRoomEnterResponse ).ToString()
				)
			);
			return;
		}

		LobbyId = (CSteamID)pCallback.m_ulSteamIDLobby;
		LobbyOwnerId = SteamMatchmaking.GetLobbyOwner( LobbyId );

		Console.PrintLine( string.Format( "...Joined lobby {0}.", pCallback.m_ulSteamIDLobby ) );

		LobbyName = SteamMatchmaking.GetLobbyData( LobbyId, "name" );
		switch ( SteamMatchmaking.GetLobbyData( LobbyId, "gametype" ) ) {
		case "Online":
			GameConfiguration.GameMode = GameMode.Online;
			break;
		case "Multiplayer":
			GameConfiguration.GameMode = GameMode.Multiplayer;
			LobbyMap = SteamMatchmaking.GetLobbyData( LobbyId, "map" );
			LobbyGameMode = Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );
			Console.PrintLine( string.Format( "Lobby map: {0}", LobbyMap ) );
			break;
		};

		Console.PrintLine( "Sending p2p handshake..." );

		GetLobbyMembers();
		MakeP2PHandkshake();

		LobbyBrowser.Instance.OnLobbyJoined( (ulong)LobbyId );

		ChatBar = ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/chat_bar.tscn" ).Instantiate<Chat>();
		GetTree().Root.AddChild( ChatBar );
	}

	private void OnP2PSessionRequest( P2PSessionRequest_t pCallback ) {
		string requester = SteamFriends.GetFriendPersonaName( pCallback.m_steamIDRemote );
		Console.PrintLine( "[STEAM] " + requester + " sent a P2P session request" );
		SteamNetworking.AcceptP2PSessionWithUser( pCallback.m_steamIDRemote );
	}
	private void OnLobbyDataUpdated( long success, ulong lobbyId, ulong memberId ) {
	}
	private void OnLobbyMessage( LobbyChatMsg_t pCallback ) {
		byte[] szBuffer = new byte[ 1024 ];

		CSteamID senderId;
		EChatEntryType entryType;
		int ret = SteamMatchmaking.GetLobbyChatEntry( (CSteamID)pCallback.m_ulSteamIDLobby,
			(int)pCallback.m_iChatID, out senderId, szBuffer, szBuffer.Length, out entryType );

		switch ( entryType ) {
		case EChatEntryType.k_EChatEntryTypeChatMsg: {
			CallDeferred( "emit_signal", "ChatMessageReceived", (ulong)senderId, szBuffer.ToString() );
			break; }
		};
	}

	private void OnRefreshComplete( HServerListRequest hRequest, EMatchMakingServerResponse response ) {
		Console.PrintLine( string.Format( "[STEAM] Server list refresh finished: {0}", response.ToString() ) );
	}
	private void OnServerResponded( HServerListRequest hRequest, int iServer ) {
	}
	private void OnServerFailedToRespond( HServerListRequest hRequest, int iServer ) {
		Console.PrintError( "[STEAM] Server failed to respond" );
	}

	private void OnPingResponse( gameserveritem_t hRequest ) {
	}
	private void OnPingFailedToRespond() {
	}

	public float GetPing() {
		SteamNetworkPingLocation_t hPingLocation = new SteamNetworkPingLocation_t();
		return SteamNetworkingUtils.GetLocalPingLocation( out hPingLocation );
	}

	private void CmdLobbyInfo() {
		Console.PrintLine( "[STEAM LOBBY METADATA]" );
		Console.PrintLine( string.Format( "Name: {0}", LobbyName ) );
		Console.PrintLine( string.Format( "MaxMembers: {0}", LobbyMaxMembers ) );
		Console.PrintLine( string.Format( "MemberCount: {0}", LobbyMemberCount ) );
		Console.PrintLine( string.Format( "LobbyId: {0}", LobbyId.ToString() ) );
		Console.PrintLine( string.Format( "GameMode: {0}", LobbyGameMode ) );
		Console.PrintLine( string.Format( "Map: {0}", LobbyMap ) );
		Console.PrintLine( string.Format( "IsValid: {0}", LobbyId.IsValid() ) );
		Console.PrintLine( string.Format( "IsLobby: {0}", LobbyId.IsLobby() ) );
	}

	public override void _EnterTree() {
		base._EnterTree();
		if ( _Instance != null ) {
			this.QueueFree();
		}
		_Instance = this;
	}

	public override void _Ready() {
		LobbyEnter = Callback<LobbyEnter_t>.Create( OnLobbyJoined );
		LobbyChatMsg = Callback<LobbyChatMsg_t>.Create( OnLobbyMessage );
		LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create( OnLobbyChatUpdate );
		P2PSessionRequest = Callback<P2PSessionRequest_t>.Create( OnP2PSessionRequest );

		OnLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create( OnLobbyCreated );
		OnLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create( OnLobbyJoined );
		OnLobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create( OnLobbyMatchList );

		ServerListResponse = new ISteamMatchmakingServerListResponse( OnServerResponded, OnServerFailedToRespond, OnRefreshComplete );
		PingResponse = new ISteamMatchmakingPingResponse( OnPingResponse, OnPingFailedToRespond );

		CachedPacket = new byte[ 2048 ];
		PacketStream = new System.IO.MemoryStream( CachedPacket );
		PacketReader = new System.IO.BinaryReader( PacketStream );
		SetPhysicsProcess( true );
		SetProcess( true );

		OpenLobbyList();

		NetworkThread = new System.Threading.Thread( () => {
			while ( !System.Threading.Interlocked.Equals( NetworkRunning, 1 ) ) {
				if ( !IsPhysicsProcessing() ) {
					continue;
				}
				System.Threading.Thread.Sleep( 150 );

				lock ( NetworkLock ) {
					foreach ( var node in NodeCache ) {
						node.Value.Send?.Invoke();
					}
					foreach ( var player in PlayerCache ) {
						player.Value.Send?.Invoke();
					}
					
					ReadAllPackets();
				}
			}
		} );
		NetworkThread.Start();

		Console.AddCommand( "lobby_info", Callable.From( CmdLobbyInfo ), Array.Empty<string>(), 0, "prints lobby information." );

		ThisSteamID = SteamManager.GetSteamID();
	}
	public override void _Notification( int what ) {
		base._Notification( what );

		if ( what == NotificationWMCloseRequest ) {
			System.Threading.Interlocked.Increment( ref NetworkRunning );
			NetworkThread.Join();
		}
	}

#if 0
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 15 ) != 0 ) {
			return;
		}
		foreach ( var node in NodeCache ) {
			node.Value.Send?.Invoke();
		}
		foreach ( var player in PlayerCache ) {
			player.Value.Send?.Invoke();
		}
	}
	public override void _PhysicsProcess( double delta ) {
		ReadAllPackets();
	}
#endif
};
*/

/*
using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Multiplayer;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

public partial class SteamLobby : Node {
	public enum Visibility {
		Private,
		Public,
		FriendsOnly
	};

	public enum MessageType {
		Handshake,

		// server commands
		ServerCommand,

		// hot cache player data
		ClientData,

		// world data, node paths that will never change
		GameData,

		Count
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
	private HSteamListenSocket ListenSocket;

	private List<CSteamID> LobbyList = new List<CSteamID>();
	private int LobbyMaxMembers = 0;
	private uint LobbyGameMode = 0;
	private string LobbyName;
	private string LobbyMap;
	private Visibility LobbyVisibility = Visibility.Public;

	private string LobbyFilterMap;
	private string LobbyFilterGameMode;

	private byte[] CachedPacket = null;
	private System.IO.MemoryStream PacketStream = null;
	private System.IO.BinaryReader PacketReader = null;

	private CSteamID ThisSteamID = CSteamID.Nil;
	private int ThisSteamIDIndex = 0;

	private System.Threading.Thread NetworkThread;
	private int NetworkRunning = 0;
	private readonly object NetworkLock = new object();

	private Dictionary<int, NetworkNode> NodeCache = new Dictionary<int, NetworkNode>();
	private Dictionary<string, NetworkNode> PlayerCache = new Dictionary<string, NetworkNode>();
	private List<NetworkNode> PlayerList = new List<NetworkNode>( MAX_LOBBY_MEMBERS );

	[Signal]
	public delegate void ChatMessageReceivedEventHandler( ulong senderSteamId, string message );
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

	// Message processing
	private struct IncomingMessage {
		public CSteamID Sender;
		public byte[] Data;
		public MessageType Type;
	}
	private Queue<IncomingMessage> MessageQueue = new Queue<IncomingMessage>();

	public NetworkPlayer GetPlayer( CSteamID userId ) {
		return PlayerCache[ userId.ToString() ].Node as NetworkPlayer;
	}
	public void AddPlayer( CSteamID userId, NetworkNode callbacks ) {
		Console.PrintLine( "Added player with hash " + userId.ToString() + " to network sync cache." );
		PlayerCache.TryAdd( userId.ToString(), callbacks );
		PlayerList.Add( callbacks );
	}
	public void RemovePlayer( CSteamID userId ) {
		PlayerList.Remove( PlayerCache[ userId.ToString() ] );
		PlayerCache.Remove( userId.ToString() );
	}
	public void AddNetworkNode( NodePath node, NetworkNode callbacks ) {
		Console.PrintLine( "Added node with hash " + node.GetHashCode() + " to network sync cache." );
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
		Console.PrintError( string.Format( "SteamLobby.GetNetworkNode: invalid network node {0}!", node.GetHashCode() ) );
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

	private void ChatUpdate( string status ) {
		Console.PrintLine( "status" );
	}
	private void OnLobbyChatUpdate( LobbyChatUpdate_t pCallback ) {
		string changerName = SteamFriends.GetFriendPersonaName( (CSteamID)pCallback.m_ulSteamIDMakingChange );

		switch ( (EChatMemberStateChange)pCallback.m_rgfChatMemberStateChange ) {
		case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
			ChatUpdate( string.Format( "{0} has joined...", changerName ) );
			EmitSignal( "ClientJoinedLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
			ChatUpdate( string.Format( "{0} has faded away...", changerName ) );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
			ChatUpdate( string.Format( "{0} tweaked out so hard they left the fever dream...", changerName ) );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
			ChatUpdate( string.Format( "{0} was rejected...", changerName ) );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
			ChatUpdate( string.Format( "{0} was excommunicated...", changerName ) );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		}
		;
	}
	private void OnLobbyCreated( LobbyCreated_t pCallback, bool bIOFailure ) {
		if ( pCallback.m_eResult != EResult.k_EResultOK ) {
			Console.PrintLine( string.Format( "[STEAM] Error creating lobby: {0}", pCallback.m_eResult.ToString() ) );
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

		Console.PrintLine(
			string.Format( "Created lobby [{0}] Name: {1}, MaxMembers: {2}, GameType: {3}",
				LobbyId.ToString(), LobbyName, LobbyMaxMembers, GameConfiguration.GameMode.ToString()
			)
		);

		CreateListenSocket();
		Console.PrintLine( "Created lobby socket" );
	}
	private void OnLobbyMatchList( LobbyMatchList_t pCallback, bool bIOFailure ) {
		LobbyList.Clear();
		for ( int i = 0; i < pCallback.m_nLobbiesMatching; i++ ) {
			LobbyList.Add( SteamMatchmaking.GetLobbyByIndex( i ) );
		}

		EmitSignal( "LobbyListUpdated" );
	}
	private void MakeP2PHandshake() {
		byte[] data = new byte[ 1 ] { (byte)MessageType.Handshake };
		SendP2PPacket( data );
	}
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

	public void OpenLobbyList() {
		SteamMatchmaking.AddRequestLobbyListDistanceFilter( ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide );

		MatchMakingKeyValuePair_t[] filters = [];
		// ServerListRequest = SteamMatchmakingServers.RequestInternetServerList(SteamManager.GetAppID(), filters, 0, ServerListResponse);

		SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
		OnLobbyMatchListCallResult.Set( handle );
	}

	public void CreateLobby() {
		if ( LobbyId.IsValid() ) {
			Console.PrintError( string.Format( "SteamLobby.CreateLobby: LobbyId {0} isn't valid!", LobbyId.ToString() ) );
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
		;

		Console.PrintLine( "Initializing SteamLobby..." );
		SteamAPICall_t handle = SteamMatchmaking.CreateLobby( lobbyType, LobbyMaxMembers );
		OnLobbyCreatedCallResult.Set( handle );
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

		Console.PrintLine( "Leaving lobby " + LobbyId + "..." );
		SteamMatchmaking.LeaveLobby( LobbyId );
		LobbyId = CSteamID.Nil;

		// Close all connections
		foreach ( var conn in Connections.Values ) {
			SteamNetworkingSockets.CloseConnection( conn, 0, "Leaving lobby", true );
		}
		Connections.Clear();
		ConnectionToSteamID.Clear();
		PendingConnections.Clear();
		ListenSocket = HSteamListenSocket.Invalid;

		for ( int i = 0; i < LobbyMembers.Length; i++ ) {
			LobbyMembers[ i ] = CSteamID.Nil;
		}

		NodeCache.Clear();
		PlayerCache.Clear();

		EmitSignal( "ClientLeftLobby", (ulong)SteamUser.GetSteamID() );

		GetTree().Root.RemoveChild( ChatBar );
		ChatBar.QueueFree();
	}

	public void SendTargetPacket( CSteamID target, byte[] data ) {
		if ( Connections.TryGetValue( target, out HSteamNetConnection conn ) ) {
			IntPtr ptr = Marshal.AllocHGlobal( data.Length );
			Marshal.Copy( data, 0, ptr, data.Length );
			EResult res = SteamNetworkingSockets.SendMessageToConnection(
				conn,
				ptr,
				(uint)data.Length,
				Constants.k_nSteamNetworkingSend_Reliable,
				out long _
			);
			Marshal.FreeHGlobal( ptr );

			if ( res != EResult.k_EResultOK ) {
				Console.PrintError( $"[STEAM] Error sending message to {target}: {res}" );
			}
		}
	}

	public void SendP2PPacket( byte[] data ) {
		foreach ( var pair in Connections ) {
			if ( pair.Key != ThisSteamID ) {
				SendTargetPacket( pair.Key, data );
			}
		}
	}

	private void ProcessIncomingMessage( byte[] data, CSteamID sender ) {
		using ( var stream = new System.IO.MemoryStream( data ) ) {
			using ( var reader = new System.IO.BinaryReader( stream ) ) {
				MessageType type = (MessageType)reader.ReadByte();
				MessageQueue.Enqueue( new IncomingMessage {
					Sender = sender,
					Data = data,
					Type = type
				} );
			}
		}
	}

	private void HandleIncomingMessages() {
		while ( MessageQueue.Count > 0 ) {
			var msg = MessageQueue.Dequeue();
			using ( var stream = new System.IO.MemoryStream( msg.Data ) ) {
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
						Console.PrintLine( SteamFriends.GetFriendPersonaName( msg.Sender ) + " sent a handshake packet." );
						break;
					case MessageType.ServerCommand:
						ServerCommandType nCommandType = (ServerCommandType)reader.ReadUInt32();
						Console.PrintLine( $"Received server command: {nCommandType}" );
						ServerCommandManager.ExecuteCommand( msg.Sender, nCommandType );
						break;
					}
				}
			}
		}
	}

	private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t status) {
    Console.PrintLine($"[STEAM CONNECTION] Status changed: {status.m_info.m_eState}");
    
    try {
        CSteamID remoteId = status.m_info.m_identityRemote.GetSteamID();
        string remoteName = SteamFriends.GetFriendPersonaName(remoteId);
        Console.PrintLine($"[STEAM CONNECTION] Remote: {remoteName} ({remoteId})");

        switch (status.m_info.m_eState) {
            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
                Console.PrintLine($"[STEAM] Connection request from {remoteName}");
                
                // Only accept if this is an incoming connection request
                if (!PendingConnections.ContainsKey(remoteId)) {
                    if (SteamNetworkingSockets.AcceptConnection(status.m_hConn) == EResult.k_EResultOK) {
                        Console.PrintLine("[STEAM] Accepted incoming connection");
                    } else {
                        Console.PrintError("[STEAM] Failed to accept incoming connection");
                        SteamNetworkingSockets.CloseConnection(status.m_hConn, 0, "Connection accept failed", false);
                    }
                } else {
                    Console.PrintLine("[STEAM] Ignoring outgoing connection request (we initiated it)");
                }
                break;

            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
                Console.PrintLine($"[STEAM] Finding route to {remoteName}");
                break;

            case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
                Console.PrintLine($"[STEAM] Connected to {remoteName}");
                
                lock (NetworkLock) {
                    Connections[remoteId] = status.m_hConn;
                    ConnectionToSteamID[status.m_hConn] = remoteId;
                    PendingConnections.Remove(remoteId);
                }
                
                // Send handshake to new connection
                byte[] handshake = new byte[1] { (byte)MessageType.Handshake };
                SendTargetPacket(remoteId, handshake);
                break;

            // ... rest of the cases ...
        }
    }
    catch (Exception e) {
        Console.PrintError($"[STEAM] Error in connection callback: {e.Message}");
    }
}

private void ConnectToLobbyMembers() {
    for (int i = 0; i < LobbyMemberCount; i++) {
        if (LobbyMembers[i] == ThisSteamID) continue;

        SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
        identity.SetSteamID(LobbyMembers[i]);

        SteamNetworkingConfigValue_t[] options = new SteamNetworkingConfigValue_t[] {
            new SteamNetworkingConfigValue_t {
                m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial,
                m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 10000 } // 10 seconds
            },
            new SteamNetworkingConfigValue_t {
                m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SymmetricConnect,
                m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 1 } // Enable symmetric connections
            }
        };

        HSteamNetConnection conn = SteamNetworkingSockets.ConnectP2P(ref identity, 0, options.Length, options);
        
        if (conn != HSteamNetConnection.Invalid) {
            lock (NetworkLock) {
                PendingConnections[LobbyMembers[i]] = conn;
            }
            Console.PrintLine($"[STEAM] Connecting to {SteamFriends.GetFriendPersonaName(LobbyMembers[i])}");
        } else {
            Console.PrintError($"[STEAM] Failed to create connection to {SteamFriends.GetFriendPersonaName(LobbyMembers[i])}");
        }
    }
}

	private void CreateListenSocket() {
		if ( ListenSocket != HSteamListenSocket.Invalid ) {
			SteamNetworkingSockets.CloseListenSocket( ListenSocket );
		}

		SteamNetworkingConfigValue_t[] options = new SteamNetworkingConfigValue_t[] {
			new SteamNetworkingConfigValue_t {
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 1 }
			},
			new SteamNetworkingConfigValue_t {
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IPLocalHost_AllowWithoutAuth,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue { m_int32 = 1 }
			}
		};
		ListenSocket = SteamNetworkingSockets.CreateListenSocketP2P( 0, options.Length, options );
		if ( ListenSocket == HSteamListenSocket.Invalid ) {
			Console.PrintError( "[STEAM] Failed to create listen socket" );
		} else {
			Console.PrintLine( "[STEAM] Created listen socket" );
		}
	}

	private void PollIncomingMessages() {
		IntPtr[] messages = new IntPtr[ 32 ];

		foreach ( var conn in Connections.Values ) {
			int count = SteamNetworkingSockets.ReceiveMessagesOnConnection( conn, messages, messages.Length );

			for ( int i = 0; i < count; i++ ) {
				SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>( messages[ i ] );
				byte[] data = new byte[ message.m_cbSize ];
				Marshal.Copy( message.m_pData, data, 0, (int)message.m_cbSize );

				CSteamID sender = message.m_identityPeer.GetSteamID();
				ProcessIncomingMessage( data, sender );

				// Release message
				Marshal.FreeHGlobal( message.m_pData );
				Marshal.FreeHGlobal( messages[ i ] );
			}
		}
	}

	private void OnLobbyJoined( LobbyEnter_t pCallback, bool bIOFailure ) {
		Console.PrintLine( "...Joined" );
		if ( pCallback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess ) {
			Console.PrintError(
				string.Format( "[STEAM] Error joining lobby {0}: {1}",
					pCallback.m_ulSteamIDLobby,
					( (EChatRoomEnterResponse)pCallback.m_EChatRoomEnterResponse ).ToString()
				)
			);
			return;
		}

		LobbyId = (CSteamID)pCallback.m_ulSteamIDLobby;
		LobbyOwnerId = SteamMatchmaking.GetLobbyOwner( LobbyId );

		Console.PrintLine( string.Format( "Joined lobby {0}.", pCallback.m_ulSteamIDLobby ) );

		LobbyName = SteamMatchmaking.GetLobbyData( LobbyId, "name" );
		switch ( SteamMatchmaking.GetLobbyData( LobbyId, "gametype" ) ) {
		case "Online":
			GameConfiguration.GameMode = GameMode.Online;
			break;
		case "Multiplayer":
			GameConfiguration.GameMode = GameMode.Multiplayer;
			LobbyMap = SteamMatchmaking.GetLobbyData( LobbyId, "map" );
			LobbyGameMode = Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );
			Console.PrintLine( string.Format( "Lobby map: {0}", LobbyMap ) );
			break;
		}
		;

		GetLobbyMembers();
		CreateListenSocket();
		ConnectToLobbyMembers();
		MakeP2PHandshake();

		EmitSignal( "LobbyJoined", (ulong)LobbyId );

		ChatBar = ResourceLoader.Load<PackedScene>( "res://scenes/multiplayer/chat_bar.tscn" ).Instantiate<Chat>();
		GetTree().Root.AddChild( ChatBar );
	}
	private void OnLobbyJoined( LobbyEnter_t pCallback ) {
		OnLobbyJoined( pCallback, false );
	}

	private void OnLobbyChatMsg( LobbyChatMsg_t pCallback ) {
		byte[] szBuffer = new byte[ 1024 ];

		CSteamID senderId;
		EChatEntryType entryType;
		int ret = SteamMatchmaking.GetLobbyChatEntry(
			(CSteamID)pCallback.m_ulSteamIDLobby,
			(int)pCallback.m_iChatID,
			out senderId,
			szBuffer,
			szBuffer.Length,
			out entryType
		);

		switch ( entryType ) {
		case EChatEntryType.k_EChatEntryTypeChatMsg: {
				string message = System.Text.Encoding.UTF8.GetString( szBuffer, 0, ret );
				EmitSignal( "ChatMessageReceived", (ulong)senderId, message );
				break;
			}
		}
		;
	}

	public float GetPing() {
		SteamNetworkPingLocation_t hPingLocation;
		return SteamNetworkingUtils.GetLocalPingLocation( out hPingLocation );
	}

	private void CmdLobbyInfo() {
		Console.PrintLine( "[STEAM LOBBY METADATA]" );
		Console.PrintLine( string.Format( "Name: {0}", LobbyName ) );
		Console.PrintLine( string.Format( "MaxMembers: {0}", LobbyMaxMembers ) );
		Console.PrintLine( string.Format( "MemberCount: {0}", LobbyMemberCount ) );
		Console.PrintLine( string.Format( "LobbyId: {0}", LobbyId.ToString() ) );
		Console.PrintLine( string.Format( "GameMode: {0}", LobbyGameMode ) );
		Console.PrintLine( string.Format( "Map: {0}", LobbyMap ) );
		Console.PrintLine( string.Format( "IsValid: {0}", LobbyId.IsValid() ) );
		Console.PrintLine( string.Format( "IsLobby: {0}", LobbyId.IsLobby() ) );
	}

	public override void _EnterTree() {
		base._EnterTree();
		if ( _Instance != null ) {
			this.QueueFree();
		}
		_Instance = this;
	}

	public override void _Ready() {
		LobbyEnter = Callback<LobbyEnter_t>.Create( OnLobbyJoined );
		LobbyChatMsg = Callback<LobbyChatMsg_t>.Create( OnLobbyChatMsg );
		LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create( OnLobbyChatUpdate );
		ConnectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create( OnConnectionStatusChanged );

		OnLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create( OnLobbyCreated );
		OnLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create( OnLobbyJoined );
		OnLobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create( OnLobbyMatchList );

		CachedPacket = new byte[ 2048 ];
		PacketStream = new System.IO.MemoryStream( CachedPacket );
		PacketReader = new System.IO.BinaryReader( PacketStream );
		SetPhysicsProcess( true );
		SetProcess( true );

		OpenLobbyList();

		SteamNetworkingUtils.InitRelayNetworkAccess();

		ESteamNetworkingAvailability status = SteamNetworkingUtils.GetRelayNetworkStatus( out SteamRelayNetworkStatus_t relayStatus );
		Console.PrintLine( string.Format( "[STEAM] Relay network status: {0}", relayStatus.ToString() ) );

		SteamNetworkingIdentity localIdentity = new SteamNetworkingIdentity();
		localIdentity.SetSteamID( SteamUser.GetSteamID() );
		SteamNetworkingSockets.ResetIdentity( ref localIdentity );

		SteamNetworkingUtils.SetDebugOutputFunction(
			ESteamNetworkingSocketsDebugOutputType.k_ESteamNetworkingSocketsDebugOutputType_Verbose,
			( type, message ) => {
				Console.PrintLine( string.Format( "[STEAM NET] {0}:{1}", type, message ) );
			}
		);


		NetworkThread = new System.Threading.Thread(() => {
			while (System.Threading.Interlocked.Equals( NetworkRunning, 0 ) == true) {
				lock ( NetworkLock ) {
					PollIncomingMessages();
				}
				System.Threading.Thread.Sleep(15);
			}
		});
//		NetworkThread.Start();

		Console.AddCommand( "lobby_info", Callable.From( CmdLobbyInfo ), Array.Empty<string>(), 0, "prints lobby information." );

		ThisSteamID = SteamManager.GetSteamID();
	}

	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 20 ) != 0 ) {
			return;
		}
		lock ( NetworkLock ) {
			PollIncomingMessages();

			HandleIncomingMessages();

			// Send updates
			foreach ( var node in NodeCache.Values ) {
				node.Send?.Invoke();
			}
			foreach ( var player in PlayerCache.Values ) {
				player.Send?.Invoke();
			}
		}
	}

	public override void _Notification( int what ) {
		base._Notification( what );

		if ( what == NotificationWMCloseRequest ) {
			System.Threading.Interlocked.Exchange( ref NetworkRunning, 1 );
			NetworkThread.Join();
			LeaveLobby();
		}
	}
};
*/

using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Multiplayer;

public partial class SteamLobby : Node {
	public enum Visibility {
		Private,
		Public,
		FriendsOnly
	};

	public enum MessageType {
		Handshake,
		ServerCommand,
		ClientData,
		GameData,
		Count
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
	private System.IO.MemoryStream PacketStream = null;
	private System.IO.BinaryReader PacketReader = null;

	private CSteamID ThisSteamID = CSteamID.Nil;
	private int ThisSteamIDIndex = 0;

	private System.Threading.Thread NetworkThread;
	private int NetworkRunning = 0;
	private readonly object NetworkLock = new object();

	private Dictionary<int, NetworkNode> NodeCache = new Dictionary<int, NetworkNode>();
	private Dictionary<string, NetworkNode> PlayerCache = new Dictionary<string, NetworkNode>();
	private List<NetworkNode> PlayerList = new List<NetworkNode>( MAX_LOBBY_MEMBERS );

	[Signal]
	public delegate void ChatMessageReceivedEventHandler( ulong senderSteamId, string message );
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

	// Message processing
	private struct IncomingMessage {
		public CSteamID Sender;
		public byte[] Data;
		public MessageType Type;
	}
	private Queue<IncomingMessage> MessageQueue = new Queue<IncomingMessage>();

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
		PacketStream = new System.IO.MemoryStream( CachedPacket );
		PacketReader = new System.IO.BinaryReader( PacketStream );
		SetPhysicsProcess( true );
		SetProcess( true );

		// Start lobby discovery
		OpenLobbyList();

		// Start network thread
		NetworkThread = new System.Threading.Thread( () => {
			while ( Interlocked.CompareExchange( ref NetworkRunning, 0, 0 ) == 0 ) {
				lock ( NetworkLock ) {
					try {
						PollIncomingMessages();
					} catch ( Exception e ) {
						Console.PrintError( $"[NETWORK THREAD] Error: {e.Message}" );
					}
				}
				Thread.Sleep( 80 );
			}
		} );
		NetworkThread.Start();

		// Add console command
		Console.AddCommand( "lobby_info", Callable.From( CmdLobbyInfo ), Array.Empty<string>(), 0, "prints lobby information." );

		// Set local Steam ID
		ThisSteamID = SteamManager.GetSteamID();
		Console.PrintLine( $"[STEAM] Local Steam ID: {ThisSteamID}" );
	}

	public override void _Process( double delta ) {
		lock ( NetworkLock ) {
			HandleIncomingMessages();

			// Send updates
			foreach ( var node in NodeCache.Values ) {
				node.Send?.Invoke();
			}
			foreach ( var player in PlayerCache.Values ) {
				player.Send?.Invoke();
			}
		}
	}

	public override void _Notification( int what ) {
		base._Notification( what );

		if ( what == NotificationWMCloseRequest ) {
			Interlocked.Exchange( ref NetworkRunning, 1 );
			NetworkThread.Join();
			LeaveLobby();
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
		;

		Console.PrintLine( "Initializing SteamLobby..." );
		SteamAPICall_t handle = SteamMatchmaking.CreateLobby( lobbyType, LobbyMaxMembers );
		OnLobbyCreatedCallResult.Set( handle );
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

		// Close listen socket
		if ( ListenSocket != HSteamListenSocket.Invalid ) {
			SteamNetworkingSockets.CloseListenSocket( ListenSocket );
			ListenSocket = HSteamListenSocket.Invalid;
		}

		// Reset lobby data
		for ( int i = 0; i < LobbyMembers.Length; i++ ) {
			LobbyMembers[ i ] = CSteamID.Nil;
		}
		LobbyMemberCount = 0;

		NodeCache.Clear();
		PlayerCache.Clear();

		EmitSignal( "ClientLeftLobby", (ulong)SteamUser.GetSteamID() );

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

		ListenSocket = SteamNetworkingSockets.CreateListenSocketP2P( 0, 0, null );

		if ( ListenSocket == HSteamListenSocket.Invalid ) {
			Console.PrintError( "[STEAM] Failed to create listen socket" );
		} else {
			Console.PrintLine( "[STEAM] Created listen socket successfully" );
		}
	}

	private void ConnectToLobbyMembers() {
		for ( int i = 0; i < LobbyMemberCount; i++ ) {
			if ( LobbyMembers[ i ] == ThisSteamID ) continue;

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
				byte[] handshake = new byte[ 1 ] { (byte)MessageType.Handshake };
				SendTargetPacket( remoteId, handshake );
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
						EmitSignal( "ClientLeftLobby", (ulong)steamId );
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

	public void SendTargetPacket( CSteamID target, byte[] data ) {
		lock ( ConnectionLock ) {
			if ( Connections.TryGetValue( target, out HSteamNetConnection conn ) ) {
//				IntPtr ptr = Marshal.AllocHGlobal( data.Length );
				Marshal.Copy( data, 0, CachedWritePacket, data.Length );
				EResult res = SteamNetworkingSockets.SendMessageToConnection(
					conn,
					CachedWritePacket,
					(uint)data.Length,
					Constants.k_nSteamNetworkingSend_UnreliableNoDelay,
					out long _
				);

				if ( res != EResult.k_EResultOK ) {
					Console.PrintError( $"[STEAM] Error sending message to {target}: {res}" );
				}
			}
		}
	}

	public void SendP2PPacket( byte[] data ) {
		lock ( ConnectionLock ) {
			foreach ( var pair in Connections ) {
				if ( pair.Key != ThisSteamID ) {
					SendTargetPacket( pair.Key, data );
				}
			}
		}
	}

	private void ProcessIncomingMessage( byte[] data, CSteamID sender ) {
		using ( var stream = new System.IO.MemoryStream( data ) ) {
			using ( var reader = new System.IO.BinaryReader( stream ) ) {
				MessageType type = (MessageType)reader.ReadByte();
				MessageQueue.Enqueue( new IncomingMessage {
					Sender = sender,
					Data = data,
					Type = type
				} );
			}
		}
	}

	private void HandleIncomingMessages() {
		while ( MessageQueue.Count > 0 ) {
			var msg = MessageQueue.Dequeue();
			using ( var stream = new System.IO.MemoryStream( msg.Data ) ) {
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
					}
				}
			}
		}
	}

	private void PollIncomingMessages() {
		IntPtr[] messages = new IntPtr[ PACKET_READ_LIMIT ];

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
				int count = SteamNetworkingSockets.ReceiveMessagesOnConnection( activeConnections[ c ], messages, messages.Length );
//				if ( count > 0 ) Console.PrintLine( $"[STEAM] Received {count} messages" );

				for ( int i = 0; i < count; i++ ) {
					try {
						SteamNetworkingMessage_t message = (SteamNetworkingMessage_t)Marshal.PtrToStructure(
							messages[ i ],
							typeof( SteamNetworkingMessage_t )
						);

						byte[] data = new byte[ message.m_cbSize ];
						Marshal.Copy( message.m_pData, data, 0, message.m_cbSize );

						CSteamID sender = message.m_identityPeer.GetSteamID();
						ProcessIncomingMessage( data, sender );
					}
					finally {
						SteamNetworkingMessage_t.Release( messages[ i ] );
					}
				}
			} catch ( Exception e ) {
				Console.PrintError( $"[STEAM] Error receiving messages: {e.Message}" );
			}
		}

		// Process pending connections
		for ( int c = 0; c < pendingConns.Count; c++ ) {
			try {
				int count = SteamNetworkingSockets.ReceiveMessagesOnConnection( pendingConns[ c ], messages, messages.Length );
				if ( count > 0 ) Console.PrintLine( $"[STEAM] Received {count} pending messages" );

				for ( int i = 0; i < count; i++ ) {
					try {
						SteamNetworkingMessage_t message = (SteamNetworkingMessage_t)Marshal.PtrToStructure(
							messages[ i ],
							typeof( SteamNetworkingMessage_t )
						);

						byte[] data = new byte[ message.m_cbSize ];
						Marshal.Copy( message.m_pData, data, 0, (int)message.m_cbSize );

						CSteamID sender = message.m_identityPeer.GetSteamID();
						ProcessIncomingMessage( data, sender );
					}
					finally {
						SteamNetworkingMessage_t.Release( messages[ i ] );
					}
				}
			} catch ( Exception e ) {
				Console.PrintError( $"[STEAM] Error receiving pending messages: {e.Message}" );
			}
		}

		// Run SteamNetworkingSockets callbacks
		SteamNetworkingSockets.RunCallbacks();
	}

	// ==================== Steam Callbacks ====================
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
		EmitSignal( "LobbyCreated", (ulong)LobbyId );
	}

	private void OnLobbyMatchList( LobbyMatchList_t pCallback, bool bIOFailure ) {
		LobbyList.Clear();
		for ( int i = 0; i < pCallback.m_nLobbiesMatching; i++ ) {
			LobbyList.Add( SteamMatchmaking.GetLobbyByIndex( i ) );
		}
		EmitSignal( "LobbyListUpdated" );
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
		}
		;

		GetLobbyMembers();
		ConnectToLobbyMembers();
		EmitSignal( "LobbyJoined", (ulong)LobbyId );
	}

	private void OnLobbyChatMsg( LobbyChatMsg_t pCallback ) {
		byte[] szBuffer = new byte[ 1024 ];
		CSteamID senderId;
		EChatEntryType entryType;

		int ret = SteamMatchmaking.GetLobbyChatEntry(
			(CSteamID)pCallback.m_ulSteamIDLobby,
			(int)pCallback.m_iChatID,
			out senderId,
			szBuffer,
			szBuffer.Length,
			out entryType
		);

		if ( entryType == EChatEntryType.k_EChatEntryTypeChatMsg ) {
			string message = System.Text.Encoding.UTF8.GetString( szBuffer, 0, ret );
			EmitSignal( "ChatMessageReceived", (ulong)senderId, message );
		}
	}

	private void OnLobbyChatUpdate( LobbyChatUpdate_t pCallback ) {
		string changerName = SteamFriends.GetFriendPersonaName( (CSteamID)pCallback.m_ulSteamIDMakingChange );
		var stateChange = (EChatMemberStateChange)pCallback.m_rgfChatMemberStateChange;

		switch ( stateChange ) {
		case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
			ChatUpdate( $"{changerName} has joined..." );
			EmitSignal( "ClientJoinedLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
		case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
		case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
		case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
			ChatUpdate( $"{changerName} has left..." );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		}
		;
	}

	// ==================== Utility Methods ====================
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
		byte[] data = new byte[ 1 ] { (byte)MessageType.Handshake };
		SendP2PPacket( data );
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
		PlayerList.Remove( PlayerCache[ userId.ToString() ] );
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
};