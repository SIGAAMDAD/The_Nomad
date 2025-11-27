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

using Godot;
using NomadCore.Abstractions.Services;
using NomadCore.Infrastructure;
using System;

namespace Game.Infrastructure.UI.NomadUI.SelectionNodes {
	/*
	===================================================================================
	
	OptionSlider
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class OptionSlider : OptionNode {
		[Export( PropertyHint.Range, "0.0,1000.0" )]
		public float Min = 0.0f;
		[Export( PropertyHint.Range, "0.0,1000.0" )]
		public float Max = 100.0f;

		public override object Value => (float)Input.Value;

		private HSlider Input;

		/*
		===============
		SetValue
		===============
		*/
		public override void SetValue( object value ) {
			float data = (float)value;
			Input.Value = data;
		}

		/*
		===============
		OnLeftCycle
		===============
		*/
		public void OnLeftCycle() {
			//UIAudioManager.OnButtonPressed();
			Input.Value -= Input.Step;
		}

		/*
		===============
		OnRightCycle
		===============
		*/
		public void OnRightCycle() {
			//UIAudioManager.OnButtonPressed();
			Input.Value += Input.Step;
		}

		/*
		===============
		OnValueChanged
		===============
		*/
		/// <summary>
		/// Called whenever the slider's value has been changed by Godot.
		/// </summary>
		/// <param name="value">The slider's new value.</param>
		private void OnValueChanged( float value ) {
			if ( value < Min || value > Max ) {
				throw new ArgumentOutOfRangeException( nameof( value ) );
			}

			ValueLabel.Text = value.ToString();
			ValueChanged.Publish( new ValueChangedEventData( ConfigVarName, value ) );
		}

		/*
		===============
		BindNodes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void BindNodes() {
			base.BindNodes();

			Input = GetNode<HSlider>( "Input" );
			Input.MinValue = Min;
			Input.MaxValue = Max;
			ServiceRegistry.Get<IGameEventBusService>().ConnectSignal( Input, HSlider.SignalName.ValueChanged, this, Callable.From<float>( OnValueChanged ) );

			ValueLabel = Input.GetNode<Godot.Label>( "Value" );
		}
	};
};