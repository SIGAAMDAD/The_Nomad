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

namespace Nomad.Game.Application.Gameplay.Enemy.Planner.Sensors {
	public abstract class SensorBase : ISensor {
		private readonly float _intervalSeconds;
		private float _timeUntilNextTick;

		protected SensorBase( float intervalSeconds ) {
			_intervalSeconds = intervalSeconds <= 0f ? 0f : intervalSeconds;
			_timeUntilNextTick = 0f;
		}

		public void Tick( NpcAgent agent, float dt ) {
			if ( agent == null ) {
				throw new ArgumentNullException( nameof( agent ) );
			}
			if ( _intervalSeconds > 0f ) {
				_timeUntilNextTick -= dt;
				if ( _timeUntilNextTick > 0f ) {
					return;
				}

				_timeUntilNextTick = _intervalSeconds;
			}

			Sense( agent, dt );
		}

		protected abstract void Sense( NpcAgent agent, float dt );
	};
};