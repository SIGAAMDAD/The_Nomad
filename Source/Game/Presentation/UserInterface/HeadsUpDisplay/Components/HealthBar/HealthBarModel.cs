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
using Nomad.Game.Application.Configuration.Enums;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HealthBar {
	internal sealed class HealthBarModel : IHealthBarModel {
		public bool LastWasHeal { get; private set; }
		public float Health { get; private set; }
		public float MaxHealth { get; private set; }
		public Color Color { get; private set; }
		public HUDPreset Preset { get; private set; }

		public event Action HealthChanged;

		public HealthBarModel( IGameEventRegistryService eventFactory ) {
			eventFactory
				.GetEvent<PlayerResourceChangedEventArgs>( $"{Constants.LOCAL_GUID}:{EventNames.PLAYER_RESOURCE_CHANGED}", EventNames.NAMESPACE )
				.Subscribe( OnResourceChanged );
			
			eventFactory
				.GetEvent<PlayerDerivedStatChangedEventArgs>( $"{Constants.LOCAL_GUID}:{EventNames.PLAYER_DERIVED_STAT_CHANGED}", EventNames.NAMESPACE )
				.Subscribe( OnDerivedStatChanged );
		}

		public void Dispose() {
		}

		private void UpdateColor() {
			Color = Color.Green;

			float fillPercent = MaxHealth / Health;
			if ( fillPercent < 0.70f ) {
				Color = Color.Yellow;
			} else if ( fillPercent < 0.30f ) {
				Color = Color.Red;
			} else if ( fillPercent < 0.10f ) {
				Color = Color.Brown;
			}
		}

		private void OnResourceChanged( in PlayerResourceChangedEventArgs args ) {
			if ( args.Resource != PlayerResourceType.Health ) {
				return;
			}
			LastWasHeal = args.NewValue > Health;
			Health = args.NewValue;
			UpdateColor();

			HealthChanged?.Invoke();
		}

		private void OnDerivedStatChanged( in PlayerDerivedStatChangedEventArgs args ) {
			if ( args.StatId != DerivedStatType.EffectiveHealthMax ) {
				return;
			}
			MaxHealth = args.NewValue;
			UpdateColor();
		}
	};
};