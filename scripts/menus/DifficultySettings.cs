using Godot;

public partial class DifficultySettings : Resource {
	[Export] public bool NPCPermaDeath { get; private set; }
	[Export] public bool FirelinkLimit { get; private set; }
	[Export] public bool AuditoryHallucinations { get; private set; }
	[Export] public bool FreeFastTravel { get; private set;  }
};