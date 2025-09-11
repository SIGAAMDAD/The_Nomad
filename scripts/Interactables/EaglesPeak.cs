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

namespace Interactables {
	public partial class EaglesPeak : InteractionItem {
		[Export]
		private Texture2D ViewImage;
		[Export]
		private AudioStream Music;
		[Export]
		private Checkpoint Destination;

		public override InteractionType InteractionType => InteractionType.EaglesPeak;

		private Player Interactor = null;

		public Texture2D GetViewImage() => ViewImage;
		public AudioStream GetMusic() => Music;

		private void OnTransitionFinished() {
			RemoveChild( GetChild( GetChildCount() - 1 ) );
		}
		public void OnYesButtonPressed() {
			/*
			AudioStreamPlayer audio = new AudioStreamPlayer();
			Interactor.AddChild( audio );
			audio.Stream = ResourceCache.LeapOfFaithSfx;
			audio.Connect( AudioStreamPlayer.SignalName.Finished, Callable.From( () => {
				Interactor.RemoveChild( audio );
				audio.QueueFree();
			} ) );
			audio.Play();
			*/

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnTransitionFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

			Interactor.GlobalPosition = Destination.GlobalPosition;
		}
		public void OnNoButtonPressed() {
			Interactor.EndInteraction();
		}

		protected override void OnInteractionAreaBody2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			Interactor = body as Player;
			if ( Interactor == null ) {
				return;
			}
			Interactor.BeginInteraction( this );
		}
		protected override void OnInteractionAreaBody2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is not Player player ) {
				return;
			}
			player.EndInteraction();
		}
		public override void _Ready() {
			base._Ready();

			Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
			Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
		}
	};
};
