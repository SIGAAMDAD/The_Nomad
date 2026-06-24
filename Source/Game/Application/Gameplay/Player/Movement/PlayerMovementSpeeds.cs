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
using System.Runtime.CompilerServices;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal sealed class PlayerMovementSpeeds
	{
		private float _effectiveMovementSpeed;
		private float _effectiveDashSpeed;

		public void Initialize( IPlayerDerivedStatService stats )
		{
			_effectiveMovementSpeed = stats.GetValue( DerivedStatType.EffectiveMovementSpeed );
			_effectiveDashSpeed = stats.GetValue( DerivedStatType.EffectiveDashSpeed );
		}

		public void ApplyStatChange( in PlayerDerivedStatChangedEventArgs args )
		{
			if ( args.StatId == DerivedStatType.EffectiveMovementSpeed ) {
				_effectiveMovementSpeed = args.NewValue;
				return;
			}

			if ( args.StatId == DerivedStatType.EffectiveDashSpeed ) {
				_effectiveDashSpeed = args.NewValue;
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetMovementSpeed()
		{
			return PlayerMovementSettings.GameplayUnitsToWorldUnits( _effectiveMovementSpeed );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetDashSpeed()
		{
			float fallback = GetMovementSpeed() * PlayerMovementSettings.DASH_SPEED_FALLBACK_MULTIPLIER;
			return MathF.Max( fallback, PlayerMovementSettings.GameplayUnitsToWorldUnits( _effectiveDashSpeed ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetSlideSpeed()
		{
			return GetMovementSpeed() * PlayerMovementSettings.SLIDE_SPEED_MULTIPLIER;
		}
	}
}
