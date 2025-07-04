/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

/*
using Godot;

namespace PlayerSystem {
	public partial class CheckpointInteractor : MarginContainer {
		private static readonly float RageUsedOnWarp = 20.0f;

		private Player _Owner;

		private Label CheckpointNameLabel;

		//
		// InactiveContainer
		//
		private Button ActivateButton;

		//
		// MainContainer
		//
		private Button RestHereButton;
		private Button OpenStorageButton;

		//
		// RestingContainer
		//
		private Button LeaveButton;
		private Button WarpButton;
		private Button ReflectButton;

		//
		// WarpLocations
		//
		private WarpPoint WarpCloner;
		private VScrollBar WarpLocationsScroll;
		private VBoxContainer WarpLocationsContainer;
		private Button BackButton;

		private HBoxContainer MemoryCloner;

		private VBoxContainer InactiveContainter;
		private VBoxContainer CheckpointMainContainer;
		private VBoxContainer SavedGamesContainer;
		private VBoxContainer RestingContainer;

		private Checkpoint CurrentCheckpoint;

		public override void _Ready() {
			base._Ready();

			_Owner = GetParent<HeadsUpDisplay>().GetPlayerOwner();
			
			WarpCloner = GetNode<WarpPoint>( "VBoxContainer/MarginContainer/WarpLocations/WarpLocationsContainer/Cloner" );
			CheckpointNameLabel = GetNode<Label>( "VBoxContainer/CheckpointNameLabel" );

			RestHereButton = GetNode<Button>( "VBoxContainer/MarginContainer/MainContainer/RestHereButton" );
			RestHereButton.Connect( "pressed", Callable.From( OnRestHereButtonPressed ) );

			ActivateButton = GetNode<Button>( "VBoxContainer/MarginContainer/InactiveContainer/ActivateButton" );
			ActivateButton.SetProcess( false );
			ActivateButton.SetProcessInternal( false );
			ActivateButton.Connect( "pressed", Callable.From( () => {
				CurrentCheckpoint.Activate();
				GetParent<HeadsUpDisplay>().ShowAnnouncement( "ACQUIRED_MEMORY" );
			} ) );

			MemoryCloner = GetNode<HBoxContainer>( "VBoxContainer/MarginContainer/SavedGamesContainer/Cloner" );
			InactiveContainter = GetNode<VBoxContainer>( "VBoxContainer/MarginContainer/InactiveContainer" );
			CheckpointMainContainer = GetNode<VBoxContainer>( "VBoxContainer/MarginContainer/MainContainer" );
			SavedGamesContainer = GetNode<VBoxContainer>( "VBoxContainer/MarginContainer/SavedGamesContainer" );
			WarpLocationsScroll = GetNode<VScrollBar>( "VBoxContainer/MarginContainer/WarpLocations" );
			WarpLocationsContainer = GetNode<VBoxContainer>( "VBoxContainer/MarginContainer/WarpLocations/WarpLocationsContainer" );

			RestingContainer = GetNode<VBoxContainer>( "VBoxContainer/MarginContainer/RestingContainer" );

			LeaveButton = RestingContainer.GetNode<Button>( "LeaveButton" );
			LeaveButton.Connect( "pressed", Callable.From( OnCheckpointLeave ) );

			WarpButton = RestingContainer.GetNode<Button>( "WarpButton" );
			WarpButton.Connect( "pressed", Callable.From( OnWarpButtonPressed ) );

			BackButton = WarpLocationsScroll.GetNode<Button>( "BackButton" );
			BackButton.Connect( "pressed", Callable.From( OnBackButtonPressed ) );
		}

		private void OnBackButtonPressed() {
			WarpLocationsScroll.Hide();
			RestingContainer.Show();
		}

		public void BeginInteraction( InteractionItem item ) {
			CurrentCheckpoint = item as Checkpoint;
			if ( CurrentCheckpoint == null ) {
				Console.PrintError( "CheckpointInteractor.BeginInteraction: invalid checkpoint given!" );
				return;
			}

			CheckpointNameLabel.Text = CurrentCheckpoint.GetTitle();
			if ( CurrentCheckpoint.GetActivated() ) {
				InactiveContainter.Hide();
				CheckpointMainContainer.Show();
			} else {
				InactiveContainter.Show();
				CheckpointMainContainer.Hide();
			}
		}
		public void EndInteraction() {
		}

		private void OnCheckpointLeave() {
			Hide();

			_Owner.LeaveCampfire();

			CheckpointMainContainer.Show();
			WarpLocationsScroll.Hide();
			RestingContainer.Hide();
			SavedGamesContainer.Hide();
		}

		private void OnWarpToCheckpoint( WarpPoint warpPoint ) {
			// TODO: confirmation screen

			if ( _Owner.GetRage() - RageUsedOnWarp < 0.0f ) {
				HeadsUpDisplay.StartThoughtBubble( "You: Shit, don't have enough mana." );
				return;
			} else if ( (Checkpoint)warpPoint.GetMeta( "Checkpoint" ) == CurrentCheckpoint ) {
				HeadsUpDisplay.StartThoughtBubble( "You: I'm already here..." );
				return;
			}

			// TODO: play warp animation
			_Owner.SetRage( _Owner.GetRage() - RageUsedOnWarp );

			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
			_Owner.GlobalPosition = ( (Checkpoint)warpPoint.GetMeta( "Checkpoint" ) ).GlobalPosition;
		}
		private void LoadWarpPoints() {
			Godot.Collections.Array<Node> checkpoints = GetTree().GetNodesInGroup( "Checkpoints" );
			for ( int i = 0; i < checkpoints.Count; i++ ) {
				Checkpoint checkpoint = checkpoints[i] as Checkpoint;
				if ( !checkpoint.GetActivated() ) {
					continue;
				}

				WarpPoint warpPoint = WarpCloner.Duplicate() as WarpPoint;
				WarpLocationsContainer.AddChild( warpPoint );
				warpPoint.ConfirmButton.Text = checkpoint.GetTitle();
				warpPoint.ConfirmButton.Connect( "pressed", Callable.From( () => { OnWarpToCheckpoint( warpPoint ); } ) );
				warpPoint.BiomeLabel.Text = checkpoint.GetLocation().GetBiome().GetAreaName();
				warpPoint.SetMeta( "Checkpoint", checkpoint );
				warpPoint.Show();
			}
		}

		private void OnWarpButtonPressed() {
			CheckpointMainContainer.Hide();
			RestingContainer.Hide();

			for ( int i = 1; i < WarpLocationsContainer.GetChildCount(); i++ ) {
				WarpLocationsContainer.CallDeferred( "remove_child", WarpLocationsContainer.GetChild( i ) );
				WarpLocationsContainer.GetChild( i ).CallDeferred( "queue_free" );
			}

			CallDeferred( "LoadWarpPoints" );

			WarpLocationsScroll.CallDeferred( "show" );
		}

		public Checkpoint GetCurrentCheckpoint() => CurrentCheckpoint;
		private void OnRestHereButtonPressed() {
			_Owner.SetHealth( 100.0f );
			_Owner.SetRage( 100.0f );

			_Owner.RestAtCampfire();

			CheckpointMainContainer.Hide();
			RestingContainer.Show();

			ArchiveSystem.SaveGame( SettingsData.GetSaveSlot() );
		}
	};
};
*/