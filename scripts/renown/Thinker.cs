using Godot;

namespace Renown {
	public partial class Thinker : CharacterBody2D {
		

		[Export]
		private Godot.Collections.Array Traits;
		[Export]
		private Godot.Collections.Dictionary<string, string> Personality;
		[Export]
		private uint Age = 0; // in years
	};
};