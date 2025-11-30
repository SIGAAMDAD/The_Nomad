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
using NomadCore.Interfaces.EventSystem;

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
		public OptionCheckboxValueChanged ValueChanged => _valueChanged;
		private readonly OptionCheckboxValueChanged _valueChanged = new OptionCheckboxValueChanged();

		public bool Value => _value;
		private bool _value;

		/*
		===============
		OptionCheckboxController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		public OptionCheckboxController( IOptionCheckboxView view, bool value )
			: base( view )
		{
			_view.Toggled.Subscribe( this, OnToggled );
			_value = value;
			_view.SetValue( _value );
		}

		/*
		===============
		OnToggled
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventData"></param>
		/// <param name="args"></param>
		private void OnToggled( in IGameEvent eventData, in IEventArgs args ) {
			_value = !_value;
			_valueChanged.Publish( new OptionCheckboxValueChangedEventData( _view.Owner.ConfigVarName, _value ) );
		}
	};
};