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
using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Sdk.Events.Player;
using Nomad.Game.Sdk.Player.Stats;

namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerMovementSpeeds

	===================================================================================
	*/
	/// <summary>
	/// Cache of stat-derived movement speeds in world units. This keeps the movement hot
	/// path from converting gameplay units every physics tick.
	/// </summary>

	internal sealed class PlayerMovementSpeeds
	{
		private float _movementSpeed;
		private float _dashStatSpeed;
		private float _dashSpeed;
		private float _slideSpeed;

		public PlayerMovementSpeeds( IPlayerDerivedStatService stats )
		{
			ArgumentGuard.ThrowIfNull( stats, nameof( stats ) );

			float movement = stats.GetValue( DerivedStatType.EffectiveMovementSpeed );
			float dash = stats.GetValue( DerivedStatType.EffectiveDashSpeed );

			SetMovementSpeed( movement );
			SetDashSpeed( dash );
		}

		public void ApplyStatChange( in PlayerDerivedStatChangedEventArgs args )
		{
			if ( args.StatId == DerivedStatType.EffectiveMovementSpeed ) {
				SetMovementSpeed( args.NewValue );
				return;
			}

			if ( args.StatId == DerivedStatType.EffectiveDashSpeed ) {
				SetDashSpeed( args.NewValue );
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetMovementSpeed()
		{
			return _movementSpeed;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetDashSpeed()
		{
			return _dashSpeed;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetSlideSpeed()
		{
			return _slideSpeed;
		}

		private void SetMovementSpeed( float gameplayUnits )
		{
			_movementSpeed = PlayerMovementSettings.GameplayUnitsToWorldUnits( gameplayUnits );
			_slideSpeed = _movementSpeed * PlayerMovementSettings.SLIDE_SPEED_MULTIPLIER;
			_dashSpeed = MathF.Max( _movementSpeed * PlayerMovementSettings.DASH_SPEED_FALLBACK_MULTIPLIER, _dashStatSpeed );
		}

		private void SetDashSpeed( float gameplayUnits )
		{
			_dashStatSpeed = PlayerMovementSettings.GameplayUnitsToWorldUnits( gameplayUnits );
			float fallback = _movementSpeed * PlayerMovementSettings.DASH_SPEED_FALLBACK_MULTIPLIER;
			_dashSpeed = MathF.Max( fallback, _dashStatSpeed );
		}
	}
}
