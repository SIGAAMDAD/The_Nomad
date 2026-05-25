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
using System.Drawing;
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HealthBar
{
	/*
	===================================================================================

	HealthBarPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class HealthBarPresenter : HudComponentPresenter<IHealthBarView>
	{
		private bool _lastWasHeal = false;
		private float _health = 0.0f;
		private float _maxHealth = 0.0f;
		private Color _color = Color.White;

		private readonly IDisposable _resourceChanged;
		private readonly IDisposable _derivedStatChanged;

		private readonly float _delay = 1.0f;
		private readonly float _trailSpeed = 50.0f;

		// TODO: make these configurable
		private readonly float _veryLowHealthThreshold = 0.25f;
		private readonly float _warningThreshold = 0.5f;

		private int _delayExpirationTicks = 0;

		private readonly PlayerId _playerId;

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
		public HealthBarPresenter( PlayerId playerId, IHealthBarView view, IGameEventRegistryService eventFactory )
			: base( view )
		{
			_playerId = playerId;

			_resourceChanged = eventFactory
				.GetEvent<PlayerResourceChangedEventArgs>(
					PlayerResourceChangedEventArgs.Name,
					PlayerResourceChangedEventArgs.NameSpace
				)
				.Subscribe( OnResourceChanged );

			_derivedStatChanged = eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>(
					PlayerDerivedStatChangedEventArgs.Name,
					PlayerDerivedStatChangedEventArgs.NameSpace
				)
				.Subscribe( OnDerivedStatChanged );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			_resourceChanged.Dispose();
			_derivedStatChanged.Dispose();
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
		public override void Render( float delta )
		{
			view.SetSizeParameters();

			int now = DateTime.Now.Millisecond;
			if ( now < _delayExpirationTicks ) {
				return;
			}

			float health = view.GetHealth();
			float trail = view.GetTrail();

			float deltaFrac = _trailSpeed * delta / _maxHealth;
			float diff = health - trail;

			if ( MathF.Abs( diff ) * _maxHealth <= 0.0001f ) {
				trail = MathF.Min( trail + deltaFrac, health );
			} else {
				trail = MathF.Max( trail - deltaFrac, health );
			}
			view.SetTrail( trail );
		}

		/*
		===============
		OnHealthChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnHealthChanged()
		{
			int now = DateTime.Now.Millisecond;
			_delayExpirationTicks = now + (int)(_delay * 1000);

			if ( _lastWasHeal ) {
				view.SetTrail( view.GetHealth() );
			}

			float ratio = _health / _maxHealth;
			view.SetWarningBarsVisibility( ratio <= _warningThreshold );
			view.SetVeryLowHealthVisibility( ratio <= _veryLowHealthThreshold );
			view.SetValue( ratio );
		}

		/*
		===============
		UpdateColor
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void UpdateColor()
		{
			_color = Color.Green;

			float fillPercent = _maxHealth / _health;
			if ( fillPercent < 0.70f ) {
				_color = Color.Yellow;
			} else if ( fillPercent < 0.30f ) {
				_color = Color.Red;
			} else if ( fillPercent < 0.10f ) {
				_color = Color.Brown;
			}
		}

		private void OnDerivedStatChanged( in PlayerDerivedStatChangedEventArgs args )
		{
			if ( args.PlayerId != _playerId || args.StatId != DerivedStatType.EffectiveHealthMax ) {
				return;
			}

			_maxHealth = args.NewValue;
			UpdateColor();
		}

		private void OnResourceChanged( in PlayerResourceChangedEventArgs args )
		{
			if ( args.PlayerId != _playerId || args.Resource != PlayerResourceType.Health ) {
				return;
			}

			_lastWasHeal = args.NewValue > _health;
			_health = args.NewValue;
			UpdateColor();
			OnHealthChanged();
		}
	};
};
