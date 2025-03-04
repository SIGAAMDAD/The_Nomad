using MessagePack;
using Godot;
using GodotSteam;
using System.Xml;
using System.Linq.Expressions;
using System.Linq;

public partial class SteamLobby : Node {
	public enum Visibility {
		Private,
		Public,
		FriendsOnly
	};

	private static SteamLobby _Instance;
	public static SteamLobby Instance => _Instance;

	private System.Collections.Generic.List<ulong> LobbyMembers = new System.Collections.Generic.List<ulong>();
	private ulong LobbyId = 0;
	private ulong LobbyOwnerId = 0;
	private bool IsHost = false;

	private System.Collections.Generic.List<ulong> LobbyList = new System.Collections.Generic.List<ulong>();
	private uint LobbyMaxMembers = 0;
	private uint LobbyGameMode = 0;
	private string LobbyName;
	private string LobbyMapName;
	private int LobbyMap = 0;
	private Visibility LobbyVisibility = Visibility.Public;

	private string LobbyFilterMap;
	private string LobbyFilterGameMode;

	private const uint PACKET_READ_LIMIT = 32;

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

	public enum MessageType {
		Handshake,

		// hot cache player data
		ClientData,

		// gamestate
		ServerData,

		// player data that only needs spare updates
		PlayerUpdate,
		
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

	public ulong GetLobbyID() {
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
	public System.Collections.Generic.List<ulong> GetLobbyList() {
		return LobbyList;
	}

	public void SetLobbyName( string name ) {
		LobbyName = name;
	}
	public void SetMaxMembers( uint nMaxMembers ) {
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

	private void OpenLobbyList( Godot.Collections.Array lobbies ) {
		LobbyList.Clear();
		foreach ( var lobby in lobbies ) {
			LobbyList.Add( (ulong)lobby );
		}
	}

	private void OnLobbyChatUpdate( ulong lobbyId, long changeId, long makingChangeId, long chatState ) {
		string changerName = Steam.GetFriendPersonaName( (ulong)changeId );

		switch ( chatState ) {
		case (long)Steam.ChatMemberStateChange.Entered:
			GetNode( "Console" ).Call( "print_line", changerName + " has joined...", true );
			EmitSignal( "ClientJoinedLobby", changeId );
			break;
		case (long)Steam.ChatMemberStateChange.Left:
			GetNode( "Console" ).Call( "print_line", changerName + " has faded away...", true );
			EmitSignal( "ClientLeftLobby", changeId );
			break;
		};
	}
	private void OnLobbyCreated( long connect, ulong lobbyId ) {
		if ( connect != 1 ) {
			return;
		}

		GD.Print( "Created lobby " + lobbyId + "." );
		LobbyId = lobbyId;
		IsHost = true;

		Steam.SetLobbyJoinable( lobbyId, true );
		Steam.SetLobbyData( lobbyId, "name", LobbyName );
		Steam.SetLobbyData( lobbyId, "map", LobbyMap.ToString() );
		Steam.SetLobbyData( lobbyId, "gamemode", LobbyGameMode.ToString() );

		bool bSetRelay = Steam.AllowP2PPacketRelay( true );
		if ( !bSetRelay ) {
			GD.PushError( "[STEAM] couldn't enable p2p packet relay!" );
			return;
		}

		EmitSignal( "LobbyCreated", lobbyId );
	}
	private void OnLobbyMatchList( Godot.Collections.Array lobbyList ) {
		LobbyList.Clear();
		foreach ( var lobby in lobbyList ) {
			LobbyList.Add( (ulong)lobby );
		}
	}
	private void MakeP2PHandkshake() {
		System.Collections.Generic.Dictionary<string, object> packet = new System.Collections.Generic.Dictionary<string, object>();
		packet[ "message" ] = "handshake";
		packet[ "remote_steam_id" ] = SteamManager.GetSteamID();
		packet[ "username" ] = SteamManager.GetSteamName();
		SendP2PPacket( 0, packet );
	}
	private void GetLobbyMembers() {
		LobbyMembers.Clear();

		int memberCount = Steam.GetNumLobbyMembers( LobbyId );
		for ( int i = 0; i < memberCount; i++ ) {
			ulong steamId = Steam.GetLobbyMemberByIndex( LobbyId, i );
			string username = Steam.GetFriendPersonaName( steamId );

			LobbyMembers.Add( steamId );
		}
	}

	private void OpenLobbyList() {
		if ( LobbyFilterMap != "Any" ) {
			Steam.AddRequestLobbyListStringFilter( "map", LobbyFilterMap, Steam.LobbyComparison.LobbyComparisonEqual );
		}
		if ( LobbyFilterGameMode != "Any" ) {
			Steam.AddRequestLobbyListStringFilter( "gamemode", LobbyFilterGameMode, Steam.LobbyComparison.LobbyComparisonEqual );
		}
		Steam.AddRequestLobbyListDistanceFilter( Steam.LobbyDistanceFilter.Worldwide );
		Steam.RequestInternetServerList( SteamManager.GetAppID(), new Godot.Collections.Array() );
		Steam.RequestLobbyList();
	}

	public void CreateLobby() {
		if ( LobbyId != 0 ) {
			return;
		}
		IsHost = true;

		Steam.LobbyType lobbyType = Steam.LobbyType.Private;
		switch ( LobbyVisibility ) {
		case Visibility.Private:
			lobbyType = Steam.LobbyType.Private;
			break;
		case Visibility.Public:
			lobbyType = Steam.LobbyType.Public;
			break;
		case Visibility.FriendsOnly:
			lobbyType = Steam.LobbyType.FriendsOnly;
			break;
		};

		GD.Print( "Initializing SteamLobby..." );
		Steam.CreateLobby( lobbyType, LobbyMaxMembers );
	}
	public void JoinLobby( ulong lobbyId ) {
		Steam.JoinLobby( lobbyId );
	}
	public void LeaveLobby() {
		if ( LobbyId == 0 ) {
			return;
		}

		GD.Print( "Leaving lobby " + LobbyId + "..." );
		Steam.LeaveLobby( LobbyId );
		LobbyId = 0;

		foreach ( var member in LobbyMembers ) {
			Godot.Collections.Dictionary sessionState = Steam.GetP2PSessionState( member );
			if ( sessionState.ContainsKey( "connection_active" ) && (bool)sessionState[ "connection_active" ] ) {
				Steam.CloseP2PSessionWithUser( member );
			}
		}
		LobbyMembers.Clear();

		EmitSignal( "ClientLeftLobby", SteamManager.GetSteamID() );
	}
	public void SendP2PPacket( ulong target, System.Collections.Generic.Dictionary<string, object> packet, Steam.P2PSend sendType = 0 ) {
		int channel = 0;

		if ( target == 0 ) {
			foreach ( var member in LobbyMembers ) {
				if ( member != SteamManager.GetSteamID() ) {
					Steam.SendP2PPacket( member, MessagePackSerializer.Serialize( packet ), sendType, channel );
				}
			}
		} else {
			Steam.SendP2PPacket( target, MessagePackSerializer.Serialize( packet ), sendType, channel );
		}
	}
	private void ReadP2Packet() {
		uint packetSize = Steam.GetAvailableP2PPacketSize( 0 );
		if ( packetSize == 0 ) {
			return;
		}
		Godot.Collections.Dictionary packet = Steam.ReadP2PPacket( packetSize, 0 );
		System.Collections.Generic.Dictionary<string, object> data = MessagePackSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>( (byte[])packet[ "data" ] );
		
		switch ( (uint)data[ "message" ] ) {
		case (uint)MessageType.Handshake:
			GD.Print( (string)packet[ "username" ] + " sent a handshake packet." );
			break;
		case (uint)MessageType.ClientData:
			( (MultiplayerData)GetTree().CurrentScene ).ProcessClientData( (ulong)packet[ "remote_steam_id" ], (System.Collections.Generic.Dictionary<string, object>)data[ "packet" ] );
			break;
		case (uint)MessageType.ServerData:
			( (MultiplayerData)GetTree().CurrentScene ).ProcessHeartbeat( (System.Collections.Generic.Dictionary<string, object>)data[ "packet" ] );
			break;
		case (uint)MessageType.PlayerUpdate:
			( (MultiplayerData)GetTree().CurrentScene ).ProcessPlayerUpdate( (ulong)packet[ "remote_steam_id" ], (System.Collections.Generic.Dictionary<string, object>)data[ "packet" ] );
			break;
		};
	}
	public void ReadAllPackets( uint readCount = 0 ) {
		if ( readCount >= PACKET_READ_LIMIT ) {
			return;
		}

		if ( Steam.GetAvailableP2PPacketSize( 0 ) > 0 ) {
			ReadP2Packet();
			ReadAllPackets( readCount + 1 );
		}
	}
	
	private void OnLobbyJoined( ulong lobbyId, long permissions, bool locked, long response ) {
		if ( response == (long)ChatRoomEnterResponse.CHAT_ROOM_ENTER_RESPONSE_SUCCESS ) {
			LobbyId = lobbyId;
			LobbyOwnerId = Steam.GetLobbyOwner( lobbyId );

			GD.Print( "Joined lobby " + lobbyId + "." );

			LobbyName = Steam.GetLobbyData( lobbyId, "name" );
			LobbyMap = Steam.GetLobbyData( lobbyId, "map" ).ToInt();
			LobbyGameMode = (uint)Steam.GetLobbyData( lobbyId, "gamemode" ).ToInt();

			GetLobbyMembers();
			MakeP2PHandkshake();

			EmitSignal( "LobbyJoined", lobbyId );
		}
	}
	private void OnP2PSessionRequest( ulong remoteId ) {
		string requester = Steam.GetFriendPersonaName( remoteId );
		Steam.AcceptP2PSessionWithUser( remoteId );
	}
	private void OnLobbyDataUpdated( long success, ulong lobbyId, ulong memberId ) {
	}
	private void OnLobbyMessage( ulong lobbyId, long user, string message, long chatType ) {
		switch ( (ChatEntryType)chatType ) {
		case ChatEntryType.CHAT_ENTRY_TYPE_CHAT_MSG:
			EmitSignal( "ChatMessageReceived", (ulong)user, message );
			break;
		};
	}

	public override void _EnterTree() {
		base._EnterTree();
		if ( _Instance != null ) {
			this.QueueFree();
		}
		_Instance = this;
	}
    public override void _Ready() {
		Steam.LobbyChatUpdate += ( lobbyId, changeId, makingChangeId, chatState ) => OnLobbyChatUpdate( lobbyId, changeId, makingChangeId, chatState );
		Steam.LobbyCreated += ( connect, lobbyId ) => OnLobbyCreated( connect, lobbyId );
		Steam.LobbyMatchList += ( lobbyList ) => OnLobbyMatchList( lobbyList );
		Steam.LobbyMessage += ( lobbyId, user, message, chatType ) => OnLobbyMessage( lobbyId, user, message, chatType );
		Steam.P2PSessionRequest += ( remoteId ) => OnP2PSessionRequest( remoteId );
		Steam.LobbyJoined += ( lobbyId, permissions, locked, response  ) => OnLobbyJoined( lobbyId, permissions, locked, response );

		OpenLobbyList();
	}
};