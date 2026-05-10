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

using Nomad.Core.Events;
using Nomad.Events.Globals;
using Godot;
using System;
using System.Collections.Generic;
using Nomad.Game.Domain.Events.UI;

namespace Nomad.Game.Infrastructure.UI.Nodes.OptionList
{
	/*
	===================================================================================

	OptionList

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class OptionList : OptionNode.OptionNode
	{
		public int Value {
			get => _value;
			set => SetValue( value );
		}
		private int _value;

		private Label _valueLabel;

		public IReadOnlyList<string> Values => _items;
		private IReadOnlyList<string> _items;

		[Event( nameSpace: "Nomad.Game.Domain.Events.UI", PayloadName = "OptionListValueSetEventArgs" )]
		[EventPayload( "Value", typeof( int ) )]
		public IGameEvent<OptionListValueSetEventArgs> ValueSet => _valueSet;
		private readonly IGameEvent<OptionListValueSetEventArgs> _valueSet = default;

		public OptionList()
		{
			_valueSet = GameEventRegistry
				.GetEvent<OptionListValueSetEventArgs>(
					$"{GetHashCode()}:{OptionListValueSetEventArgs.Name}",
					OptionListValueSetEventArgs.NameSpace
				);
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
		/// <exception cref="InvalidOperationException"></exception>
		public void SetValue( int value )
		{
			if ( _items == null ) {
				throw new InvalidOperationException();
			}
			_value = value;
			_valueLabel.Text = _items[value];
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
		public void SetOptions( IReadOnlyList<string> items )
		{
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
		protected override void OnInit()
		{
			Label title = GetNode<Label>( "Title" );
			title.Text = Title;

			GetNode<Button>( "LeftIcon" ).Pressed += OnPrevToggle;
			GetNode<Button>( "RightIcon" ).Pressed += OnNextToggle;

			_valueLabel = GetNode<Label>( "Value" );
		}

		/*
		===============
		OnPrevToggle
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnPrevToggle()
		{
			if ( _items == null ) {
				return;
			}
			_value--;
			if ( _value < 0 ) {
				_value = _items.Count - 1;
			}
			_valueLabel.Text = _items[_value];
			_valueSet.Publish( new OptionListValueSetEventArgs( _value ) );
		}

		/*
		===============
		OnNextToggle
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnNextToggle()
		{
			if ( _items == null ) {
				return;
			}
			_value++;
			if ( _value >= _items.Count ) {
				_value = 0;
			}
			_valueLabel.Text = _items[_value];
			_valueSet.Publish( new OptionListValueSetEventArgs( _value ) );
		}
	};
};
