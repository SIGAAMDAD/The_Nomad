using System.Collections.Generic;
using Godot;

namespace Multiplayer {
	public abstract partial class Mode : Node2D {
		public enum GameMode {
			// classic modes
			Bloodbath,
			CaptureTheFlag,
			KingOfTheHill,

			// merc modes
			Duel,
			BountyHunt,
			Extraction,
			HoldTheLine,

			Count
		};

		protected GameMode Type;
		protected Dictionary<string, object> Options;

		public static readonly Dictionary<GameMode, string> ModeNames = new Dictionary<GameMode, string>{
			{ GameMode.Bloodbath, "Bloodbath" },
			{ GameMode.CaptureTheFlag, "Capture The Flag" },
			{ GameMode.KingOfTheHill, "King of the Hill" },

			{ GameMode.Duel, "Duel" },
			{ GameMode.BountyHunt, "Bounty Hunt" },
			{ GameMode.Extraction, "Extraction" },
			{ GameMode.HoldTheLine, "Hold The Line" }
		};

		[Signal]
		public delegate void ShowScoreboardEventHandler();
		[Signal]
		public delegate void EndGameEventHandler();

		public abstract void OnPlayerJoined( Renown.Entity player );
		public abstract void OnPlayerLeft( Renown.Entity player );
		public abstract void SpawnPlayer( Renown.Entity player );
		public abstract bool HasTeams();
		public abstract GameMode GetMode();
	};
};