/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Game.Application.UI;
using Nomad.UI;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Events.UI;

namespace Nomad.Game.Infrastructure.UI.Nodes.OptionSlider
{
	/*
	===================================================================================

	OptionSlider

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class OptionSlider : OptionNode.OptionNode
	{
		public float Min {
			get => _min;
			set {
				_min = value;
				if ( _slider != null ) {
					_slider.Minimum = _min;
				}
			}
		}
		private float _min = 0.0f;

		public float Max {
			get => _max;
			set {
				_max = value;
				if ( _slider != null ) {
					_slider.Maximum = _max;
				}
			}
		}
		private float _max = 100.0f;

		public float Value {
			get => (float)_slider.Value;
			set => SetValue( value );
		}

		private EngineHorizontalSlider _slider;
		private EngineText _valueLabel;

		[Event( nameSpace: "Nomad.Game.Domain.Events.UI", PayloadName = "OptionSliderValueChangedEventArgs" )]
		[EventPayload( "Value", typeof( float ) )]
		public IGameEvent<OptionSliderValueChangedEventArgs> ValueChanged => _valueChanged;
		private readonly IGameEvent<OptionSliderValueChangedEventArgs> _valueChanged = default;

		public OptionSlider()
		{
			_valueChanged = GameEventRegistry
				.GetEvent<OptionSliderValueChangedEventArgs>(
					$"{GetHashCode()}:{OptionSliderValueChangedEventArgs.Name}",
					OptionSliderValueChangedEventArgs.NameSpace
				);
		}

		/*
		===============
		OnInit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void OnInit()
		{
			EngineText title = FindChild<EngineText>( "Title" );
			title.Text = Title;

			_slider = FindChild<EngineHorizontalSlider>( "Input" );
			_slider.Minimum = _min;
			_slider.Maximum = _max;
			_slider.ValueSet.Subscribe( OnValueChanged );

			_valueLabel = _slider.FindChild<EngineText>( "Value" );

			var leftButton = FindChild<EngineButton>( "LeftIcon" );
			leftButton.Clicked.Subscribe( OnToggleLeft );

			var rightButton = FindChild<EngineButton>( "RightIcon" );
			rightButton.Clicked.Subscribe( OnToggleRight );
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
		public void SetValue( float value )
		{
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
		private void OnValueChanged( in float args )
		{
			SetValue( args );
			_valueChanged.Publish( new OptionSliderValueChangedEventArgs( args ) );
		}

		/*
		===============
		OnToggleLeft
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnToggleLeft( in EmptyEventArgs args )
		{
			OnValueChanged( (float)_slider.Value - 1.0f );
		}

		/*
		===============
		OnToggleRight
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnToggleRight( in EmptyEventArgs args )
		{
			OnValueChanged( (float)_slider.Value + 1.0f );
		}
	};
};
