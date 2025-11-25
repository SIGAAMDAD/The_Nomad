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

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes {
	/*
	===================================================================================
	
	OptionList
		
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class OptionList : OptionNode {
		[Export]
		public bool NoTranslate { get; private set; }
		[Export]
		public Godot.Collections.Array<StringName> Items { get; private set; } = new Godot.Collections.Array<StringName>();

		/*
		===============
		AddItem
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <exception cref="ArgumentException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void AddItem( StringName item ) {
			if ( item == null || item.IsEmpty ) {
				throw new ArgumentException( "item is null or empty!", nameof( item ) );
			}
			Items.Add( item );
			if ( ValueLabel.Text.Length == 0 ) {
				SetValue( 0 );
			}
		}

		/*
		===============
		SetValue
		===============
		*/
		public override void SetValue( object value ) {
			int data = (int)value;
			if ( data == -1 ) {
				ValueLabel.Text = "Custom";
			} else {
				ValueLabel.Text = Items[ data ];//NoTranslate ? Items[ data ] : TranslationServer.Translate( Items[ data ] );
			}
			base.SetValue( value );
		}

		/*
		===============
		OnLeftIconPressed
		===============
		*/
		private void OnLeftIconPressed() {
			int data = (int)Value;
			if ( data == 0 ) {
				SetValue( Items.Count - 1 );
			} else if ( data == -1 ) {
				SetValue( 0 );
			} else {
				SetValue( data - 1 );
			}
			//UIAudioManager.OnButtonPressed();
		}

		/*
		===============
		OnRightIconPressed
		===============
		*/
		private void OnRightIconPressed() {
			int data = (int)Value;
			if ( data == Items.Count - 1 || data == -1 ) {
				SetValue( 0 );
			} else {
				SetValue( data + 1 );
			}
			//UIAudioManager.OnButtonPressed();
		}

		/*
		===============
		BindNodes
		===============
		*/
		protected override void BindNodes() {
			base.BindNodes();

			ValueLabel = GetNode<Godot.Label>( "Value" );
		}

		/*
		===============
		ConnectSignals
		===============
		*/
		protected override void ConnectSignals() {
			base.ConnectSignals();

			GameEventBus.ConnectSignal( GetNode<Godot.Button>( "LeftIcon" ), Button.SignalName.Pressed, this, OnLeftIconPressed );
			GameEventBus.ConnectSignal( GetNode<Godot.Button>( "RightIcon" ), Button.SignalName.Pressed, this, OnRightIconPressed );
		}
	};
};