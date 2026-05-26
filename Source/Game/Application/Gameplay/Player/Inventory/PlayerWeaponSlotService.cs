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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Items;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Player.Inventory;
using Nomad.Input;
using Nomad.Input.ValueObjects;
using Nomad.Game.Application.Gameplay.Player;

namespace Nomad.Game.Application.Gameplay.Player.Inventory
{
	/*
	===================================================================================

	PlayerWeaponSlotService

	===================================================================================
	*/
	/// <summary>
	/// Owns weapon slot changes and handling.
	/// </summary>

	internal sealed class PlayerWeaponSlotService : IWeaponSlotService
	{
		private readonly ItemInstanceId[]? _slots = new ItemInstanceId[(int)WeaponSlotIndex.Count];

		public WeaponSlotIndex Current => _activeSlot;
		private WeaponSlotIndex _activeSlot = WeaponSlotIndex.LightPrimary;

		private readonly PlayerId _playerId;

		private readonly IDisposable _nextWeapon;
		private readonly IDisposable _prevWeapon;
		private readonly IDisposable _switchToPrimaryWeapon;
		private readonly IDisposable _switchToSecondaryWeapon;
		private readonly IDisposable _switchToHeavyPrimaryWeapon;
		private readonly IDisposable _switchToHeavySecondaryWeapon;

		private bool _isDisposed = false;

		public IGameEvent<WeaponSlotChangedEventArgs> WeaponSlotChanged => _weaponSlotChanged;
		private readonly IGameEvent<WeaponSlotChangedEventArgs> _weaponSlotChanged = null;

		public IGameEvent<WeaponSlotContentsChangedEventArgs> WeaponSlotContentsChanged => _weaponSlotContentsChanged;
		private readonly IGameEvent<WeaponSlotContentsChangedEventArgs> _weaponSlotContentsChanged = null;

		/*
		===============
		PlayerWeaponSlotService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="eventFactory"></param>
		public PlayerWeaponSlotService( PlayerId playerId, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			playerId.ThrowIfInvalid( nameof( PlayerWeaponSlotService ) );

			_playerId = playerId;

			_weaponSlotChanged = eventFactory.GetEvent<WeaponSlotChangedEventArgs>(
				WeaponSlotChangedEventArgs.Name,
				WeaponSlotChangedEventArgs.NameSpace
			);

			_weaponSlotContentsChanged = eventFactory.GetEvent<WeaponSlotContentsChangedEventArgs>(
				WeaponSlotContentsChangedEventArgs.Name,
				WeaponSlotContentsChangedEventArgs.NameSpace
			);

			_nextWeapon = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"NextWeapon:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnNextWeapon );

			_prevWeapon = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"PrevWeapon:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnPrevWeapon );

			_switchToPrimaryWeapon = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"SwitchToPrimaryWeapon:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnSwitchToPrimaryWeapon );

			_switchToSecondaryWeapon = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"SwitchToSecondaryWeapon:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnSwitchToSecondaryWeapon );

			_switchToHeavyPrimaryWeapon = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"SwitchToHeavyPrimaryWeapon:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnSwitchToHeavyPrimaryWeapon );

			_switchToHeavySecondaryWeapon = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"SwitchToHeavySecondaryWeapon:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnSwitchToHeavySecondaryWeapon );
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
			_nextWeapon.Dispose();
			_prevWeapon.Dispose();
			_switchToPrimaryWeapon.Dispose();
			_switchToSecondaryWeapon.Dispose();
			_switchToHeavyPrimaryWeapon.Dispose();
			_switchToHeavySecondaryWeapon.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		IsOccupied
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public bool IsOccupied( WeaponSlotIndex slot )
		{
			return _slots[(int)slot].IsValid;
		}

		/*
		===============
		TryGetSlot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public bool TryGetSlot( WeaponSlotIndex slot, out ItemInstanceId weapon )
		{
			int index = (int)slot;
			weapon = _slots[index];
			return weapon.IsValid;
		}

		/*
		===============
		TryGetSlot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public bool TrySetSlot( WeaponSlotIndex slot, ItemInstanceId weapon )
		{
			return true;
		}

		/*
		===============
		SetActiveSlot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="slot"></param>
		private void SetActiveSlot( WeaponSlotIndex slot )
		{
			RangeGuard.ThrowIfOutOfRange( (int)slot, (int)WeaponSlotIndex.Min, (int)WeaponSlotIndex.Max, nameof( slot ) );

			WeaponSlotIndex previousSlot = _activeSlot;
			_activeSlot = slot;

			_weaponSlotChanged.Publish(
				new WeaponSlotChangedEventArgs(
					_playerId,
					previousSlot,
					_activeSlot
				)
			);
		}

		/*
		===============
		OnPrevWeapon
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnPrevWeapon( in ButtonActionEventArgs args )
		{
			if ( _activeSlot == WeaponSlotIndex.Min ) {
				SetActiveSlot( WeaponSlotIndex.Max );
			} else {
				SetActiveSlot( _activeSlot - 1 );
			}
		}

		/*
		===============
		OnNextWeapon
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnNextWeapon( in ButtonActionEventArgs args )
		{
			if ( _activeSlot == WeaponSlotIndex.Max ) {
				SetActiveSlot( WeaponSlotIndex.Min );
			} else {
				SetActiveSlot( _activeSlot + 1 );
			}
		}

		/*
		===============
		OnSwitchToPrimaryWeapon
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnSwitchToPrimaryWeapon( in ButtonActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started ) {
				SetActiveSlot( WeaponSlotIndex.LightPrimary );
			}
		}

		/*
		===============
		OnSwitchToSecondaryWeapon
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnSwitchToSecondaryWeapon( in ButtonActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started ) {
				SetActiveSlot( WeaponSlotIndex.LightSidearm );
			}
		}

		/*
		===============
		OnSwitchToHeavyPrimaryWeapon
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnSwitchToHeavyPrimaryWeapon( in ButtonActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started ) {
				SetActiveSlot( WeaponSlotIndex.HeavyPrimary );
			}
		}

		/*
		===============
		OnSwitchToHeavySecondaryWeapon
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnSwitchToHeavySecondaryWeapon( in ButtonActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started ) {
				SetActiveSlot( WeaponSlotIndex.HeavySidearm );
			}
		}

		/*
		===============
		TrySwapSlots
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public bool TrySwapSlots( WeaponSlotIndex a, WeaponSlotIndex b )
		{
			var tmp = _slots[ (int)b ];

			if ( !TrySetSlot( b, _slots[ (int)a ] ) ) {
				return false;
			}

			if ( !TrySetSlot( a, tmp ) ) {
				return false;
			}

			return true;
		}

		/*
		===============
		TryClearSlots
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool TryClearSlots()
		{
			Array.Fill( _slots, ItemInstanceId.Invalid );
			return true;
		}
	};
};
