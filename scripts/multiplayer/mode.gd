class_name MultiplayerMode extends Node

enum GameMode {
	# classic modes
	Massacre,
	TeamBrawl,
	CaptureTheFlag,
	KingOfTheHill,
	
	# merc modes
	Blitz,
	BountyHuntPVE,
	BountyHuntPVP,
	ExtractionPVE,
	ExtractionPVP,
	Duel
};

const MODE_NAMES:Dictionary = {
	GameMode.Massacre: "Massacre",
	GameMode.TeamBrawl: "Team Brawl",
	GameMode.CaptureTheFlag: "Capture The Flag",
	GameMode.KingOfTheHill: "King of the Hill",
	
	GameMode.Blitz: "Blitz",
	GameMode.BountyHuntPVE: "Bounty Hunt PVE",
	GameMode.BountyHuntPVP: "Bounty Hunt PVP",
	GameMode.ExtractionPVE: "Extraction PVE",
	GameMode.ExtractionPVP: "Extraction PVP",
	GameMode.Duel: "Duel"
};
