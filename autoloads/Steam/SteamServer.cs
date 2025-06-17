using Godot;
using Steamworks;

public class SteamServer {
	private int MaxMembers = 0;
	private uint LobbyGameMode = 0;
	private string LobbyName;
	private string LobbyMap;
	private SteamLobby.Visibility LobbyVisibility = SteamLobby.Visibility.Public;

	public SteamServer( string name, string map, int nGameMode, int nMaxPlayers ) {
	}
};