using DialogueManagerRuntime;
using Godot;
using PlayerSystem.UserInterface;
using Renown.Thinkers;
using System.Linq;

public partial class DialogueInteractor : InteractionItem {
	[Export]
	public Resource DialogueResource { get; private set; }
	[Export]
	private Godot.Collections.Array<StringName> OptionNames;
	[Export]
	private Thinker _Owner;
	[Export]
	private bool HasStartInteractionLine = false;

	private RichTextLabel Text;
	private Callable Callback;

	[Signal]
	public delegate void DialogueOptionSelectedEventHandler( int nOptionSelected );
	[Signal]
	public delegate void BeginDialogueEventHandler();
	[Signal]
	public delegate void EndDialogueEventHandler();

	public void RemoveDialogueOption( StringName optionName ) => OptionNames.Remove( optionName );
	public void AddDialogueOption( StringName optionName ) => OptionNames.Add( optionName );

	private void AddDialogueOptions( Resource dialogueResource ) {
		DialogueContainer.StartInteraction();
		for ( int i = 0; i < OptionNames.Count; i++ ) {
			int index = i;
			DialogueContainer.AddOption( OptionNames[ i ], Callable.From( () => EmitSignalDialogueOptionSelected( index ) ) );
		}
		if ( HasStartInteractionLine ) {
			DialogueManager.DialogueEnded -= AddDialogueOptions;
		}
	}
	private void OnInteract( Player player ) {
		if ( HasStartInteractionLine ) {
			DialogueManager.ShowDialogueBalloon( DialogueResource, "start" );
			DialogueManager.DialogueEnded += AddDialogueOptions;
		} else {
			AddDialogueOptions( null );
		}

		Text.Hide();
		player.Disconnect( Player.SignalName.Interaction, Callback );
		EmitSignalBeginDialogue();
	}

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Callback = Callable.From( () => OnInteract( player ) );
			Text.Show();
			player.Connect( Player.SignalName.Interaction, Callback );
		}
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Text.Hide();
			if ( player.IsConnected( Player.SignalName.Interaction, Callback ) ) {
				player.Disconnect( Player.SignalName.Interaction, Callback );
			}
		}
	}
	public override InteractionType GetInteractionType() => InteractionType.Dialogue;

	public override void _Ready() {
		base._Ready();

		Text = GetNode<RichTextLabel>( "RichTextLabel" );
		LevelData.Instance.ThisPlayer.InputMappingContextChanged += () => Text.ParseBbcode( AccessibilityManager.GetBindString( LevelData.Instance.ThisPlayer.InteractAction ) );

		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};