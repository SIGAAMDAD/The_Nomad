using Godot;
using Multiplayer.Overlays;
using Steamworks;

namespace Multiplayer.Modes {
	public partial class CaptureTheFlag : Mode {
		/// <summary>
		/// player currently holding team 2's flag
		/// </summary>
		private CSteamID Team1FlagHolder = CSteamID.Nil;
		
		/// <summary>
		/// player currently holding team 1's flag
		/// </summary>
		private CSteamID Team2FlagHolder = CSteamID.Nil;

		public override void _Ready() {
			base._Ready();
		}
	};
};