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

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes.OptionSlider {
	/*
	===================================================================================
	
	OptionSliderView
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class OptionSliderView : OptionNodeView<OptionSlider>, IOptionSliderView {
		public OptionSliderValueChanged ValueChanged => _valueChanged;
		private readonly OptionSliderValueChanged _valueChanged = new OptionSliderValueChanged();

		public float Value => (float)_input.Value;

		private readonly HSlider _input;
		private readonly Godot.Label _valueLabel;

		/*
		===============
		OptionSliderView
		===============
		*/
		public OptionSliderView( OptionSlider owner, float min, float max )
			: base( owner )
		{
			_input = _owner.GetNode<HSlider>( "Input" );
			_input.MinValue = min;
			_input.MaxValue = max;
			ServiceRegistry.Get<IGameEventBusService>()?.ConnectSignal( _input, HSlider.SignalName.ValueChanged, _input, OnValueChanged );

			_valueLabel = _input.GetNode<Godot.Label>( "Value" );
		}

		/*
		===============
		SetValue
		===============
		*/
		public void SetValue( float value ) {
			_input.Value = value;
		}

		/*
		===============
		OnValueChanged
		===============
		*/
		private void OnValueChanged() {
			_valueLabel.Text = Value.ToString();
			_valueChanged.Publish( new OptionSliderValueChangedEventData( _owner.ConfigVarName, Value ) );
		}
	};
};