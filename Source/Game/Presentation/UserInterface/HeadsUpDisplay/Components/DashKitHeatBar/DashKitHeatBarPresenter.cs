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
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.HeadsUpDisplay;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.DashKitHeatBar
{
	/*
	===================================================================================

	DashKitHeatBarPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class DashKitHeatBarPresenter : HudComponentPresenter<IDashKitHeatBarView>
	{
		private float _burnoutAmount = 0.0f;
		private float _maxBurnout = 0.0f;

		private readonly IDisposable _resourceChanged;
		private readonly IDisposable _dashModuleChanged;

		private float _lastFrameBurnoutAmount = 0.0f;

		private readonly PlayerId _playerId;

		/*
		===============
		DashKitHeatBarPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="model"></param>
		/// <param name="view"></param>
		public DashKitHeatBarPresenter( PlayerId playerId, IDashKitHeatBarView view, IGameEventRegistryService eventFactory )
			: base( view )
		{
			_playerId = playerId;

			_resourceChanged = eventFactory
				.GetEvent<PlayerResourceChangedEventArgs>(
					PlayerResourceChangedEventArgs.Name,
					PlayerResourceChangedEventArgs.NameSpace
				)
				.Subscribe( OnResourceChanged );
		}

		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			_resourceChanged.Dispose();
		}

		private void OnResourceChanged( in PlayerResourceChangedEventArgs args )
		{
			if ( args.PlayerId != _playerId || args.Resource != PlayerResourceType.JumpKitHeat ) {
				return;
			}

			_burnoutAmount = args.NewValue;

			view.ShowOverlayVisibility( _lastFrameBurnoutAmount > _burnoutAmount );
			view.SetValue( _burnoutAmount );
		}
	};
};
