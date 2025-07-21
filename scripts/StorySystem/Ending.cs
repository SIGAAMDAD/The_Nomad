using Godot;

namespace StorySystem {
	public partial class Ending : Resource {
		[Export]
		public PackedScene Scene { get; private set; }
		[Export]
		private Godot.Collections.Dictionary<StringName, Variant> Requirements;
	};
};