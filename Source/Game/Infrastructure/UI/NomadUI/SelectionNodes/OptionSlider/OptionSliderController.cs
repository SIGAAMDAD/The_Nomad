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
		public InternString SliderId => _view.SliderId;

		public float Value {
			get => (float)_view.Slider.Value;
			set {
				_view.SetValue( value );
			}
		}

		/*
		===============
		OptionSliderController
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventBus"></param>
		/// <param name="view"></param>
		public OptionSliderController( IGodotEventBusService eventBus, OptionSliderView view )
			: base( eventBus, view )
		{
			eventBus.ConnectSignal( view.Slider, HSlider.SignalName.ValueChanged, view.Slider, Callable.From<float>( OnValueChanged ) );
		}

		/*
		===============
		OnValueChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnValueChanged( float value ) {
			var eventFactory = _view.Owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			UIEventHelper.PublishUIEvent( eventFactory, UIConstants.OPTION_SLIDER_VALUE_CHANGED_EVENT, new OptionSliderValueChangedEventArgs( _view.SliderId, (float)_view.Slider.Value ) );
		}
	};
};