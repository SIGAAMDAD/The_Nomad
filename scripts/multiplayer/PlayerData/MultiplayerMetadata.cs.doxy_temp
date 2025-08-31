using Steamworks;

namespace Multiplayer.PlayerData {
	public struct MultiplayerMetadata {
		public readonly CSteamID Id = CSteamID.Nil;
		public readonly string Username = "UNKNOWN";
		public int FlagCaptures = 0;
		public int FlagReturns = 0;
		public int Kills = 0;
		public int Deaths = 0;
		public float HillTime = 0.0f;
		public int Score = 0;

		public MultiplayerMetadata( CSteamID Id ) {
			this.Id = Id;
			Username = SteamFriends.GetFriendPersonaName( Id );
		}

		public static implicit operator CSteamID( MultiplayerMetadata meta ) => meta.Id;
	};
};