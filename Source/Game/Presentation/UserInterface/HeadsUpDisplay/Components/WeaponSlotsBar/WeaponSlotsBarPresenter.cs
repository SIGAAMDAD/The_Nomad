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
using System.Numerics;
using System.Timers;
using Nomad.Game.Domain.Data.Items;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.HeadsUpDisplay;
using Nomad.Game.Domain.Interfaces.Inventory;
using Nomad.Game.Domain.Interfaces.Player.Inventory;

namespace Nomad.Game.Presentation.UserInterface.HeadsUpDisplay.Components.WeaponSlotsBar
{
	/*
	===================================================================================

	WeaponSlotsBarPresenter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class WeaponSlotsBarPresenter : IDisposable
	{
		private readonly IHotSlotContainerView _view;
		private readonly IWeaponSlotService _slotService;
		private readonly IItemInstanceRepository _itemCatalog;

		private readonly Timer _hideTimer;

		private readonly IDisposable _weaponSlotChanged;

		private readonly PlayerId _playerId;

		private bool _isDisposed = false;

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
		public WeaponSlotsBarPresenter( PlayerId playerId, IHotSlotContainerView view, IWeaponSlotService slotService, IItemInstanceRepository itemCatalog )
		{
			playerId.ThrowIfInvalid( nameof( WeaponSlotsBarPresenter ) );

			_playerId = playerId;
			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_itemCatalog = itemCatalog ?? throw new ArgumentNullException( nameof( itemCatalog ) );

			_slotService = slotService ?? throw new ArgumentNullException( nameof( slotService ) );
			_weaponSlotChanged = _slotService.WeaponSlotChanged.Subscribe( OnWeaponSlotChanged );

			_hideTimer = new Timer() {
				Interval = 1.25f,
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
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_weaponSlotChanged.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
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

			if ( !_slotService.TryGetSlot( args.CurrentSlot, out var instanceId ) ) {
				return;
			}

			if ( !_itemCatalog.TryGet<WeaponDefinition>( instanceId, out var instance ) ) {
				return;
			}

			_view.SetColor( new Vector4( 1.0f, 1.0f, 1.0f, 1.0f ) );
			_view.SetSelectedHotSlot( (int)args.CurrentSlot );
		}

		private void OnHide( object? sender, ElapsedEventArgs e )
		{
			_view.FadeOut();
		}
	};
};
