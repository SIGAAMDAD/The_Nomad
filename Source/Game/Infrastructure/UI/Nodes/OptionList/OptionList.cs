/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Game.Application.UI;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.UI;
using Nomad.Events.Globals;
using System;
using System.Collections.Generic;

namespace Nomad.Game.Infrastructure.UI.Nodes.OptionList {
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

		public IReadOnlyList<string> Values => _items;
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
			EngineText title = FindChild<EngineText>( "Title" );
			title.Text = Title;
			
			FindChild<EngineButton>( "LeftIcon" ).Clicked.Subscribe( OnPrevToggle );
			FindChild<EngineButton>( "RightIcon" ).Clicked.Subscribe( OnNextToggle );

			_valueLabel = FindChild<EngineText>( "Value" );

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
