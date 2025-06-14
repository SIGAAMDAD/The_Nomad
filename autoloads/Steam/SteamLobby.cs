using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using Multiplayer;
using System.Threading;

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

	public class NetworkNode {
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

	private Thread NetworkThread;
	private int Done = 0;

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

	/*
	public void SendP2PPacket( CSteamID target, nint packet, int packetSize ) {
		int channel = 0;

		if ( target == CSteamID.Nil ) {
			for ( int i = 0; i < LobbyMembers.Count; i++ ) {
				if ( LobbyMembers[i] != SteamManager.GetSteamID() ) {
					SteamNetworkingIdentity identity = NetworkingSessions[ LobbyMembers[i] ];
					SteamNetworkingMessages.SendMessageToUser( ref identity, packet, (uint)packetSize, 0, channel );
				}
			}
		} else {
			SteamNetworkingIdentity identity = NetworkingSessions[ target ];
			SteamNetworkingMessages.SendMessageToUser( ref identity, packet, (uint)packetSize, 0, channel );
		}
	}
	*/
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

	/*
	private void OnIncomingConnectionRequest( SteamNetConnectionStatusChangedCallback_t pCallback ) {
		if ( pCallback.m_eOldState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting ) {
			GD.Print( "[STEAM] Connection established with " + pCallback.m_hConn );

			if ( SteamNetworkingSockets.AcceptConnection( pCallback.m_hConn ) != EResult.k_EResultOK ) {
				GD.PushError( "[STEAM] Error accepting connection!" );
				SteamNetworkingSockets.CloseConnection( pCallback.m_hConn, 0, "Connection refused", true );
			}
		}
	}
	*/

	private void OnP2PSessionRequest( P2PSessionRequest_t pCallback ) {
		string requester = SteamFriends.GetFriendPersonaName( pCallback.m_steamIDRemote );
		Console.PrintLine( "[STEAM] " + requester + " sent a P2P session request" );
		SteamNetworking.AcceptP2PSessionWithUser( pCallback.m_steamIDRemote );
	}
	/*
	private void OnSteamNetworkingMessagesSessionRequest( SteamNetworkingMessagesSessionRequest_t pCallback ) {
		string requester = SteamFriends.GetFriendPersonaName( pCallback.m_identityRemote.GetSteamID() );
		GD.Print( "[STEAM] " + requester + " sent a NetworkingMessage session request" );
		if ( !SteamNetworkingMessages.AcceptSessionWithUser( ref pCallback.m_identityRemote ) ) {
			GD.PushError( "[STEAM] Error accepting session request!" );
		}
	}
	*/
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
		
		CachedPacket = new byte[ 8192 ];
		PacketStream = new System.IO.MemoryStream( CachedPacket );
		PacketReader = new System.IO.BinaryReader( PacketStream );

		OpenLobbyList();

		NetworkThread = new Thread( NetworkProcess );
		NetworkThread.Start();

		Console.AddCommand( "lobby_info", Callable.From( CmdLobbyInfo ), Array.Empty<string>(), 0, "prints lobby information." );

		ThisSteamID = SteamManager.GetSteamID();
	}
	public override void _ExitTree() {
		base._ExitTree();

		Interlocked.Increment( ref Done );
	}

	private void NetworkProcess() {
		while ( Done != 1 ) {
			if ( !IsPhysicsProcessing() ) {
				continue;
			}
			Thread.Sleep( 250 );

			foreach ( var node in NodeCache ) {
				node.Value.Node.CallDeferred( "Send" );
			}
			foreach ( var player in PlayerCache ) {
				player.Value.Node.CallDeferred( "Send" );
			}

			ReadAllPackets();
		}
	}
};
