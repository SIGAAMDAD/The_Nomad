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

namespace Interactables {
	/*
	===================================================================================
	
	MechanicalTutorial
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class MechanicTutorial : InteractionItem {
		[Export]
		private string? TutorialString;

		public override InteractionType InteractionType => InteractionType.Tutorial;

		private TextureRect? Background;
		private RichTextLabel? InteractionPrompt;
		private Callable Callback;

		private Resource? DialogueResource;

		/*
		===============
		OnInteract
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		private void OnInteract( Player player ) {
			InteractionPrompt.Hide();
			DialogueManager.ShowDialogueBalloon( DialogueResource );
			player.Disconnect( Player.SignalName.Interaction, Callback );
		}

		/*
		===============
		OnInteractionAreaBody2DEntered
		===============
		*/
		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				Callback = Callable.From( () => OnInteract( player ) );
				InteractionPrompt.Show();
				player.Connect( Player.SignalName.Interaction, Callback );
				player.EmitSignal( Player.SignalName.ShowInteraction, this );
			}
		}

		/*
		===============
		OnInteractionAreaBody2DExited
		===============
		*/
		protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				InteractionPrompt.Hide();
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

			DialogueResource = DialogueManager.CreateResourceFromText( string.Format( "~ start\nTUTORIAL: {0}", TutorialString ) );

			InteractionPrompt = GetNode<RichTextLabel>( "RichTextLabel" );
			LevelData.Instance.ThisPlayer.InputMappingContextChanged += () =>
				InteractionPrompt.ParseBbcode( AccessibilityManager.GetBindString(
					LevelData.Instance.ThisPlayer.InputManager.GetBindActionResource( PlayerSystem.Input.InputController.ControlBind.Interact )
				) );

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
		}
	};
};