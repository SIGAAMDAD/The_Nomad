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
using Nomad.Game.Application.Multiplayer.Modes;

namespace Nomad.Game.Application.Multiplayer
{
	/*
	===================================================================================

	NetworkGameSimulation

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class NetworkGameSimulation
	{
		private readonly ModeBase _modeData;
		private readonly NetworkGameClock _clock;

		public NetworkGameSimulation( ModeBase modeData )
		{
			_modeData = modeData ?? throw new ArgumentNullException( nameof( modeData ) );
			_clock = new NetworkGameClock();
		}
	};
};
