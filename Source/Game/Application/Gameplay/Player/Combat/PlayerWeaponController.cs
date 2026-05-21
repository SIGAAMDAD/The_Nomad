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
using Nomad.Game.Domain.Events.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Input;
using Nomad.Input.ValueObjects;

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerWeaponController

	===================================================================================
	*/
	/// <summary>
	/// Owns weapon usage handling (useWeapon + reload bindings).
	/// </summary>

	internal sealed class PlayerWeaponController : IWeaponController
	{
		private readonly IDisposable _useWeapon;
		private readonly IDisposable _reload;

		private bool _isDisposed = false;

		public IGameEvent<WeaponUsedEventArgs> WeaponUsed => _weaponUsed;
		private readonly IGameEvent<WeaponUsedEventArgs> _weaponUsed = null;

		public PlayerWeaponController( IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_useWeapon = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"UseWeapon:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnUseWeapon );

			_reload = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"Reload:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( OnReload );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_useWeapon.Dispose();
			_reload.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		private void OnUseWeapon( in ButtonActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started ) {
			}
		}

		private void OnReload( in ButtonActionEventArgs args )
		{
			if ( args.Phase != InputActionPhase.Started ) {
				return;
			}
		}
	};
};
