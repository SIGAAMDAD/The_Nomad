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

namespace Nomad.Game.Sdk.Multiplayer.Objectives
{
	/// <summary>
	///
	/// </summary>
	public enum HillStatus : byte
	{
		/// <summary>
		/// The hill is currently not generating points for either team.
		/// </summary>
		Unclaimed,

		/// <summary>
		/// One team is holding the hill.
		/// </summary>
		Claimed,

		/// <summary>
		/// Two teams on the same hill.
		/// </summary>
		Contested
	}
}
