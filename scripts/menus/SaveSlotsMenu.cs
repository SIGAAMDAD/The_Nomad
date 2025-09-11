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
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Menus {
	public partial class SaveSlotsMenu : Control {
		private static readonly StringName @IndexMetaName = "index";

		private Button DeleteSaveButton;
		private VBoxContainer SlotsContainer;

		private Button[]? SaveSlots = null;
		private int SelectedSlot = 0;

		[Signal]
		public delegate void BeginGameEventHandler();

		/*
		===============
		OnContinueGameFinished
		===============
		*/
		private void OnContinueGameFinished() {
			Console.PrintLine( "Loading game..." );

			TransitionScreen.TransitionFinished -= OnContinueGameFinished;

			Hide();
			GetNode<LoadingScreen>( "/root/LoadingScreen" ).FadeIn( "res://levels/world.tscn" );
			GameConfiguration.SetGameMode( GameMode.SinglePlayer );

			World.LoadTime = System.Diagnostics.Stopwatch.StartNew();
		}

		/*
		===============
		TransitionToLoadingScreen
		===============
		*/
		private void TransitionToLoadingScreen() {
			UIAudioManager.OnActivate();

			Hide();
			TransitionScreen.TransitionFinished += OnContinueGameFinished;
			TransitionScreen.Transition();

			UIAudioManager.FadeMusic();
		}

		/*
		===============
		OnContinueGameButtonPressed
		===============
		*/
		private void OnContinueGameButtonPressed() {
			if ( MainMenu.Loaded ) {
				return;
			}
			MainMenu.Loaded = true;

			EmitSignalBeginGame();
			TransitionToLoadingScreen();
		}

		/*
		===============
		OnSaveSlotButtonPressed
		===============
		*/
		private void OnSaveSlotButtonPressed( Button SaveSlot ) {
			if ( MainMenu.Loaded ) {
				return;
			}
			MainMenu.Loaded = true;

			EmitSignalBeginGame();

			SettingsData.SetSaveSlot( SaveSlot.GetMeta( IndexMetaName ).AsInt32() );

			TransitionToLoadingScreen();
		}

		/*
		===============
		ConnectButton
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="button"></param>
		/// <param name="pressedCallback"></param>
		private void ConnectButton( Button button, Action pressedCallback ) {
			GameEventBus.ConnectSignal( button, Button.SignalName.FocusEntered, this, UIAudioManager.OnButtonFocusedCallable );
			GameEventBus.ConnectSignal( button, Button.SignalName.MouseEntered, this, UIAudioManager.OnButtonFocusedCallable );
			GameEventBus.ConnectSignal( button, Button.SignalName.Pressed, this, pressedCallback );
		}

		/*
		===============
		ConnectSlotButton
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="button"></param>
		/// <param name="pressedCallback"></param>
		private void ConnectSlotButton( Button button ) {
			GameEventBus.ConnectSignal( button, Button.SignalName.FocusEntered, this, () => OnButtonFocused( button ) );
			GameEventBus.ConnectSignal( button, Button.SignalName.MouseEntered, this, () => OnButtonFocused( button ) );
			GameEventBus.ConnectSignal( button, Button.SignalName.Pressed, this, () => OnSaveSlotButtonPressed( button ) );
		}

		/*
		===============
		LoadSaveSlots
		===============
		*/
		private void LoadSaveSlots() {
			int i;
			List<string> saveSlots = new List<string>();
			Godot.Collections.Array<Node> children;

			Utils.Methods.LoadFileList( "user://SaveData", saveSlots );
			Console.PrintLine( $"...found {saveSlots.Count} save slots" );

			// release all the children first
			children = SlotsContainer.GetChildren();
			for ( i = 0; i < children.Count; i++ ) {
				children[ i ].CallDeferred( MethodName.QueueFree );
				SlotsContainer.RemoveChild( children[ i ] );
			}
			for ( i = 0; i < saveSlots.Count; i++ ) {
				Button label = new Button() {
					Text = saveSlots[ i ],
					Theme = this.Theme,
					Visible = true
				};
				ConnectSlotButton( label );
				label.SetMeta( IndexMetaName, i );
				SlotsContainer.AddChild( label );
			}

			// link focus navigation
			children = SlotsContainer.GetChildren();
			for ( i = 0; i < children.Count; i++ ) {
				if ( children[ i ] is Button button && button != null ) {
					if ( i == 0 ) {
						button.FocusNeighborTop = children.Last().GetPath();
						button.FocusNeighborBottom = children[ i + 1 ].GetPath();
					} else if ( i == children.Count - 1 ) {
						button.FocusNeighborTop = children[ i - 1 ].GetPath();
						button.FocusNeighborBottom = children.First().GetPath();
					} else {
						button.FocusNeighborTop = children[ i - 1 ].GetPath();
						button.FocusNeighborBottom = children[ i + 1 ].GetPath();
					}
				}
			}
		}

		/*
		===============
		OnDeleteSaveButtonPressed
		===============
		*/
		private void OnDeleteSaveButtonPressed() {
			ArchiveSystem.DeleteSave( SelectedSlot );
			SelectedSlot = 0;

			LoadSaveSlots();
		}

		/*
		===============
		OnButtonFocused
		===============
		*/
		private void OnButtonFocused( Button button ) {
			UIAudioManager.OnButtonFocused( button );

			if ( SaveSlots[ SelectedSlot ] != button ) {
				SelectedSlot = button.GetMeta( IndexMetaName ).AsInt32();
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			Theme = SettingsData.DyslexiaMode ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

			ConfirmationDialog deleteSaveConfirm = GetNode<ConfirmationDialog>( "DeleteSaveConfirmation" );
			GameEventBus.ConnectSignal( deleteSaveConfirm, ConfirmationDialog.SignalName.Canceled, this, Hide );
			GameEventBus.ConnectSignal( deleteSaveConfirm, ConfirmationDialog.SignalName.Confirmed, this, OnDeleteSaveButtonPressed );

			DeleteSaveButton = GetNode<Button>( "DeleteSaveButton" );
			Methods.ConnectMenuButton( DeleteSaveButton, this, DeleteSaveButton.Hide );

			SlotsContainer = GetNode<VBoxContainer>( "VScrollBar/SaveSlotsContainer" );

			LoadSaveSlots();
		}
	};
};