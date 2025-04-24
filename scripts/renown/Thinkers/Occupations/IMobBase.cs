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
	public enum MobState : sbyte {
		Invalid = -1,
		Guarding, // guarding a node
	
		// moving along a patrol link chain
		PatrolStart,
		Patrolling,
	
		Attacking,
	
		Scared, // Fear > 80
		Investigating, // investigating a position

		Count
	};	
	public enum DirType {
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest
	};
};