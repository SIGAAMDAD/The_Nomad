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

using Nomad.Core.CVars;

namespace Nomad.Game.Gameplay.Renown
{
	internal static partial class RenownCVarRegistry
	{
		[CVar( "game.renown.DecayGracePeriod", 7, Group = "Renown", Flags = CVarFlags.Archive )]
		public static readonly CVarDefinition<int> DecayGracePeriod = new CVarDefinition<int>( "game.renown.DecayGracePeriod" );

		[CVar( "game.renown.BaseDailyDecay", 0.15f, Group = "Renown", Flags = CVarFlags.Archive )]
		public static readonly CVarDefinition<float> BaseDailyDecay = new CVarDefinition<float>( "game.renown.BaseDailyDecay" );
	};
};
