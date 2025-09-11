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
using System.Runtime.CompilerServices;

namespace Menus.SelectionNodes {
	/*
	===================================================================================
	
	OptionList
		
	===================================================================================
	*/

	public partial class OptionList : HBoxContainer {
		[Export]
		public StringName Title { get; private set; }
		[Export]
		public StringName Description { get; private set; }
		[Export]
		public bool NoTranslate { get; private set; }
		[Export]
		public StringName[] Items { get; private set; }

		public int Value { get; private set; }

		private string DescriptionCached;
		private Godot.Label ValueLabel;
		private SelectionNodes.Label TitleLabel;

		public event Action<int> ValueChanged;

		/*
		===============
		GetDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string GetDescription() {
			return Description.IsEmpty ? "" : DescriptionCached;
		}

		/*
		===============
		SetValue
		===============
		*/
		public void SetValue( int value ) {
			Value = value;
			ValueLabel.Text = NoTranslate ? Items[ Value ] : TranslationServer.Translate( Items[ Value ] );
			ValueChanged?.Invoke( Value );
		}

		/*
		===============
		OnLeftIconPressed
		===============
		*/
		private void OnLeftIconPressed() {
			if ( Value == 0 ) {
				SetValue( Items.Length - 1 );
			} else {
				SetValue( Value - 1 );
			}
			UIAudioManager.OnButtonPressed();
		}

		/*
		===============
		OnRightIconPressed
		===============
		*/
		private void OnRightIconPressed() {
			if ( Value == Items.Length - 1 ) {
				SetValue( 0 );
			} else {
				SetValue( Value + 1 );
			}
			UIAudioManager.OnButtonPressed();
		}

		/*
		===============
		DisableMouseFocus
		===============
		*/
		private void DisableMouseFocus() {
			Control focusOwner = GetViewport().GuiGetHoveredControl();
			if ( focusOwner != null && focusOwner is HBoxContainer option ) {
				option.Call( "OnUnfocused" );
			}
		}

		/*
		===============
		OnFocused
		===============
		*/
		private void OnFocused() {
			DisableMouseFocus();
			TitleLabel.StartStrobe();

			UIAudioManager.OnButtonFocused( this );
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		private void OnUnfocused() {
			TitleLabel.StopStrobe();

			UIAudioManager.OnButtonUnfocused( this );
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			ValueLabel = GetNode<Godot.Label>( "Value" );

			TitleLabel = GetNode<SelectionNodes.Label>( "Title" );
			TitleLabel.Text = TranslationServer.Translate( Title );
			DescriptionCached = TranslationServer.Translate( Description );

			GameEventBus.ConnectSignal( this, HBoxContainer.SignalName.FocusEntered, this, OnFocused );
			GameEventBus.ConnectSignal( this, HBoxContainer.SignalName.MouseEntered, this, OnFocused );
			GameEventBus.ConnectSignal( this, HBoxContainer.SignalName.FocusExited, this, OnUnfocused );
			GameEventBus.ConnectSignal( this, HBoxContainer.SignalName.MouseExited, this, OnUnfocused );

			GameEventBus.ConnectSignal( GetNode<Godot.Button>( "LeftIcon" ), Button.SignalName.Pressed, this, OnLeftIconPressed );
			GameEventBus.ConnectSignal( GetNode<Godot.Button>( "RightIcon" ), Button.SignalName.Pressed, this, OnRightIconPressed );
		}

		/*
		===============
		_ExitTree
		===============
		*/
		public override void _ExitTree() {
			base._ExitTree();

			GameEventBus.ReleaseDanglingDelegates( ValueChanged );
		}
	};
};