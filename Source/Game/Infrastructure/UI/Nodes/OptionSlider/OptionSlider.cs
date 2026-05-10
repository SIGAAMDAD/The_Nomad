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

using Nomad.Events.Globals;
using Nomad.Game.Domain.Events.UI;
using Godot;
using Nomad.Core.Events;

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
					_slider.MinValue = _min;
				}
			}
		}
		private float _min = 0.0f;

		public float Max {
			get => _max;
			set {
				_max = value;
				if ( _slider != null ) {
					_slider.MaxValue = _max;
				}
			}
		}
		private float _max = 100.0f;

		public float Value {
			get => (float)_slider.Value;
			set => SetValue( value );
		}

		private HSlider _slider;
		private Label _valueLabel;

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
			Label title = GetNode<Label>( "Title" );
			title.Text = Title;

			_slider = GetNode<HSlider>( "Input" );
			_slider.MinValue = _min;
			_slider.MaxValue = _max;
			_slider.ValueChanged += OnValueChanged;

			_valueLabel = _slider.GetNode<Label>( "Value" );

			var leftButton = GetNode<Button>( "LeftIcon" );
			leftButton.Pressed += OnToggleLeft;

			var rightButton = GetNode<Button>( "RightIcon" );
			rightButton.Pressed += OnToggleRight;
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
		private void OnValueChanged( double args )
		{
			float value = (float)args;
			SetValue( value );
			_valueChanged.Publish( new OptionSliderValueChangedEventArgs( value ) );
		}

		/*
		===============
		OnToggleLeft
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnToggleLeft()
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
		private void OnToggleRight()
		{
			OnValueChanged( (float)_slider.Value + 1.0f );
		}
	};
};
