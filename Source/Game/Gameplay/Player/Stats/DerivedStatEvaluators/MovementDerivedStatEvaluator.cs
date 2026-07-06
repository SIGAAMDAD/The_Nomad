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
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Gameplay.Player.Stats.DerivedStatEvaluators
{
	/*
	===================================================================================

	MovementDerivedStatEvaluator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class MovementDerivedStatEvaluator : IPlayerDerivedStatEvaluator
	{
		/*
		===============
		CanEvaluate
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool CanEvaluate( DerivedStatType type )
		{
			return type == DerivedStatType.MovementSpeedMultiplier
				|| type == DerivedStatType.EffectiveMovementSpeed;
		}

		public float Evaluate( DerivedStatType type, in PlayerDerivedStatEvaluationContext context )
		{
			return type switch {
				DerivedStatType.MovementSpeedMultiplier => EvaluateMovementSpeedMultiplier( in context ),
				DerivedStatType.EffectiveMovementSpeed => Math.Max(
					0.0f,
					context.GetBaseStat( BaseStatType.BaseMovementSpeed ) * context.GetDerivedStat( DerivedStatType.MovementSpeedMultiplier ) ),
				_ => throw new ArgumentOutOfRangeException( nameof( type ) )
			};
		}

		private static float EvaluateMovementSpeedMultiplier( in PlayerDerivedStatEvaluationContext context )
		{
			float weight = context.GetBaseStat( BaseStatType.InventoryWeight );
			float threshold = context.GetBaseStat( BaseStatType.EncumbranceThreshold );

			if ( threshold <= 0.0f || weight <= threshold ) {
				return 1.0f;
			}

			float overRatio = (weight - threshold) / threshold;
			float penalty = Math.Min( overRatio * 0.35f, 0.80f );
			return Math.Max( 0.20f, 1.0f - penalty );
		}
	};
};
