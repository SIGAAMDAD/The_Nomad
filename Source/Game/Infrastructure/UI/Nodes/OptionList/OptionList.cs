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

using Game.Application.UI;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.EngineUtils.UserInterface;
using Nomad.Events.Globals;
using System;
using System.Collections.Generic;

namespace Game.Infrastructure.UI.Nodes.OptionList {
	/*
	===================================================================================
	
	OptionList
		
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class OptionList : OptionNode.OptionNode {
		public int Value {
			get => _value;
			set => SetValue( value );
		}
		private int _value;

		public InternString ListId => new InternString( Name );

		private EngineText _valueLabel;
		private IReadOnlyList<string> _items;

		public IGameEvent<int> ValueSet => _valueSet;
		private IGameEvent<int> _valueSet;

		/*
		===============
		SetValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void SetValue( int value ) {
			if ( _items == null ) {
				throw new InvalidOperationException();
			}
			_value = value;
			_valueLabel.Text = _items[ value ];
		}

		/*
		===============
		SetOptions
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="items"></param>
		public void SetOptions( IReadOnlyList<string> items ) {
			_items = items;

			// we don't know how many elements we have so just reset
			SetValue( 0 );
		}


		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			FindChild<EngineButton>( "LeftIcon" ).Clicked.Subscribe( OnPrevToggle );
			FindChild<EngineButton>( "RightIcon" ).Clicked.Subscribe( OnNextToggle );

			_valueSet = GameEventRegistry.GetEvent<int>( $"{Name}:{UIConstants.OPTION_LIST_VALUE_SET_EVENT}", UIConstants.NAMESPACE );
		}

		/*
		===============
		OnPrevToggle
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnPrevToggle( in EmptyEventArgs args ) {
			if ( _items == null ) {
				return;
			}
			_value--;
			if ( _value < 0 ) {
				_value = _items.Count - 1;
			}
			_valueLabel.Text = _items[ _value ];
			_valueSet.Publish( _value );
		}

		/*
		===============
		OnNextToggle
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnNextToggle( in EmptyEventArgs args ) {
			if ( _items == null ) {
				return;
			}
			_value++;
			if ( _value >= _items.Count ) {
				_value = 0;
			}
			_valueLabel.Text = _items[ _value ];
			_valueSet.Publish( _value );
		}
	};
};
