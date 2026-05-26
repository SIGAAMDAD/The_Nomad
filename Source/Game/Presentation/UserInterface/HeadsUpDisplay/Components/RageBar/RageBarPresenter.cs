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
using Nomad.Core.Events;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.HeadsUpDisplay;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.RageBar
{
	internal sealed class RageBarPresenter : HudComponentPresenter<IRageBarView>
	{
		private readonly PlayerId _playerId;

		private readonly IDisposable _resourceChanged;
		private readonly IDisposable _derivedStatChanged;

		private float _rage = 0.0f;
		private float _maxRage = 0.0f;
		private readonly float _trailSpeed = 50.0f;

		public RageBarPresenter( PlayerId playerId, IRageBarView view, IGameEventRegistryService eventFactory )
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

		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}

			base.Dispose( disposing );

			_resourceChanged.Dispose();
			_derivedStatChanged.Dispose();
		}

		public override void Render( float delta )
		{
			view.SetSizeParameters();

			if ( _maxRage <= 0.0f ) {
				view.SetValue( 0.0f );
				view.SetTrail( 0.0f );
				return;
			}

			float rage = view.GetRage();
			float trail = view.GetTrail();

			float deltaFrac = _trailSpeed * delta / _maxRage;
			float diff = rage - trail;

			if ( MathF.Abs( diff ) * _maxRage <= 0.0001f ) {
				trail = rage;
			} else if ( diff > 0.0f ) {
				trail = MathF.Min( trail + deltaFrac, rage );
			} else {
				trail = MathF.Max( trail - deltaFrac, rage );
			}

			view.SetTrail( trail );
		}

		private void OnRageChanged()
		{
			float ratio = _maxRage > 0.0f ? Math.Clamp( _rage / _maxRage, 0.0f, 1.0f ) : 0.0f;
			view.SetValue( ratio );
		}

		private void OnDerivedStatChanged( in PlayerDerivedStatChangedEventArgs args )
		{
			if ( args.PlayerId != _playerId || args.StatId != DerivedStatType.EffectiveRageMax ) {
				return;
			}

			_maxRage = args.NewValue;
			OnRageChanged();
		}

		private void OnResourceChanged( in PlayerResourceChangedEventArgs args )
		{
			if ( args.PlayerId != _playerId || args.Resource != PlayerResourceType.Rage ) {
				return;
			}

			_rage = args.NewValue;
			OnRageChanged();
		}
	};
};
