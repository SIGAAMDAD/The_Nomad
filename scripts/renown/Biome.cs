using Godot;

namespace Renown {
	public partial class Biome : Node2D {
		public enum WeatherType {
			Rainy,
			Snowing,
			British,
			Blazing,
			Clear,

			Count
		};

		[Export]
		private Godot.Collections.Array<WeatherType> Weather;
		[Export]
		private Godot.Collections.Array Settlements;

		private Timer WeatherChangeTimer;

		public override void _Ready() {
			base._Ready();
		}
    };
};