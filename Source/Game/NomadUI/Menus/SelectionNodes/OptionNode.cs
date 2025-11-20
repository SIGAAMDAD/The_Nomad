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

using EventSystem;
using Godot;
using System;
using System.Runtime.CompilerServices;

namespace Menus.SelectionNodes {
	/*
	===================================================================================
	
	OptionNode
	
	===================================================================================
	*/
	/// <summary>
	/// The base class for all <see cref="HBoxContainer"/> based option nodes in the settings menu.
	/// </summary>
	/// <typeparam name="T">The type being used to store the data. Should be a float, int32, or a bool.</typeparam>

	public partial class OptionNode : HBoxContainer {
		public readonly struct ValueChangedEventData : IEventArgs {
			public readonly object Value;

			/*
			===============
			ValueChangedEventData
			===============
			*/
			public ValueChangedEventData( object value ) {
				Value = value;
			}
		};

		[Export]
		public StringName Title { get; private set; }
		[Export]
		public StringName Description { get; private set; }

		public virtual object Value { get; protected set; }

		protected Godot.Label ValueLabel;
		protected SelectionNodes.Label TitleLabel;

		private string? DescriptionCached;

		public readonly UIEvent ValueChanged = null;

		/*
		===============
		OptionNode
		===============
		*/
		public OptionNode() {
			ValueChanged = new UIEvent( this, nameof( ValueChanged ) );
		}

		/*
		===============
		GetDescription
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public string? GetDescription() {
			return Description.IsEmpty ? "" : DescriptionCached;
		}

		/*
		===============
		SetValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public virtual void SetValue( object value ) {
			Value = value;
			ValueChanged.Publish( new ValueChangedEventData( value ) );
		}

		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void OnFocused() {
			DisableMouseFocus();
			GrabClickFocus();

			//UIAudioManager.OnButtonFocused( this );
		}

		/*
		===============
		OnUnfocused
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void OnUnfocused() {
			//UIAudioManager.OnButtonUnfocused( this );
		}

		/*
		===============
		DisableMouseFocus
		===============
		*/
		private void DisableMouseFocus() {
			Control focusOwner = GetViewport().GuiGetHoveredControl();
			if ( focusOwner != null && focusOwner is OptionNode option ) {
				option.OnUnfocused();
			}
		}

		/*
		===============
		LinkFocusNodes
		===============
		*/
		private void LinkFocusNodes() {
			FocusNeighborLeft = GetPath();
			FocusNeighborRight = GetPath();

			int index = GetIndex();
			Node parent = GetParent();
			int childCount = parent.GetChildCount();
			if ( index == 0 ) {
				FocusNeighborTop = parent.GetChild( childCount - 1 ).GetPath();
				FocusNeighborBottom = parent.GetChild( index + 1 ).GetPath();
			} else if ( index == childCount - 1 ) {
				FocusNeighborTop = parent.GetChild( index - 1 ).GetPath();
				FocusNeighborBottom = parent.GetChild( 0 ).GetPath();
			} else {
				FocusNeighborTop = parent.GetChild( index - 1 ).GetPath();
				FocusNeighborBottom = parent.GetChild( index + 1 ).GetPath();
			}
			FocusNeighborLeft = GetPath();
			FocusNeighborRight = GetPath();
			FocusNext = GetPath();
			FocusPrevious = GetPath();

			FocusMode = FocusModeEnum.All;
		}

		/*
		===============
		BindNodes
		===============
		*/
		/// <summary>
		/// Binds the relevant nodes associated with this <see cref="OptionNode{T}"/>.
		/// </summary>
		protected virtual void BindNodes() {
			if ( Title.IsEmpty ) {
				ConsoleSystem.Console.PrintError( $"OptionNode.BindNodes: Title StringName is empty for {Name}!" );
			}
			if ( Description != null && Description.IsEmpty ) {
				ConsoleSystem.Console.PrintError( $"OptionNode.BindNodes: Description StringName is empty for {Name}!" );
			}

			TitleLabel = GetNode<SelectionNodes.Label>( "Title" );
			TitleLabel.Text = TranslationServer.Translate( Title );

			DescriptionCached = TranslationServer.Translate( Description );
		}

		/*
		===============
		ConnectSignals
		===============
		*/
		/// <summary>
		/// Connects all focus and unfocus signals associated with the <see cref="OptionNode{T}"/>.
		/// </summary>
		protected virtual void ConnectSignals() {
			GameEventBus.ConnectSignal( this, SignalName.FocusEntered, this, OnFocused );
			GameEventBus.ConnectSignal( this, SignalName.MouseEntered, this, OnFocused );
			GameEventBus.ConnectSignal( this, SignalName.FocusExited, this, OnUnfocused );
			GameEventBus.ConnectSignal( this, SignalName.MouseExited, this, OnUnfocused );
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public sealed override void _Ready() {
			base._Ready();

			LinkFocusNodes();
			BindNodes();
			ConnectSignals();
		}
	};
};