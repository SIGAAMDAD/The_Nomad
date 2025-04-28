namespace Renown.Thinkers {
	// audio barks because in the heat of the moment, you really aren't gonna
	// be able to read lines of text
	public enum BarkType : uint {
		TargetSpotted,
		TargetPinned,
		TargetRunning,
		Stuck,
		ManDown,
		MenDown2,
		MenDown3,
		Confusion,
		Alert,
		OutOfTheWay,
		NeedBackup,
		SquadWiped,
		Curse,
		Quiet,
		CheckItOut,
		Unstoppable,
	
		Count
	};
	public enum MobAwareness : sbyte {
		Invalid = -1,
		Relaxed,
		Suspicious,
		Alert,
	
		Count
	};
};