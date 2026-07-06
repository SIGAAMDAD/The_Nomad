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

using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.Game.Gameplay.Renown.Regional;

namespace Nomad.Game.Gameplay.Renown
{
	internal sealed class RenownCoordinator
	{
		private readonly RenownDecayService _decayService;
		private readonly RenownSaveCoordinator _saveCoordinator;
		private readonly RenownEventResolver _eventResolver;
		private readonly RenownPropogationService _propogationService;
		private readonly RenownTrackerService _trackerService;

		public RenownCoordinator( ICVarSystemService cvarSystem )
		{
			ArgumentGuard.ThrowIfNull( cvarSystem, nameof( cvarSystem ) );

			RenownCVarRegistry.RegisterCVars( cvarSystem );
		}
	};
};
