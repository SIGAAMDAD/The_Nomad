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

using Nomad.Events.Globals;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.Game.Application.Gameplay.SplitScreen;
using Nomad.Input.Interfaces;
using Nomad.Input.ValueObjects;

namespace Nomad.Game.Prefabs
{
	public partial class SplitScreen2World : WorldBase
	{
		private SplitScreenService _splitScreenService;

		protected override void OnInit()
		{
			base.OnInit();

			var inputSlots = ServiceLocator.GetService<IInputDeviceSlotService>();
			inputSlots.AssignDevice( InputDeviceSlot.Keyboard, 0 );
			inputSlots.AssignDevice( InputDeviceSlot.Mouse, 0 );
			inputSlots.AssignDevice( InputDeviceSlot.Gamepad0, 1 );
			inputSlots.AssignDevice( InputDeviceSlot.Gamepad1, 2 );
			inputSlots.AssignDevice( InputDeviceSlot.Gamepad2, 3 );

			_splitScreenService = new SplitScreenService( GameEventRegistry.Instance, ServiceLocator.Instance, this );
		}

		protected override void OnShutdown()
		{
			base.OnShutdown();

			_splitScreenService?.Dispose();
		}
	};
};
