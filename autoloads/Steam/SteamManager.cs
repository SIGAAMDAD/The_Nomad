using Godot;
using GodotSteam;

public partial class SteamAppManager : Node {
	private const uint AppID = 480;

    public override void _EnterTree() {
		OS.SetEnvironment( "SteamAppId", AppID.ToString() );
		OS.SetEnvironment( "SteamGameId", AppID.ToString() );
    }

	public void Init() {
		Steam.SteamInit();

		bool isSteamRunning = Steam.IsSteamRunning();
		if ( !isSteamRunning ) {
			GD.Print( "Steam is not running." );
			return;
		}

		ulong SteamID = Steam.GetSteamID();
		string Name = Steam.GetFriendPersonaName( SteamID );

		GD.Print( "Got steam name: " + Name );
	}

    public override void _Ready() {
	}

	public override void _Process( double delta ) {
	}
}
