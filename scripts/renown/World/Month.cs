using Godot;

namespace Renown.World {
	public partial class Month : Node {
		[Export]
		private int DayCount;

		public int GetDayCount() => DayCount;
	};
};