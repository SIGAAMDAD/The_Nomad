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
using Nomad.Core.Events;
using Nomad.Core.Util;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionCheckbox {
	/*
	===================================================================================
	
	OptionCheckboxController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class OptionCheckboxController : OptionNodeController<IOptionCheckboxView>, IOptionCheckboxController {
		public InternString CheckboxId => _view.CheckboxId;

		public bool Value {
			get => _value;
			set {
				_value = value;
				_view.SetValue( value );
			}
		}
		private bool _value;

		/*
		===============
		OptionCheckboxController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventBus"></param>
		/// <param name="eventFactory"></param>
		/// <param name="view"></param>
		public OptionCheckboxController( IGodotEventBusService eventBus, OptionCheckboxView view )
			: base( eventBus, view )
		{
			_view.SetValue( false );

			eventBus.ConnectSignal( view.Left, Godot.Button.SignalName.Pressed, view.Left, OnToggled );
			eventBus.ConnectSignal( view.Right, Godot.Button.SignalName.Pressed, view.Right, OnToggled );
		}

		/*
		===============
		OnToggled
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnToggled() {
			_value = !_value;

			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.PublishUIEvent( eventFactory, UIConstants.OPTION_CHECKBOX_TOGGLED_EVENT, new OptionCheckboxValueChangedEventArgs( _view.CheckboxId, _value ) );
		}
	};
};