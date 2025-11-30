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

using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionNode;
using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using NomadCore.Systems.EventSystem.Common;
using System;
using System.Collections.Generic;
using Godot;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionList {
	/*
	===================================================================================
	
	OptionListView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class OptionListView : OptionNodeView<OptionList>, IOptionListView {
		public GameEvent TogglePrev => _togglePrev;
		private readonly GameEvent _togglePrev = new GameEvent( nameof( TogglePrev ) );

		public GameEvent ToggleNext => _toggleNext;
		private readonly GameEvent _toggleNext = new GameEvent( nameof( ToggleNext ) );

		private readonly Godot.Label _valueLabel;
		private readonly IReadOnlyList<string> _items;

		/*
		===============
		OptionListView
		===============
		*/
		public OptionListView( OptionList owner )
			: base( owner )
		{
			var eventBus = ServiceRegistry.Get<IGameEventBusService>();
			ArgumentNullException.ThrowIfNull( eventBus );
			eventBus.ConnectSignal( _owner.GetNode<Godot.Button>( "LeftIcon" ), Button.SignalName.Pressed, _owner, OnLeftIconPressed );
			eventBus.ConnectSignal( _owner.GetNode<Godot.Button>( "RightIcon" ), Button.SignalName.Pressed, _owner, OnRightIconPressed );

			_items = _owner.Items;

			_valueLabel = _owner.GetNode<Godot.Label>( "Value" );
		}

		/*
		===============
		SetSelectedIndex
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void SetSelectedIndex( int index ) {
			if ( index < 0 || index >= _items.Count ) {
				throw new ArgumentOutOfRangeException( nameof( index ) );
			}
			_valueLabel.Text = TranslationServer.Translate( _items[ index ] );
		}

		/*
		===============
		OnLeftIconPressed
		===============
		*/
		private void OnLeftIconPressed() {
			_togglePrev.Publish( EmptyEventArgs.Args );
		}

		/*
		===============
		OnRightIconPressed
		===============
		*/
		private void OnRightIconPressed() {
			_toggleNext.Publish( EmptyEventArgs.Args );
		}
	};
};