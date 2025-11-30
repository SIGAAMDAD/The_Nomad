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

using Game.Infrastructure.UI.NomadUI.SelectionNodes.Events;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.Interfaces;
using Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionNode;
using Godot;
using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using NomadCore.Systems.EventSystem.Common;
using System;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionCheckbox {
	/*
	===================================================================================
	
	OptionCheckboxView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class OptionCheckboxView : OptionNodeView<OptionCheckbox>, IOptionCheckboxView {
		private static readonly StringName ON_STRING = TranslationServer.Translate( "UI_ON" );
		private static readonly StringName OFF_STRING = TranslationServer.Translate( "UI_OFF" );

		public GameEvent Toggled => _toggled;
		private readonly GameEvent _toggled = new GameEvent( nameof( Toggled ) );

		private readonly Godot.Label _valueLabel;

		/*
		===============
		OptionCheckboxView
		===============
		*/
		public OptionCheckboxView( OptionCheckbox owner )
			: base( owner )
		{
			var eventBus = ServiceRegistry.Get<IGameEventBusService>();
			ArgumentNullException.ThrowIfNull( eventBus );
			eventBus.ConnectSignal( _owner.GetNode<Godot.Button>( "LeftIcon" ), Button.SignalName.Pressed, _owner, OnToggled );
			eventBus.ConnectSignal( _owner.GetNode<Godot.Button>( "RightIcon" ), Button.SignalName.Pressed, _owner, OnToggled );

			_valueLabel = _owner.GetNode<Godot.Label>( "Value" );
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
		public void SetValue( bool value ) {
			_valueLabel.Text = value ? ON_STRING : OFF_STRING;
		}

		/*
		===============
		OnToggled
		===============
		*/
		private void OnToggled() {
			_toggled.Publish( EmptyEventArgs.Args );
		}
	};
};