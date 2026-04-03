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

namespace Nomad.Game.Application.Configuration.Enums {
	/// <summary>
	/// The amount of the Heads Up Display that is shown during gameplay
	/// </summary>
	public enum HUDPreset : uint {
		/// <summary>
		/// Everything is shown at all times.
		/// </summary>
		Full,

		/// <summary>
		/// Elements are only shown when they are being used then fade away after a short delay.
		/// </summary>
		Partial,

		/// <summary>
		/// The HUD is completely hidden.
		/// </summary>
		Hidden,

		/// <summary>
		/// Custom setup that the user can configure.
		/// </summary>
		Custom,

		Count
	};
};