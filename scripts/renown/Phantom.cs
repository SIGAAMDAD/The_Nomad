using Godot;
using Renown.Thinkers;

namespace Renown {
	public enum WhisperType : uint {
		Count
	};

	public partial class Phantom : CharacterBody2D {
		private Thinker Thinker;
		private AudioStreamPlayer2D Whisper;

		public override void _Ready() {
			base._Ready();
		}
	};
};