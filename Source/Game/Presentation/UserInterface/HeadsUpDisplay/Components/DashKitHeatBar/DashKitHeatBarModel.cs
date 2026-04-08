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

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.DashKitHeatBar {
	internal sealed class DashKitHeatBarModel : IDashKitHeatBarModel {
		public float BurnoutAmount { get; private set; }
		public float MaxBurnout { get; private set; }
		public HUDPreset Preset { get; private set; }
		public Color Color { get; private set; }

		public DashKitHeatBarModel( IGameEventRegistryService eventFactory ) {
			eventFactory
				.GetEvent<PlayerResourceChangedEventArgs>( EventNames.PLAYER_RESOURCE_CHANGED, EventNames.NAMESPACE )
				.Subscribe( OnResourceChanged );
			
			eventFactory
				.GetEvent<PlayerDashModuleChangedEventArgs>( EventNames.PLAYER_DASH_MODULE_CHANGED, EventNames.NAMESPACE )
				.Subscribe( OnDashModuleChanged );
		}

		private void OnDashModuleChanged( in PlayerDashModuleChangedEventArgs args ) {
			MaxBurnout = 1.0f;
		}

		private void OnResourceChanged( in PlayerResourceChangedEventArgs args ) {
			if ( args.Resource != PlayerResourceType.JumpKitHeat ) {
				return;
			}
			BurnoutAmount = args.NewValue;
		}

		public void Dispose() {
		}
	};
};