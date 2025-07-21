using Godot;

namespace StorySystem {
	public partial class NewGamePlusManager : Node {
		[Signal]
		public delegate void NGPlusStartEventHandler( int cycle, Ending ending );
	};
};