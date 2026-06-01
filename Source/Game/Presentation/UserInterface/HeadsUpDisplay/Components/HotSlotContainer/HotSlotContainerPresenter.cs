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
using System.Timers;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.HeadsUpDisplay;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Game.Sdk.Items;
using Godot;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.HotSlotContainer
{
	internal sealed class HotSlotContainerPresenter : HudComponentPresenter<IHotSlotContainerView>
	{
		private readonly IHotSlotContainerView _view;
		private readonly IWeaponSlotService _slotService;
		private readonly IItemInstanceRepository _itemCatalog;

		private readonly System.Timers.Timer _hideTimer;
		private readonly IDisposable _weaponSlotChanged;
		private readonly PlayerId _playerId;

		/*
		===============
		WeaponSlotsBarPresenter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="view"></param>
		/// <param name="slotService"></param>
		/// <param name="itemCatalog"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public HotSlotContainerPresenter( PlayerId playerId, IHotSlotContainerView view, IWeaponSlotService slotService, IItemInstanceRepository itemCatalog )
			: base( view )
		{
			playerId.ThrowIfInvalid( nameof( HotSlotContainerPresenter ) );

			_playerId = playerId;
			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_itemCatalog = itemCatalog ?? throw new ArgumentNullException( nameof( itemCatalog ) );

			_slotService = slotService ?? throw new ArgumentNullException( nameof( slotService ) );
			_weaponSlotChanged = _slotService.WeaponSlotChanged.Subscribe( OnWeaponSlotChanged );

			_hideTimer = new System.Timers.Timer() {
				Interval = 4.25f,
				AutoReset = false,
				Enabled = true
			};
			_hideTimer.Elapsed += OnHide;
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

			_weaponSlotChanged.Dispose();
			_hideTimer.Dispose();
		}

		/*
		===============
		OnWeaponSlotChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWeaponSlotChanged( in WeaponSlotChangedEventArgs args )
		{
			if ( args.PlayerId != _playerId ) {
				return;
			}

			/*
			if ( !_slotService.TryGetSlot( args.CurrentSlot, out var instanceId ) ) {
				return;
			}

			if ( !_itemCatalog.TryGet( instanceId, out var instance ) ) {
				return;
			}
			*/

			_view.SetColor( new System.Numerics.Vector4( 1.0f, 1.0f, 1.0f, 1.0f ) );
			_view.SetSelectedHotSlot( (int)args.CurrentSlot );
			_hideTimer.Start();
		}

		private void OnHide( object? sender, ElapsedEventArgs e )
		{
			Callable.From(() => _view.FadeOut()).CallDeferred();
		}
	};
};
