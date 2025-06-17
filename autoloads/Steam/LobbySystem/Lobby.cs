using Steamworks;

namespace LobbySystem {
	public readonly struct Lobby {
		public readonly CSteamID Id = CSteamID.Nil;
		public readonly string Name;
		public readonly int MaxPlayers;

		public Lobby( string name, int maxPlayers ) {
		}
	};
};