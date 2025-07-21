using Godot;

namespace Renown.Thinkers {
	public partial class DialogueInteractor : InteractionItem {
		[Export]
		private Resource DialogueResource;

		private RichTextLabel Text;

		[Signal]
		public delegate void BeginDialogueEventHandler();
		[Signal]
		public delegate void EndDialogueEventHandler();

		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		}
		protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		}

		public override void _Ready() {
			base._Ready();

			Text = GetNode<RichTextLabel>( "RichTextLabel" );
			LevelData.Instance.ThisPlayer.InputMappingContextChanged += () => Text.ParseBbcode( AccessibilityManager.GetBindString( LevelData.Instance.ThisPlayer.InteractAction ) );

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
		}
	};
};