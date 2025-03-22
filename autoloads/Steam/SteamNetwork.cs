using Godot;

public enum Permission : uint {
	Server,
	ClientAll,

	Count
};

/*
public partial class SteamNetwork : Node {
	private static System.Collections.Generic.Dictionary<ulong, CharacterBody2D> Peers;
	private static ulong MySteamId = 0;
	private static ulong ServerSteamId = 0;
	private static System.Collections.Generic.Dictionary<int, NodePath> NodePathCache;

//	private static System.Collections.Generic.Dictionary<> Permissions;

	public static void RegisterRPC( Node caller, string method, Permission permission ) {
		string permissionHash = GetPermissionHash( caller.GetPath(), method );
	}

	private static int GetPathCache( NodePath nodePath ) {
		for ( int i = 0; i < NodePathCache.Count; i++ ) {
			if ( NodePathCache[i] == nodePath ) {
				return i;
			}
		}
		return -1;
	}
	private static NodePath GetRSetPropertyPath( NodePath nodePath, string property ) {
		return new NodePath( string.Format( "{0}:{1}", nodePath, property ) );
	}
	private NodePath GetNodePath( int pathCacheIndex ) {
		return NodePathCache[ pathCacheIndex ];
	}
	private bool PeerConfirmedPath( ulong peer, NodePath nodePath ) {
		int pathCacheIndex = GetPathCache( nodePath );
		return PeerConfirmedPath[ peer.steamId ];
	}
	private static string GetPermissionHash( NodePath nodePath, string value = "" ) {
		if ( value.Length == 0 ) {
			return nodePath;
		}
		return ( nodePath + value ).Md5Text();
	}

	public static void ClearNodePathCache() {

	}

	public override void _Ready() {
		base._Ready();

		SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From( InitP2PSession ) );
	}
};
*/