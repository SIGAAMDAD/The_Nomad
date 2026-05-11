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

using Nomad.Game.Domain.Data.Multiplayer.Objectives;

namespace Nomad.Game.Domain.Data.Multiplayer.Modes
{
	public readonly struct CaptureTheFlagInstanceData
	{
		public int RedTeamScore { get; init; }
		public int BlueTeamScore { get; init; }

		public FlagStatus RedFlagState { get; init; }
		public FlagStatus BlueFlagState { get; init; }
	};
};
