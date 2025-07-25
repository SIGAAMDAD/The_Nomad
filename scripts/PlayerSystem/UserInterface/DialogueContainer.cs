using DialogueManagerRuntime;
using Godot;
namespace PlayerSystem.UserInterface {
	public partial class DialogueContainer : MarginContainer {
		private VBoxContainer MainContainer;

		private static DialogueContainer Instance;

		public static void AddOption( StringName translationString, Callable callback ) {
			if ( Instance.MainContainer.GetChildCount() > 0 ) {
				Instance.MainContainer.AddChild( new HSeparator() );
			}

			Button button = new Button();
			button.Text = TranslationServer.Translate( translationString );
			button.Flat = true;
			button.Connect( Button.SignalName.Pressed, callback );
			Instance.MainContainer.AddChild( button );
		}
		public static void EndInteraction() {
			Instance.Hide();
			LevelData.Instance.ThisPlayer.BlockInput( false );
		}
		public static void StartInteraction() {
			Godot.Collections.Array<Node> children = Instance.MainContainer.GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				Instance.MainContainer.RemoveChild( children[ i ] );
				children[ i ].QueueFree();
			}
			Instance.Show();

			LevelData.Instance.ThisPlayer.BlockInput( true );
		}

		public override void _Ready() {
			base._Ready();

			MainContainer = GetNode<VBoxContainer>( "OptionList" );

			Instance = this;
		}
	};
};