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
using Nomad.Game.Domain.Interfaces.Player.Stats;

namespace Nomad.Game.Domain.Data.Player
{
	/*
	===================================================================================

	RuneBehavior

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public abstract class RuneBehavior
	{
		protected readonly IPlayerDerivedStatService derivedStatService;
		protected readonly IPlayerResourceService resourceService;

		public RuneBehavior( IPlayerDerivedStatService derivedStatService, IPlayerResourceService resourceService )
		{
			this.derivedStatService = derivedStatService ?? throw new ArgumentNullException( nameof( derivedStatService ) );
			this.resourceService = resourceService ?? throw new ArgumentNullException( nameof( resourceService ) );
		}

		public abstract void Activate();
		public abstract void Deactivate();
	};
};
