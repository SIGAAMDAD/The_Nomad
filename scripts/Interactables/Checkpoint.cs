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
using Renown.World;

namespace Interactables {

	/*
	===================================================================================

	Checkpoint

	===================================================================================
	*/
	/// <summary>
	/// Class implementation for a Meliora
	/// </summary>

	public partial class Checkpoint : InteractionItem {
		[Export]
		public StringName Title { get; private set; }
		[Export]
		public WorldArea Location { get; private set; }

		public override InteractionType InteractionType => InteractionType.Checkpoint;
		public bool Activated { get; private set; }

		private AnimatedSprite2D Animations;
		private AnimatedSprite2D ActivateAnimation;
		private AudioStreamPlayer2D AudioChannel;

		private Callable Callback;
		private RichTextLabel Text;

		/*
		================
		Activate
		===============
		*/
		public void Activate( Player player ) {
			if ( !Activated ) {
				Activated = true;
				Animations.Hide();
				ActivateAnimation.Show();
				ActivateAnimation.Play( "default" );
			}
			player.BeginInteraction( this );
			player.Disconnect( Player.SignalName.Interaction, Callback );
			Text.Hide();
		}

		/*
		================
		Save
		===============
		*/
		public void Save() {
			using ( var writer = new SaveSystem.SaveSectionWriter( GetPath(), ArchiveSystem.SaveWriter ) ) {
				writer.SaveBool( nameof( Activated ), Activated );
			}
		}

		/*
		================
		Load
		===============
		*/
		public void Load() {
			SaveSystem.SaveSectionReader? reader = ArchiveSystem.GetSection( GetPath() );

			// save file compatibility
			if ( reader == null ) {
				return;
			}

			Activated = reader.LoadBoolean( "Activated" );
			if ( Activated ) {
				Animations.Play( "activated" );
			}
		}

		/*
		================
		OnInteractionAreaBody2DEntered
		===============
		*/
		protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				Callback = Callable.From( () => Activate( player ) );
				Text.Show();
				player.Connect( Player.SignalName.Interaction, Callback );
			}
		}

		/*
		================
		OnInteractionAreaBody2DExited
		===============
		*/
		protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Player player && player != null ) {
				Text.Hide();
				if ( player.IsConnected( Player.SignalName.Interaction, Callback ) ) {
					player.Disconnect( Player.SignalName.Interaction, Callback );
				}
				player.EndInteraction();
			}
		}

		/*
		================
		OnActivateAnimationFinished
		===============
		*/
		private void OnActivateAnimationFinished() {
			ActivateAnimation.Hide();
			RemoveChild( ActivateAnimation );
			ActivateAnimation.QueueFree();
			Animations.Play( "activated" );
			Animations.Show();
			Text.Hide();
		}

		/*
		================
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			Activated = false;
			ProcessMode = ProcessModeEnum.Pausable;

			Animations = GetNode<AnimatedSprite2D>( "AnimatedSprite2D" );

			VisibleOnScreenEnabler2D enabler = GetNode<VisibleOnScreenEnabler2D>( "VisibleOnScreenEnabler2D" );
			enabler.Connect( VisibleOnScreenEnabler2D.SignalName.ScreenEntered, Callable.From( Animations.Show ) );
			enabler.Connect( VisibleOnScreenEnabler2D.SignalName.ScreenExited, Callable.From( Animations.Hide ) );

			ActivateAnimation = GetNode<AnimatedSprite2D>( "ActivateAnimation" );
			ActivateAnimation.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( OnActivateAnimationFinished ) );

			Text = GetNode<RichTextLabel>( "RichTextLabel" );
			LevelData.Instance.ThisPlayer.InputMappingContextChanged += () =>
				Text.ParseBbcode( AccessibilityManager.GetBindString(
					LevelData.Instance.ThisPlayer.InputManager.GetBindActionResource( PlayerSystem.Input.InputController.ControlBind.Interact )
				) );

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

			if ( !IsInGroup( "Archive" ) ) {
				AddToGroup( "Archive" );
			}
			if ( ArchiveSystem.IsLoaded() ) {
				Load();
			}
		}
	};
};