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

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionSlider {
	/*
	===================================================================================
	
	OptionSliderController
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class OptionSliderController : OptionNodeController<IOptionSliderView>, IOptionSliderController {
		public OptionSliderValueChanged ValueChanged => _view.ValueChanged;

		public float Value => _value;
		private float _value;
		
		/*
		===============
		OptionSliderController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		public OptionSliderController( IOptionSliderView view )
			: base( view )
		{
			_view.ValueChanged.Subscribe( this, OnValueChanged );
		}

		/*
		===============
		OnValueChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnValueChanged( in OptionSliderValueChangedEventData args ) {
			_value = args.Value;
		}
	};
};