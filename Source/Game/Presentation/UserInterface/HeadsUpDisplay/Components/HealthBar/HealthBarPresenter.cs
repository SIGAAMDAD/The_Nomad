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

using System;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HealthBar {
	/*
	===================================================================================
	
	HealthBarPresenter
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	internal sealed class HealthBarPresenter {
		private readonly IHealthBarModel _model;
		private readonly IHealthBarView _view;

		private readonly float _delay = 1.0f;
		private readonly float _trailSpeed = 50.0f;

		// TODO: make these configurable
		private readonly float _veryLowHealthThreshold = 0.25f;
		private readonly float _warningThreshold = 0.5f;

		private int _delayExpirationTicks = 0;

		/*
		===============
		HealthBarPresenter
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="model"></param>
		/// <param name="view"></param>
		public HealthBarPresenter( IHealthBarModel model, IHealthBarView view ) {
			_model = model;
			_view = view;

			_model.HealthChanged += OnHealthChanged;
		}

		/*
		===============
		Render
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public void Render( float delta ) {
			_view.SetSizeParameters();
			
			int now = DateTime.Now.Millisecond;
			if ( now < _delayExpirationTicks ) {
				return;
			}

			float health = _view.GetHealth();
			float trail = _view.GetTrail();

			float deltaFrac = _trailSpeed * delta / _model.MaxHealth;
			float diff = health - trail;

			if ( MathF.Abs( diff ) * _model.MaxHealth <= 0.0001f ) {
				trail = MathF.Min( trail + deltaFrac, health );
			} else {
				trail = MathF.Max( trail - deltaFrac, health );
			}
			_view.SetTrail( trail );
		}

		/*
		===============
		OnHealthChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnHealthChanged() {
			int now = DateTime.Now.Millisecond;
			_delayExpirationTicks = now + (int)( _delay * 1000 );

			if ( _model.LastWasHeal ) {
				_view.SetTrail( _view.GetHealth() );
			}

			float ratio = _model.Health / _model.MaxHealth;
			_view.SetWarningBarsVisibility( ratio <= _warningThreshold );
			_view.SetVeryLowHealthVisibility( ratio <= _veryLowHealthThreshold );
			_view.SetValue( ratio );
		}
	};
};