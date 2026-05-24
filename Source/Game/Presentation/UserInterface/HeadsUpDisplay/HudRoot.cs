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
using System.Collections.Generic;
using Nomad.Core.Events;
using Nomad.Game.Application.Configuration.Enums;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;
using Nomad.Game.Prefabs;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.DashKitHeatBar;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HealthBar;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.InteractionMenu;
using Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.RageBar;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay
{
	/*
	===================================================================================

	HudRoot

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class HudRoot : IHudRoot
	{
		public HUDPreset Preset => _preset;
		private HUDPreset _preset;

		private readonly ComponentGroup _combatGroup;
		private readonly ComponentGroup _neutralGroup;
		private readonly ComponentGroup _explorationGroup;
		private readonly InteractionMenuPresenter _interactionMenu;

		private bool _isDisposed = false;

		/*
		===============
		HudRoot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="root"></param>
		/// <param name="eventFactory"></param>
		public HudRoot( PlayerId playerId, HeadsUpDisplayView root, IGameEventRegistryService eventFactory )
		{
			_neutralGroup = new ComponentGroup(
				new List<HudComponentPresenter>() {
					new HealthBarPresenter( playerId, root.GetNode<HealthBarView>( "NeutralHUD/StatBarContainer/HealthBar" ), eventFactory ),
					new RageBarPresenter( playerId, root.GetNode<RageBarView>( "NeutralHUD/StatBarContainer/RageBar" ), eventFactory ),
					new DashKitHeatBarPresenter( playerId, root.GetNode<DashStatusBarView>( "NeutralHUD/StatBarContainer/DashStatusBar" ), eventFactory ),
				}
			);
			_interactionMenu = new InteractionMenuPresenter( playerId, root );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_neutralGroup.Dispose();
			_interactionMenu.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
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
		public void Render( float delta )
		{
			_neutralGroup.Render( delta );
			_interactionMenu.Render();
		}
	};
};
