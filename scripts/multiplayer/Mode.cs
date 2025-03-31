using System.Collections.Generic;
using Godot;
using Steamworks;

namespace Multiplayer {
	public partial class Mode : Node {
		public enum GameMode {
			// classic modes
			Bloodbath,
			TeamBrawl,
			CaptureTheFlag,
			KingOfTheHill,
	
			// merc modes
			Blitz,
			BountyHuntPVE,
			BountyHuntPVP,
			ExtractionPVE,
			ExtractionPVP,
			Duel,

			Count
		};

		protected GameMode Type;

		public static readonly Dictionary<GameMode, string> ModeNames = new Dictionary<GameMode, string>{
			{ GameMode.Bloodbath, "Bloodbath" },
			{ GameMode.TeamBrawl, "Team Brawl" },
			{ GameMode.CaptureTheFlag, "Capture The Flag" },
			{ GameMode.KingOfTheHill, "King of the Hill" },

			{ GameMode.Blitz, "Blitz" },
			{ GameMode.BountyHuntPVE, "Bounty Hunt PVE" },
			{ GameMode.BountyHuntPVP, "Bounty Hunt PVP" },
			{ GameMode.ExtractionPVE, "Extraction PVE" },
			{ GameMode.ExtractionPVP, "Extraction PVP" },
			{ GameMode.Duel, "Duel" }
		};

		public virtual void OnPlayerJoined( Player player ) {
		}
		public virtual void OnPlayerLeft( Player player ) {
		}

		public GameMode GetMode() {
			return Type;
		}
		public void SetMode( GameMode mode ) {
			Type = mode;
		}
	};
};