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

namespace Nomad.Game.Domain.Data.Multiplayer.Packets
{
	public readonly struct UserCommandPacket
	{
		/// <summary>
		/// Simulation time covered by this command.
		/// Usually fixed, e.g. 16 ms or 33 ms.
		/// </summary>
		public readonly byte Msec;

		/// <summary>
		/// -127 to 127 normalized input.
		/// </summary>
		public readonly sbyte MoveX;

		/// <summary>
		/// -127 to 127 normalized input.
		/// </summary>
		public readonly sbyte MoveY;

		/// <summary>
		/// 0-65535 maps to 0-360 degrees.
		/// </summary>
		public readonly ushort AimAngle;

		public readonly InputButtons HeldButtons;
		public readonly InputButtons PressedButtons;
		public readonly InputButtons ReleasedButtons;

		public readonly ushort SelectedWeaponId;
		public readonly ushort SelectedArmAttachmentId;

		public readonly byte Impulse;

		public UserCommandPacket(
			byte msec,
			sbyte moveX,
			sbyte moveY,
			ushort aimAngle,
			InputButtons heldButtons,
			InputButtons pressedButtons,
			InputButtons releasedButtons,
			ushort selectedWeaponId,
			ushort selectedArmAttachmentId,
			byte impulse
		)
		{
			Msec = msec;
			MoveX = moveX;
			MoveY = moveY;
			AimAngle = aimAngle;
			HeldButtons = heldButtons;
			PressedButtons = pressedButtons;
			ReleasedButtons = releasedButtons;
			SelectedWeaponId = selectedWeaponId;
			SelectedArmAttachmentId = selectedArmAttachmentId;
			Impulse = impulse;
		}
	};
};
