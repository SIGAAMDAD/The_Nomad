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

using Nomad.Core.Events;
using Nomad.Game.Application.Configuration.Enums;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;
using Nomad.Game.Prefabs;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.DashKitHeatBar;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HealthBar;
using Nomad.UI;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay {
	internal sealed class HudRoot : IHudRoot {
		public HUDPreset Preset {
			get {
				throw new System.NotImplementedException();
			}
		}

		public IHealthBarView HealthBar => _healthBar;
		private readonly IHealthBarView _healthBar;

		public IRageBarView RageBar {
			get {
				throw new System.NotImplementedException();
			}
		}

		public IAmmoCounterView AmmoCounter {
			get {
				throw new System.NotImplementedException();
			}
		}

		private readonly HealthBarPresenter _healthBarPresenter;
		private readonly DashKitHeatBarPresenter _dashKitPresenter;

		public HudRoot( EnginePanel root, IGameEventRegistryService eventFactory ) {
			_healthBarPresenter = new HealthBarPresenter( new HealthBarModel( eventFactory ), root.FindChild<HealthBarView>( "StatBarContainer/HealthBar" ) );
			_dashKitPresenter = new DashKitHeatBarPresenter( new DashKitHeatBarModel( eventFactory ), root.FindChild<DashStatusBarView>( "CombatContainer/DashStatusBar" ) );
		}

		public void Render( float delta ) {
			_healthBarPresenter.Render( delta );
			_dashKitPresenter.Render();
		}
	};
};