using Godot;
using GodotSteam;

[GlobalClass]
public partial class SteamDataManager : Node {
	private uint LifeTimeKills = 0;
	private uint LifeTimeDeaths = 0;
	private uint LifeTimeWins = 0;
	private uint LifeTimeLosses = 0;
	private uint LifeTimeGames = 0;
	private float LifeTimeKDR = 0.0f;
	private bool UserStatsReceived = false;

	public void SaveStats() {
		FileAccess file = FileAccess.Open( "user://stats.dat", FileAccess.ModeFlags.Write );
		file.Store32( LifeTimeKills );
		file.Store32( LifeTimeDeaths );
		file.Store32( LifeTimeWins );
		file.Store32( LifeTimeLosses );
		file.Store32( LifeTimeGames );
		file.StoreFloat( LifeTimeKDR );
		file.Close();

		if ( UserStatsReceived ) {
			Steam.SetStatInt( "LifeTime Kills", LifeTimeKills );
			Steam.SetStatInt( "LifeTime Deaths", LifeTimeDeaths );
			Steam.SetStatInt( "LifeTime Wins", LifeTimeWins );
			Steam.SetStatInt( "LifeTime Losses", LifeTimeLosses );
			Steam.SetStatInt( "LifeTime Games", LifeTimeGames );
			Steam.SetStatFloat( "LifeTime KDR", LifeTimeKDR );

			if ( !Steam.StoreStats() ) {
				GD.PushError( "Store.StoreStats failed!" );
			}
		}
	}

	public void LoadStats() {
	}
};