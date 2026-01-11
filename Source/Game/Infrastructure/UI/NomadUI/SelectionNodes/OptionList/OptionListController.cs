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
using Game.Domain.Events.UI;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionNode;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System;
using System.Collections.Generic;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList {
	/*
	===================================================================================
	
	OptionListController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class OptionListController : OptionNodeController<IOptionListView>, IOptionListController {
		private static readonly NodePath LEFT_BUTTON_NODEPATH = "LeftIcon";
		private static readonly NodePath RIGHT_BUTTON_NODEPATH = "RightIcon";

		public InternString ListId => _view.ListId;

		public int Value => _value;
		private int _value;

		private IReadOnlyList<string> _items;

		/*
		===============
		OptionListController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventBus"></param>
		/// <param name="view"></param>
		public OptionListController( IGodotEventBusService eventBus, OptionListView view )
			: base( eventBus, view )
		{
			eventBus.ConnectSignal( view.Owner.GetNode<Button>( LEFT_BUTTON_NODEPATH ), Button.SignalName.Pressed, view.Owner, OnPrevToggle );
			eventBus.ConnectSignal( view.Owner.GetNode<Button>( RIGHT_BUTTON_NODEPATH ), Button.SignalName.Pressed, view.Owner, OnNextToggle );
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
			_view.SetOption( _items[ value ] );
		}

		/*
		===============
		OnPrevToggle
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnPrevToggle() {
			if ( _items == null ) {
				return;
			}
			_value--;
			if ( _value < 0 ) {
				_value = _items.Count - 1;
			}
			_view.SetOption( _items[ _value ] );

			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.PublishUIEvent( eventFactory, UIConstants.OPTION_LIST_VALUE_SET_EVENT, new OptionListValueSetEventArgs( _view.ListId, _value ) );
		}

		/*
		===============
		OnNextToggle
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnNextToggle() {
			if ( _items == null ) {
				return;
			}
			_value++;
			if ( _value >= _items.Count ) {
				_value = 0;
			}
			_view.SetOption( _items[ _value ] );

			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.PublishUIEvent( eventFactory, UIConstants.OPTION_LIST_VALUE_SET_EVENT, new OptionListValueSetEventArgs( _view.ListId, _value ) );
		}
	};
};