using System.Collections.Generic;
using Godot;

namespace Multiplayer {
	public partial class Mode : Node2D {
		public enum GameMode {
			// classic modes
			Bloodbath,
			TeamBrawl,
			CaptureTheFlag,
			KingOfTheHill,
	
			// merc modes
			Duel,
			Blitz,
			BountyHuntPVE,
			BountyHuntPVP,
			ExtractionPVE,
			ExtractionPVP,

			Count
		};

		protected GameMode Type;

		public static readonly Dictionary<GameMode, string> ModeNames = new Dictionary<GameMode, string>{
			{ GameMode.Bloodbath, "Bloodbath" },
			{ GameMode.TeamBrawl, "Team Brawl" },
			{ GameMode.CaptureTheFlag, "Capture The Flag" },
			{ GameMode.KingOfTheHill, "King of the Hill" },

			{ GameMode.Duel, "Duel" },
			{ GameMode.Blitz, "Blitz" },
			{ GameMode.BountyHuntPVE, "Bounty Hunt PVE" },
			{ GameMode.BountyHuntPVP, "Bounty Hunt PVP" },
			{ GameMode.ExtractionPVE, "Extraction PVE" },
			{ GameMode.ExtractionPVP, "Extraction PVP" }
		};

		public virtual void OnPlayerJoined( CharacterBody2D player ) {
		}
		public virtual void OnPlayerLeft( CharacterBody2D player ) {
		}
		public virtual void SpawnPlayer( CharacterBody2D player ) {
		}

		public GameMode GetMode() {
			return Type;
		}
		public void SetMode( GameMode mode ) {
			Type = mode;
		}
	};
};