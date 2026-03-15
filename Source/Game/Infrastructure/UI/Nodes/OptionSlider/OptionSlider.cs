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
using Godot;
using Nomad.Core.UI;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Events.Globals;

namespace Game.Infrastructure.UI.Nodes.OptionSlider {
	/*
	===================================================================================
	
	OptionSlider
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class OptionSlider : OptionNode.OptionNode {
		[Export( PropertyHint.Range, "0.0,1000.0" )]
		public float Min { get; private set; } = 0.0f;
		[Export( PropertyHint.Range, "0.0,1000.0" )]
		public float Max { get; private set; } = 100.0f;

		public float Value {
			get => _value;
			set {
				SetValue( value );
			}
		}
		private float _value;

		public InternString SliderId => new InternString( Name );
		
		private EngineHorizontalSlider _slider;
		private EngineText _valueLabel;

		public IGameEvent<float> ValueChanged => _valueChanged;
		private IGameEvent<float> _valueChanged;

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnInit() {
			_slider = FindChild<EngineHorizontalSlider>( "Input" );
			_slider.Changed.Subscribe( OnValueChanged );

			_valueLabel = FindChild<EngineText>( "Value" );
			
			// CHAIN?
			_valueChanged = GameEventRegistry.GetEvent<float>( $"{Name}:{UIConstants.OPTION_SLIDER_VALUE_CHANGED_EVENT}", UIConstants.NAMESPACE );
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
		public void SetValue( float value ) {
			_slider.Value = value;
			_valueLabel.Text = value.ToString();
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
		private void OnValueChanged( in float args ) {
			_valueChanged.Publish( args );
		}
	};
};
