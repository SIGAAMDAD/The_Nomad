using MessagePack;
using Godot;
using Steamworks;
using System;

public partial class SteamLobby : Node {
	public enum Visibility {
		Private,
		Public,
		FriendsOnly
	};

	private static SteamLobby _Instance;
	public static SteamLobby Instance => _Instance;

	private Callback<LobbyEnter_t> LobbyEnter;
	private Callback<LobbyChatMsg_t> LobbyChatMsg;
	private Callback<LobbyChatUpdate_t> LobbyChatUpdate;
	private Callback<P2PSessionRequest_t> P2PSessionRequest;
	private Callback<P2PSessionConnectFail_t> P2PSessionConnectFail;

	private CallResult<LobbyCreated_t> OnLobbyCreatedCallResult;
	private CallResult<LobbyEnter_t> OnLobbyEnterCallResult;
	private CallResult<LobbyMatchList_t> OnLobbyMatchListCallResult;

	private System.Collections.Generic.List<CSteamID> LobbyMembers = new System.Collections.Generic.List<CSteamID>();
	private CSteamID LobbyId = CSteamID.Nil;
	private CSteamID LobbyOwnerId = CSteamID.Nil;
	private bool IsHost = false;

	private HServerListRequest ServerListRequest;
	private ISteamMatchmakingServerListResponse ServerListResponse;
	private ISteamMatchmakingPingResponse PingResponse;
	private ISteamMatchmakingPlayersResponse PlayersResponse;
	private ISteamMatchmakingRulesResponse RulesResponse;

	private System.Collections.Generic.List<CSteamID> LobbyList = new System.Collections.Generic.List<CSteamID>();
	private int LobbyMaxMembers = 0;
	private uint LobbyGameMode = 0;
	private string LobbyName;
	private string LobbyMapName;
	private int LobbyMap = 0;
	private Visibility LobbyVisibility = Visibility.Public;

	private string LobbyFilterMap;
	private string LobbyFilterGameMode;

	private const uint PACKET_READ_LIMIT = 32;

	/*
	public enum ChatRoomEnterResponse {
		CHAT_ROOM_ENTER_RESPONSE_SUCCESS = 1,
		CHAT_ROOM_ENTER_RESPONSE_DOESNT_EXIST,
		CHAT_ROOM_ENTER_RESPONSE_NOT_ALLOWED,
		CHAT_ROOM_ENTER_RESPONSE_FULL,
		CHAT_ROOM_ENTER_RESPONSE_ERROR,
		CHAT_ROOM_ENTER_RESPONSE_BANNED,
		CHAT_ROOM_ENTER_RESPONSE_LIMITED,
		CHAT_ROOM_ENTER_RESPONSE_CLAN_DISABLED,
		CHAT_ROOM_ENTER_RESPONSE_COMMUNITY_BAN,
		CHAT_ROOM_ENTER_RESPONSE_MEMBER_BLOCKED_YOU,
		CHAT_ROOM_ENTER_RESPONSE_YOU_BLOCKED_MEMBER,
		CHAT_ROOM_ENTER_RESPONSE_RATE_LIMIT_EXCEEDED
	};

	public enum ChatEntryType {
		CHAT_ENTRY_TYPE_INVALID,
		CHAT_ENTRY_TYPE_CHAT_MSG,
		CHAT_ENTRY_TYPE_TYPING,
		CHAT_ENTRY_TYPE_INVITE_GAME,
		CHAT_ENTRY_TYPE_EMOTE,
		CHAT_ENTRY_TYPE_LEFT_CONVERSATION,
		CHAT_ENTRY_TYPE_ENTERED,
		CHAT_ENTRY_TYPE_WAS_KICKED,
		CHAT_ENTRY_TYPE_WAS_BANNED,
		CHAT_ENTRY_TYPE_DISCONNECTED
	};
	*/

	public enum MessageType {
		Handshake,

		// hot cache player data
		ClientData,

		// gamestate
		ServerData,
		
		Count
	};

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
	public System.Collections.Generic.List<CSteamID> GetLobbyList() {
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

	private void OnLobbyChatUpdate( LobbyChatUpdate_t pCallback ) {
		string changerName = SteamFriends.GetFriendPersonaName( (CSteamID)pCallback.m_ulSteamIDMakingChange );
//		string changerName = SteamFriends.GetFriendPersonaName( changeId );

		switch ( (EChatMemberStateChange)pCallback.m_rgfChatMemberStateChange ) {
		case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
			GetNode( "Console" ).Call( "print_line", changerName + " has joined...", true );
			EmitSignal( "ClientJoinedLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
			GetNode( "Console" ).Call( "print_line", changerName + " has faded away...", true );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
			GetNode( "Console" ).Call( "print_line", changerName + " tweaked out...", true );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
			GetNode( "Console" ).Call( "print_line", changerName + " was rejected...", true );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
			GetNode( "Console" ).Call( "print_line", changerName + " was excommunicated...", true );
			EmitSignal( "ClientLeftLobby", pCallback.m_ulSteamIDMakingChange );
			break;
		};
	}
	private void OnLobbyCreated( LobbyCreated_t pCallback, bool bIOFailure ) {
		if ( pCallback.m_eResult != EResult.k_EResultOK ) {
			return;
		}

		GD.Print( "Created lobby " + pCallback.m_ulSteamIDLobby + "." );
		LobbyId = (CSteamID)pCallback.m_ulSteamIDLobby;
		IsHost = true;

		SteamMatchmaking.SetLobbyJoinable( LobbyId, true );
		SteamMatchmaking.SetLobbyData( LobbyId, "appid", SteamManager.GetAppID().ToString() );
		SteamMatchmaking.SetLobbyData( LobbyId, "name", LobbyName );
		SteamMatchmaking.SetLobbyData( LobbyId, "map", LobbyMap.ToString() );
		SteamMatchmaking.SetLobbyData( LobbyId, "gamemode", LobbyGameMode.ToString() );

		bool bSetRelay = SteamNetworking.AllowP2PPacketRelay( true );
		if ( !bSetRelay ) {
			GD.PushError( "[STEAM] couldn't enable p2p packet relay!" );
			return;
		}
	}
	private void OnLobbyMatchList( LobbyMatchList_t pCallback, bool bIOFailure ) {	
		GD.Print( "Received lobby match list." );

		LobbyList.Clear();
		for ( int i = 0; i < pCallback.m_nLobbiesMatching; i++ ) {
			LobbyList.Add( SteamMatchmaking.GetLobbyByIndex( i ) );
		}

		EmitSignal( "LobbyListUpdated" );
	}
	private void MakeP2PHandkshake() {
		SendP2PPacket( CSteamID.Nil, new System.Collections.Generic.Dictionary<string, object>(), MessageType.Handshake );
	}
	public void GetLobbyMembers() {
		LobbyMembers.Clear();

		int memberCount = SteamMatchmaking.GetNumLobbyMembers( LobbyId );
		for ( int i = 0; i < memberCount; i++ ) {
			CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex( LobbyId, i );
			string username = SteamFriends.GetFriendPersonaName( steamId );

			LobbyMembers.Add( steamId );
		}
	}

	public void OpenLobbyList() {
//		if ( LobbyFilterMap != "Any" ) {
//			SteamMatchmaking.AddRequestLobbyListStringFilter( "map", LobbyFilterMap, ELobbyComparison.k_ELobbyComparisonEqual );
//		}
//		if ( LobbyFilterGameMode != "Any" ) {
//			SteamMatchmaking.AddRequestLobbyListStringFilter( "gamemode", LobbyFilterGameMode, ELobbyComparison.k_ELobbyComparisonEqual );
//		}

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

		GD.Print( "Initializing SteamLobby..." );
		SteamMatchmaking.CreateLobby( lobbyType, LobbyMaxMembers );
	}
	public void JoinLobby( CSteamID lobbyId ) {
		SteamMatchmaking.JoinLobby( lobbyId );
	}
	public void LeaveLobby() {
		if ( !LobbyId.IsValid() ) {
			return;
		}

		GD.Print( "Leaving lobby " + LobbyId + "..." );
		SteamMatchmaking.LeaveLobby( LobbyId );
		LobbyId = CSteamID.Nil;

		for ( int i = 0; i < LobbyMembers.Count; i++ ) {
			P2PSessionState_t sessionState;
			if ( !SteamNetworking.GetP2PSessionState( LobbyMembers[i], out sessionState ) ) {
				continue;
			}
			if ( sessionState.m_bConnectionActive == 1 ) {
				SteamNetworking.CloseP2PSessionWithUser( LobbyMembers[i] );
			}
		}
		LobbyMembers.Clear();

		EmitSignal( "ClientLeftLobby", (ulong)SteamUser.GetSteamID() );
	}
	public void SendP2PPacket( CSteamID target, System.Collections.Generic.Dictionary<string, object> packet, MessageType messageType ) {
		int channel = 0;

		packet[ "message" ] = messageType;

		byte[] data = MessagePackSerializer.Serialize( packet );
		if ( !target.IsValid() ) {
			for ( int i = 0; i < LobbyMembers.Count; i++ ) {
				if ( LobbyMembers[i] != SteamUser.GetSteamID() ) {
					SteamNetworking.SendP2PPacket( LobbyMembers[i], data, (uint)data.Length, EP2PSend.k_EP2PSendReliable, channel );
				}
			}
		} else {
			SteamNetworking.SendP2PPacket( target, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable, channel );
		}
	}
	private void ReadP2Packet() {
		uint packetSize;
		if ( !SteamNetworking.IsP2PPacketAvailable( out packetSize ) ) {
			return;
		}

		CSteamID senderId;
		byte[] data = new byte[ packetSize ];
		SteamNetworking.ReadP2PPacket( data, (uint)data.Length, out packetSize, out senderId );
		System.Collections.Generic.Dictionary<string, object> packet = MessagePackSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>( data );
		
		switch ( (uint)packet[ "message" ] ) {
		case (uint)MessageType.Handshake:
			GD.Print( (string)packet[ "username" ] + " sent a handshake packet." );
			break;
		case (uint)MessageType.ClientData:
			( (MultiplayerData)GetTree().CurrentScene ).ProcessClientData( (ulong)senderId, (System.Collections.Generic.Dictionary<string, object>)packet[ "packet" ] );
			break;
		case (uint)MessageType.ServerData:
			( (MultiplayerData)GetTree().CurrentScene ).ProcessHeartbeat( (System.Collections.Generic.Dictionary<string, object>)packet[ "packet" ] );
			break;
		};
	}
	public void ReadAllPackets( uint readCount = 0 ) {
		if ( readCount >= PACKET_READ_LIMIT ) {
			return;
		}

		uint packetSize;
		if ( SteamNetworking.IsP2PPacketAvailable( out packetSize ) ) {
			ReadP2Packet();
			ReadAllPackets( readCount + 1 );
		}
	}
	
	private void OnLobbyJoined( LobbyEnter_t pCallback, bool bIOFailure ) {
		OnLobbyJoined( pCallback );
	}
	private void OnLobbyJoined( LobbyEnter_t pCallback ) {
		GD.Print( "...Joined" );
		if ( pCallback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess ) {
			GD.PushError( "[STEAM] Error joining lobby " + pCallback.m_ulSteamIDLobby + ": " + ( (EChatRoomEnterResponse)pCallback.m_EChatRoomEnterResponse ).ToString() );
			return;
		}

		LobbyId = (CSteamID)pCallback.m_ulSteamIDLobby;
		LobbyOwnerId = SteamMatchmaking.GetLobbyOwner( LobbyId );

		GD.Print( "Joined lobby " + pCallback.m_ulSteamIDLobby + "." );

		LobbyName = SteamMatchmaking.GetLobbyData( LobbyId, "name" );
		LobbyMap = Convert.ToInt32( SteamMatchmaking.GetLobbyData( LobbyId, "map" ) );
		LobbyGameMode = Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );

		GetLobbyMembers();
		MakeP2PHandkshake();

		EmitSignal( "LobbyJoined", (ulong)LobbyId );
	}
	private void OnP2PSessionRequest( P2PSessionRequest_t pCallback ) {
		string requester = SteamFriends.GetFriendPersonaName( pCallback.m_steamIDRemote );
		SteamNetworking.AcceptP2PSessionWithUser( pCallback.m_steamIDRemote );
	}
	private void OnLobbyDataUpdated( long success, ulong lobbyId, ulong memberId ) {
	}
	private void OnLobbyMessage( LobbyChatMsg_t pCallback ) {
		byte[] szBuffer = new byte[ 4096 ];
		CSteamID senderId;
		EChatEntryType entryType;
		int ret = SteamMatchmaking.GetLobbyChatEntry( (CSteamID)pCallback.m_ulSteamIDLobby, (int)pCallback.m_iChatID, out senderId, szBuffer, szBuffer.Length, out entryType );

		switch ( entryType ) {
		case EChatEntryType.k_EChatEntryTypeChatMsg: {
			EmitSignal( "ChatMessageReceived", (ulong)senderId, szBuffer.ToString() );
			break; }
		};
	}

	private void OnRefreshComplete( HServerListRequest hRequest, EMatchMakingServerResponse response ) {
		GD.Print( "[STEAM] Server list refresh finished: " + response.ToString() );
	}
	private void OnServerResponded( HServerListRequest hRequest, int iServer ) {
	}
	private void OnServerFailedToRespond( HServerListRequest hRequest, int iServer ) {
		GD.PushError( "[STEAM] Server failed to respond" );
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

		/*
		Steam.LobbyChatUpdate += OnLobbyChatUpdate;
		Steam.LobbyCreated += OnLobbyCreated;
		Steam.LobbyMatchList += OnLobbyMatchList;
		Steam.LobbyMessage += OnLobbyMessage;
		Steam.P2PSessionRequest += OnP2PSessionRequest;
		Steam.LobbyJoined += OnLobbyJoined;
		Steam.LobbyMatchList += OnLobbyMatchList;
		*/

		OpenLobbyList();
	}
};