using Godot;

namespace Renown.Thinkers.GoapCache {
	public partial class EntityGoapData : Resource {
		[Export]
		public Godot.Collections.Array<ActionType> AllowedActions { get; private set; }
		[Export]
		public Godot.Collections.Array<GoalType> AllowedGoals { get; private set; }
		[Export]
		public Godot.Collections.Array<SensorType> AllowedSensors { get; private set; }
	};
};