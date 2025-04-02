using Godot;
using Multiplayer.Overlays;

namespace Multiplayer.Modes {
	public partial class CaptureTheFlag : Mode {
		/// <summary>
		/// player currently holding team 2's flag
		/// </summary>
		private CharacterBody2D Team1FlagHolder = null;
		
		/// <summary>
		/// player currently holding team 1's flag
		/// </summary>
		private CharacterBody2D Team2FlagHolder = null;

		public override void _Ready() {
			base._Ready();
		}
	};
};