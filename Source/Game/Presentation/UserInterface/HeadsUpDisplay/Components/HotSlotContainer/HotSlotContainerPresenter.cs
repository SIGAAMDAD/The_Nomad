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
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.HeadsUpDisplay;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HotSlotContainer
{
	internal sealed class HotSlotContainerPresenter : HudComponentPresenter<IHotSlotContainerView>
	{
		private readonly IDisposable _weaponSlotChanged;
		private readonly PlayerId _playerId;

		public HotSlotContainerPresenter( PlayerId playerId, IHotSlotContainerView view, IGameEventRegistryService eventFactory )
			: base( view )
		{
			_playerId = playerId;

			_weaponSlotChanged = eventFactory
				.GetEvent<WeaponSlotChangedEventArgs>(
					WeaponSlotChangedEventArgs.Name,
					WeaponSlotChangedEventArgs.NameSpace
				)
				.Subscribe( OnWeaponSlotChanged );
		}

		protected override void Dispose( bool disposing )
		{
			if ( !disposing ) {
				return;
			}
			base.Dispose( disposing );

			_weaponSlotChanged.Dispose();
		}

		public override void Render( float delta )
		{
		}

		private void OnWeaponSlotChanged( in WeaponSlotChangedEventArgs args )
		{
			if ( args.PlayerId != _playerId ) {
				return;
			}
		}
	};
};
