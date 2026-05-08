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

namespace Nomad.Game.Application.Gameplay.Enemy.Planner.Sensors
{
	/*
	===================================================================================
	
	SensorBase
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public abstract class SensorBase : ISensor
	{
		private readonly float _intervalSeconds;
		private float _timeUntilNextTick;

		/*
		===============
		SensorBase
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="intervalSeconds"></param>
		protected SensorBase( float intervalSeconds )
		{
			_intervalSeconds = intervalSeconds <= 0.0f ? 0.0f : intervalSeconds;
			_timeUntilNextTick = 0.0f;
		}

		/*
		===============
		Tick
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="agent"></param>
		/// <param name="dt"></param>
		public void Tick( NpcAgent agent, float dt )
		{
			ArgumentGuard.ThrowIfNull( agent );

			if ( _intervalSeconds > 0.0f ) {
				_timeUntilNextTick -= dt;
				if ( _timeUntilNextTick > 0.0f ) {
					return;
				}

				_timeUntilNextTick = _intervalSeconds;
			}

			Sense( agent, dt );
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
		protected abstract void Sense( NpcAgent agent, float dt );
	};
};
