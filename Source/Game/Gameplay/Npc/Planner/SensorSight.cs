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

using Nomad.Core.Physics.Services;
using Nomad.Game.Sdk.Npc;
using Nomad.Game.Sdk.Npc.Planner;

namespace Nomad.Game.Gameplay.Npc.Planner.Sensors
{
	/*
	===================================================================================

	SensorSight

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public class SensorSight : SensorBase
	{
		private readonly IRaycastService _raycastService;

		/*
		===============
		SensorSight
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public SensorSight( IRaycastService raycastService )
			: base( 0.75f )
		{
			_raycastService = raycastService;
		}

		/*
		===============
		Sense
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="agent"></param>
		/// <param name="dt"></param>
		protected override void Sense( INpcAgent agent, float dt )
		{
			WorkingMemory memory = agent.Memory;
		}
	};
};
