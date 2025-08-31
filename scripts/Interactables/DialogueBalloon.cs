/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;
using DialogueManagerRuntime;

/*
===================================================================================

DialogueBalloon

===================================================================================
*/

public partial class DialogueBalloon : CanvasLayer {
	[Export]
	public string NextAction { get; private set; } = "ui_accept";
	[Export]
	public string SkipAction { get; private set; } = "ui_cancel";

	public DialogueLine DialogueLine {
		get => _dialogueLine;
		set {
			if ( value == null ) {
				QueueFree();
				return;
			}

			_dialogueLine = value;
			ApplyDialogueLine();
		}
	}

	private Control Balloon;
	private RichTextLabel CharacterLabel;
	private RichTextLabel dialogueLabel;
	private VBoxContainer responsesMenu;

	private Resource Resource;
	private Godot.Collections.Array<Variant> TemporaryGameStates = new Godot.Collections.Array<Variant>();
	private bool IsWaitingForInput = false;
	private bool WillHideBalloon = false;

	private DialogueLine _dialogueLine;
	private Timer MutationCooldown = new Timer();


	public override void _Ready() {
		Balloon = GetNode<Control>( "%Balloon" );
		CharacterLabel = GetNode<RichTextLabel>( "%CharacterLabel" );
		dialogueLabel = GetNode<RichTextLabel>( "%DialogueLabel" );
		responsesMenu = GetNode<VBoxContainer>( "%ResponsesMenu" );

		Balloon.Hide();

		Balloon.GuiInput += ( @event ) => {
			if ( (bool)dialogueLabel.Get( "is_typing" ) ) {
				bool mouseWasClicked = @event is InputEventMouseButton && ( @event as InputEventMouseButton ).ButtonIndex == MouseButton.Left && @event.IsPressed();
				bool skipButtonWasPressed = @event.IsActionPressed( SkipAction );
				if ( mouseWasClicked || skipButtonWasPressed ) {
					GetViewport().SetInputAsHandled();
					dialogueLabel.Call( "skip_typing" );
					return;
				}
			}

			if ( !IsWaitingForInput ) {
				return;
			}
			if ( _dialogueLine.Responses.Count > 0 ) {
				return;
			}

			GetViewport().SetInputAsHandled();

			if ( @event is InputEventMouseButton && @event.IsPressed() && ( @event as InputEventMouseButton ).ButtonIndex == MouseButton.Left ) {
				Next( _dialogueLine.NextId );
			} else if ( @event.IsActionPressed( NextAction ) && GetViewport().GuiGetFocusOwner() == Balloon ) {
				Next( _dialogueLine.NextId );
			}
		};

		if ( string.IsNullOrEmpty( (string)responsesMenu.Get( "next_action" ) ) ) {
			responsesMenu.Set( "next_action", NextAction );
		}
		responsesMenu.Connect( "response_selected", Callable.From( ( DialogueResponse response ) => {
			Next( response.NextId );
		} ) );


		// Hide the balloon when a mutation is running
		MutationCooldown.Timeout += () => {
			if ( WillHideBalloon ) {
				WillHideBalloon = false;
				Balloon.Hide();
			}
		};
		AddChild( MutationCooldown );

		DialogueManager.Mutated += OnMutated;

		LevelData.Instance.ThisPlayer.BlockInput( true );
	}


	public override void _ExitTree() {
		DialogueManager.Mutated -= OnMutated;

		LevelData.Instance.ThisPlayer.BlockInput( false );
	}


	public override void _UnhandledInput( InputEvent @event ) {
		// Only the balloon is allowed to handle input while it's showing
		GetViewport().SetInputAsHandled();
	}


	public override async void _Notification( int what ) {
		// Detect a change of locale and update the current dialogue line to show the new language
		if ( what == NotificationTranslationChanged && IsInstanceValid( dialogueLabel ) ) {
			float visibleRatio = dialogueLabel.VisibleRatio;
			DialogueLine = await DialogueManager.GetNextDialogueLine( Resource, DialogueLine.Id, TemporaryGameStates );
			if ( visibleRatio < 1.0f ) {
				dialogueLabel.Call( "skip_typing" );
			}
		}
	}


	public async void Start( Resource dialogueResource, string title, Godot.Collections.Array<Variant> extraGameStates = null ) {
		TemporaryGameStates = new Godot.Collections.Array<Variant> { this } + ( extraGameStates ?? new Godot.Collections.Array<Variant>() );
		IsWaitingForInput = false;
		Resource = dialogueResource;

		DialogueLine = await DialogueManager.GetNextDialogueLine( Resource, title, TemporaryGameStates );
	}


	public async void Next( string nextId ) {
		DialogueLine = await DialogueManager.GetNextDialogueLine( Resource, nextId, TemporaryGameStates );
	}

	private async void ApplyDialogueLine() {
		MutationCooldown.Stop();

		IsWaitingForInput = false;
		Balloon.FocusMode = Control.FocusModeEnum.All;
		Balloon.GrabFocus();

		// Set up the character name
		CharacterLabel.Visible = !string.IsNullOrEmpty( _dialogueLine.Character );
		CharacterLabel.Text = Tr( _dialogueLine.Character, "dialogue" );

		// Set up the dialogue
		dialogueLabel.Hide();
		dialogueLabel.Set( "dialogue_line", _dialogueLine );

		// Set up the responses
		responsesMenu.Hide();
		responsesMenu.Set( "responses", _dialogueLine.Responses );

		// Type out the text
		Balloon.Show();
		WillHideBalloon = false;
		dialogueLabel.Show();
		if ( !string.IsNullOrEmpty( _dialogueLine.Text ) ) {
			dialogueLabel.Call( "type_out" );
			await ToSignal( dialogueLabel, "finished_typing" );
		}

		if ( _dialogueLine.Responses.Count > 0 ) {
			Balloon.FocusMode = Control.FocusModeEnum.None;
			responsesMenu.Show();
		} else if ( !string.IsNullOrEmpty( _dialogueLine.Time ) ) {
			if ( !float.TryParse( _dialogueLine.Time, out float time ) ) {
				time = _dialogueLine.Text.Length * 0.02f;
			}
			await ToSignal( GetTree().CreateTimer( time ), "timeout" );
			Next( _dialogueLine.NextId );
		} else {
			IsWaitingForInput = true;
			Balloon.FocusMode = Control.FocusModeEnum.All;
			Balloon.GrabFocus();
		}
	}
	private void OnMutated( Godot.Collections.Dictionary _mutation ) {
		IsWaitingForInput = false;
		WillHideBalloon = true;
		MutationCooldown.Start( 0.1f );
	}
};