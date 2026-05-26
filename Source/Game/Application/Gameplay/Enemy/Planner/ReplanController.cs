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

using Nomad.Game.Application.Gameplay.Enemy.Planner;
using Nomad.Game.Application.Gameplay.Enemy.Planner.Goals;

namespace Nomad.Game.Application.Gameplay.Enemy.Planner
{
	public sealed class ReplanController
	{
		private readonly float _minReplanIntervalSeconds;
		private readonly float _maxPlanAgeSeconds;
		private float _timeSinceLastPlan;

		public ReplanReason PendingReasons { get; private set; }

		public ReplanController( float minReplanIntervalSeconds, float maxPlanAgeSeconds )
		{
			_minReplanIntervalSeconds = minReplanIntervalSeconds;
			_maxPlanAgeSeconds = maxPlanAgeSeconds;
			_timeSinceLastPlan = 999f;
			PendingReasons = ReplanReason.None;
		}

		public void Tick( float dt )
		{
			_timeSinceLastPlan += dt;
		}

		public void MarkDirty( ReplanReason reason )
		{
			PendingReasons |= reason;
		}

		public bool ShouldReplan( bool hasUsablePlan, bool currentStepStillValid )
		{
			if ( _timeSinceLastPlan < _minReplanIntervalSeconds ) {
				return false;
			}

			if ( !hasUsablePlan ) {
				PendingReasons |= ReplanReason.NoPlan;
				return true;
			}

			if ( !currentStepStillValid ) {
				PendingReasons |= ReplanReason.ContextInvalid;
				return true;
			}

			if ( PendingReasons != ReplanReason.None ) {
				return true;
			}

			if ( _timeSinceLastPlan >= _maxPlanAgeSeconds ) {
				PendingReasons |= ReplanReason.PeriodicRefresh;
				return true;
			}

			return false;
		}

		public ReplanReason ConsumeReasons()
		{
			ReplanReason reasons = PendingReasons;
			PendingReasons = ReplanReason.None;
			_timeSinceLastPlan = 0f;
			return reasons;
		}

		public void OnPlanBuilt()
		{
			PendingReasons = ReplanReason.None;
			_timeSinceLastPlan = 0f;
		}
	}
};
