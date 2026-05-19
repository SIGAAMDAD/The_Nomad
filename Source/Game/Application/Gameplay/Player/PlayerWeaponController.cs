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

namespace Nomad.Game.Application.Gameplay.Player
{
	/*
	===================================================================================

	PlayerWeaponController

	===================================================================================
	*/
	/// <summary>
	/// Owns weapon state, weapon binding usage, and weapon slot handling.
	/// </summary>

	internal sealed class PlayerWeaponController
	{
		private readonly IDisposable _useWeapon;
		private readonly IDisposable _nextWeapon;
		private readonly IDisposable _prevWeapon;
		private readonly IDisposable _reload;
		private readonly IDisposable _switchToPrimaryWeapon;
		private readonly IDisposable _switchToSecondaryWeapon;
		private readonly IDisposable _switchToHeavyPrimaryWeapon;
		private readonly IDisposable _switchToHeavySecondaryWeapon;

		public PlayerWeaponController( IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
		}
	};
};
