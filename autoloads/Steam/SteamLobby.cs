using Godot;
using Steamworks;
using System;
using System.Collections.Generic;

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

	private static SteamLobby _Instance;
	public static SteamLobby Instance => _Instance;
	
	private const uint PACKET_READ_LIMIT = 32;
	private const uint MAX_LOBBY_MEMBERS = 16;
	
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
	private string LobbyMapName;
	private int LobbyMap = 0;
	private Visibility LobbyVisibility = Visibility.Public;

	private string LobbyFilterMap;
	private string LobbyFilterGameMode;

	private byte[] CachedPacket = null;
	private System.IO.MemoryStream PacketStream = null;
	private System.IO.BinaryReader PacketReader = null;
	
	private CSteamID ThisSteamID = CSteamID.Nil;
	private int ThisSteamIDIndex = 0;

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

	private Dictionary<int, NetworkNode> NodeCache = new Dictionary<int, NetworkNode>();
	private Dictionary<string, NetworkNode> PlayerCache = new Dictionary<string, NetworkNode>();

	public void AddPlayer( CSteamID userId, NetworkNode callbacks ) {
		GD.Print( "Added player with hash " + userId.ToString() + " to network sync cache." );
		PlayerCache.TryAdd( userId.ToString(), callbacks );
	}
	public void RemovePlayer( CSteamID userId ) {
		PlayerCache.Remove( userId.ToString() );
	}
	public void AddNetworkNode( NodePath node, NetworkNode callbacks ) {
		GD.Print( "Added node with hash " + node.GetHashCode() + " to network sync cache." );
		NodeCache.TryAdd( node.GetHashCode(), callbacks );
	}
	public Node GetNetworkNode( NodePath node ) {
		return NodeCache[ node.GetHashCode() ].Node;
	}

	public CSteamID GetHost() => LobbyOwnerId;
	public CSteamID GetLobbyID() {
		return LobbyId;
	}
	public bool IsOwner() {
		return IsHost;
	}
	public uint GetGameMode() {
		return LobbyGameMode;
	}
	public int GetMap() {
		return LobbyMap;
	}
	public List<CSteamID> GetLobbyList() {
		return LobbyList;
	}

	public void SetLobbyName( string name ) {
		LobbyName = name;
	}
	public void SetMaxMembers( int nMaxMembers ) {
		LobbyMaxMembers = nMaxMembers;
	}
	public void SetGameMode( uint nGameMode ) {
		LobbyGameMode = nGameMode;
	}
	public void SetMap( int nMapIndex ) {
		LobbyMap = nMapIndex;
		LobbyMapName = MultiplayerMapManager.MapCache[ LobbyMap ].Name;
	}
	public void SetHostStatus( bool bHost ) {
		IsHost = bHost;
	}

	private void ChatUpdate( string status ) {
		Console.PrintLine( "status" );
	}
	private void OnLobbyChatUpdate( LobbyChatUpdate_t pCallback ) {
		string changerName = SteamFriends.GetFriendPersonaName( (CSteamID)pCallback.m_ulSteamIDMakingChange );

		switch ( (EChatMemberStateChange)pCallback.m_rgfChatMemberStateChange ) {
		case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
			CallDeferred( "ChatUpdate", string.Format( "{0} has joined...", changerName ) );
			CallDeferred( "emit_signal", "ClientJoinedLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
			CallDeferred( "ChatUpdate", string.Format( "{0} has faded away...", changerName ) );
			CallDeferred( "emit_signal", "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
			CallDeferred( "ChatUpdate", string.Format( "{0} tweaked out so hard they left the fever dream...", changerName ) );
			CallDeferred( "emit_signal", "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
			CallDeferred( "ChatUpdate", string.Format( "{0} was rejected...", changerName ) );
			CallDeferred( "emit_signal", "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
			CallDeferred( "ChatUpdate", string.Format( "{0} was excommunicated...", changerName ) );
			CallDeferred( "emit_signal", "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
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
		SteamMatchmaking.SetLobbyData( LobbyId, "map", LobbyMap.ToString() );
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

		CallDeferred( "emit_signal", "LobbyListUpdated" );
	}
	private void MakeP2PHandkshake() {
		SendP2PPacket( new byte[]{ (byte)MessageType.Handshake } );
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

		CallDeferred( "emit_signal", "ClientLeftLobby", (ulong)SteamUser.GetSteamID() );
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
		
		SteamNetworking.SendP2PPacket( target, data, (uint)data.Length, EP2PSend.k_EP2PSendReliableWithBuffering, channel );
	}
	public void SendP2PPacket( CSteamID target, byte[] data ) {
		int channel = 0;
		
		for ( int i = 0; i < LobbyMemberCount; i += 2 ) {
			if ( i != ThisSteamIDIndex ) {
				SteamNetworking.SendP2PPacket( LobbyMembers[ i ], data, (uint)data.Length,
					EP2PSend.k_EP2PSendReliableWithBuffering, channel
				);
			}
			if ( i + 1 != ThisSteamIDIndex ) {
				SteamNetworking.SendP2PPacket( LobbyMembers[ i + 1 ], data, (uint)data.Length,
					EP2PSend.k_EP2PSendReliableWithBuffering, channel
				);
			}
		}
	}

	private void ReadP2Packet() {
		uint packetSize;
		if ( !SteamNetworking.IsP2PPacketAvailable( out packetSize ) ) {
			return;
		}

		CSteamID senderId;
		SteamNetworking.ReadP2PPacket( CachedPacket, packetSize, out packetSize, out senderId );
		PacketStream.Seek( 0, System.IO.SeekOrigin.Begin );

		switch ( (MessageType)PacketReader.ReadByte() ) {
		case MessageType.Handshake:
			GD.Print( SteamFriends.GetFriendPersonaName( senderId ) + " sent a handshake packet." );
			break;
		case MessageType.ServerCommand:
			ServerCommandManager.ExecuteCommand( (ServerCommandType)PacketReader.ReadUInt32() );
			break;
		case MessageType.ClientData:
			PlayerCache[ senderId.ToString() ].Receive( PacketReader );
			break;
		case MessageType.GameData:
			NodeCache[ PacketReader.ReadInt32() ].Receive( PacketReader );
			break;
		};
	}
	private void ReadPackets( uint readCount = 0 ) {
		if ( readCount >= PACKET_READ_LIMIT ) {
			return;
		}

		uint packetSize;
		if ( SteamNetworking.IsP2PPacketAvailable( out packetSize ) ) {
			ReadP2Packet();
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
			break;
		};
		LobbyMap = Convert.ToInt32( SteamMatchmaking.GetLobbyData( LobbyId, "map" ) );
		LobbyGameMode = Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );

		// more for debugging...
		GD.Print( "Sending p2p handshake..." );
		
		GetLobbyMembers();
		MakeP2PHandkshake();
		
		CallDeferred( "emit_signal", "LobbyJoined", (ulong)LobbyId );
	}

	private void OnLobbyJoined( LobbyEnter_t pCallback ) {
		if ( pCallback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess ) {
			Console.PrintError(
				string.Format( "[STEAM] Error joining lobby {0}: {1}", pCallback.m_ulSteamIDLobby
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
			break;
		};
		LobbyMap = Convert.ToInt32( SteamMatchmaking.GetLobbyData( LobbyId, "map" ) );
		LobbyGameMode = Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );

		GD.Print( "Sending p2p handshake..." );
		GetLobbyMembers();
		MakeP2PHandkshake();

		CallDeferred( "emit_signal", "LobbyJoined", (ulong)LobbyId );
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
		GD.Print( "[STEAM] " + requester + " sent a P2P session request" );
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
		byte[] szBuffer = new byte[ 4096 ];
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

	public float GetPing() {
		SteamNetworkPingLocation_t hPingLocation = new SteamNetworkPingLocation_t();
		return SteamNetworkingUtils.GetLocalPingLocation( out hPingLocation );
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
		
		CachedPacket = new byte[ 1024 ];
		PacketStream = new System.IO.MemoryStream( CachedPacket );
		PacketReader = new System.IO.BinaryReader( PacketStream );

		OpenLobbyList();

		SetProcess( false );
		SetPhysicsProcess( false );
		SetProcessInternal( false );
		SetPhysicsProcessInternal( false );
		
		ThisSteamID = SteamManager.GetSteamID();
	}
	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		foreach ( var node in NodeCache ) {
			node.Value.Send?.Invoke();
		}
		foreach ( var player in PlayerCache ) {
			player.Value.Send?.Invoke();
		}

		ReadPackets();
	}
};
