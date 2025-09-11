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

using DialogueManagerRuntime;
using Godot;
using PlayerSystem.UserInterface;
using Renown.Thinkers;

namespace Interactables {
	/*
	===================================================================================
	
	DialogueInteractor
	
	===================================================================================
	*/
	
	public partial class DialogueInteractor : InteractionItem {
		[Export]
		public Resource DialogueResource { get; private set; }
		[Export]
		private Godot.Collections.Array<StringName> OptionNames;
		[Export]
		private Thinker _Owner;
		[Export]
		private bool HasStartInteractionLine = false;

		public override InteractionType InteractionType => InteractionType.Dialogue;

		private RichTextLabel Text;
		private Callable Callback;

		[Signal]
		public delegate void DialogueOptionSelectedEventHandler( int nOptionSelected );
		[Signal]
		public delegate void BeginDialogueEventHandler();
		[Signal]
		public delegate void EndDialogueEventHandler();

		/*
		===============
		RemoveDialogueOption
		===============
		*/
		public void RemoveDialogueOption( StringName optionName ) {
			OptionNames.Remove( optionName );
		}

		/*
		===============
		AddDialogueOption
		===============
		*/
		public void AddDialogueOption( StringName optionName ) {
			OptionNames.Add( optionName );
		}

		/*
		===============
		AddDialogueOptions
		===============
		*/
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

		/*
		===============
		OnInteract
		===============
		*/
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

		/*
		===============
		OnInteractionAreaBody2DEntered
		===============
		*/
		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				Callback = Callable.From( () => OnInteract( player ) );
				Text.Show();
				player.Connect( Player.SignalName.Interaction, Callback );
			}
		}

		/*
		===============
		OnInteractionAreaBody2DExited
		===============
		*/
		protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				Text.Hide();
				if ( player.IsConnected( Player.SignalName.Interaction, Callback ) ) {
					player.Disconnect( Player.SignalName.Interaction, Callback );
				}
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			Text = GetNode<RichTextLabel>( "RichTextLabel" );
			LevelData.Instance.ThisPlayer.InputMappingContextChanged += () =>
				Text.ParseBbcode( AccessibilityManager.GetBindString(
					LevelData.Instance.ThisPlayer.InputManager.GetBindActionResource( Player.InputController.ControlBind.Interact )
				) );

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
		}
	};
};