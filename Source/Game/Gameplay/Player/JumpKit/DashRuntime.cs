/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Core.Compatibility.Guards;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Player.JumpKit;
using System.Runtime.CompilerServices;

namespace Nomad.Game.Gameplay.Player.JumpKit
{
	/*
	===================================================================================

	DashRuntime

	===================================================================================
	*/
	/// <summary>
	/// Pure jump-kit state. This object intentionally has no Godot, input, VFX,
	/// audio, or event dependencies. Timed transitions are driven externally.
	/// </summary>

	internal sealed class DashRuntime
	{
		private const float DEFAULT_BURNOUT_LOCKOUT_SECONDS = 2.5f;
		private const float BURNOUT_RECOVERY_RATE = 0.10f;
		private const float DASH_DURATION_PENALTY_PER_DASH = 0.05f;
		private const float MINIMUM_DASH_DURATION = 0.10f;

		private const PlayerJumpKitFlags BLOCKING_DASH_FLAGS = PlayerJumpKitFlags.DashInProgress | PlayerJumpKitFlags.BurnedOut;
		private const PlayerJumpKitFlags ACTIVE_STATE_MASK = PlayerJumpKitFlags.DashInProgress
			| PlayerJumpKitFlags.BurnedOut
			| PlayerJumpKitFlags.Cooling
			| PlayerJumpKitFlags.BurnoutPending;

		private PlayerJumpKitFlags _stateFlags;

		public float BurnoutAmount { get; private set; }
		public float BurnoutCooldownElapsed { get; private set; }
		public float CurrentDashDuration { get; private set; }
		public float RemainingDashTime { get; private set; }
		public float CurrentBurnoutLockoutSeconds { get; private set; }

		public PlayerJumpKitFlags StateFlags => _stateFlags & ACTIVE_STATE_MASK;

		public bool IsDashing => HasAnyState( PlayerJumpKitFlags.DashInProgress );
		public bool IsBurnedOut => HasAnyState( PlayerJumpKitFlags.BurnedOut );
		public bool IsCooling => HasAnyState( PlayerJumpKitFlags.Cooling );
		public bool IsBurnoutPending => HasAnyState( PlayerJumpKitFlags.BurnoutPending );

		public DashRuntime( float initialDashDuration )
		{
			CurrentDashDuration = Max( MINIMUM_DASH_DURATION, initialDashDuration );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool CanStartDash()
		{
			return (_stateFlags & BLOCKING_DASH_FLAGS) == 0;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void ResetDashDuration( float dashDuration )
		{
			CurrentDashDuration = Max( MINIMUM_DASH_DURATION, dashDuration );
		}

		public DashRuntimeChange TryStartDash( in DashModuleRuntimeSettings settings )
		{
			if ( (_stateFlags & BLOCKING_DASH_FLAGS) != 0 ) {
				return DashRuntimeChange.FromState(
					PlayerJumpKitFlags.DashRejected,
					BurnoutAmount,
					BurnoutAmount,
					0.0f,
					CurrentBurnoutLockoutSeconds
				);
			}

			float burnoutMax = settings.BurnoutMax;
			if ( BurnoutAmount >= burnoutMax ) {
				EnterBurnout( in settings );
				return DashRuntimeChange.FromState(
					PlayerJumpKitFlags.DashRejected | PlayerJumpKitFlags.BurnoutEntered,
					BurnoutAmount,
					BurnoutAmount,
					0.0f,
					CurrentBurnoutLockoutSeconds
				);
			}

			_stateFlags &= ~(PlayerJumpKitFlags.Cooling | PlayerJumpKitFlags.BurnoutPending);
			_stateFlags |= PlayerJumpKitFlags.DashInProgress;
			BurnoutCooldownElapsed = 0.0f;

			float oldBurnout = BurnoutAmount;
			float newBurnout = oldBurnout + settings.DashBurnoutIncrease;
			if ( newBurnout >= burnoutMax ) {
				newBurnout = burnoutMax;
				_stateFlags |= PlayerJumpKitFlags.BurnoutPending;
			} else if ( newBurnout <= 0.0f ) {
				newBurnout = 0.0f;
			}

			BurnoutAmount = newBurnout;

			float dashDuration = CurrentDashDuration;
			RemainingDashTime = dashDuration;
			CurrentDashDuration = Max( MINIMUM_DASH_DURATION, dashDuration - DASH_DURATION_PENALTY_PER_DASH );

			PlayerJumpKitFlags changeFlags = PlayerJumpKitFlags.DashStarted;
			if ( newBurnout != oldBurnout ) {
				changeFlags |= PlayerJumpKitFlags.BurnoutChanged;
			}

			return DashRuntimeChange.FromState(
				changeFlags,
				oldBurnout,
				newBurnout,
				dashDuration,
				0.0f
			);
		}

		public DashRuntimeChange CompleteDash( in DashModuleRuntimeSettings settings )
		{
			if ( (_stateFlags & PlayerJumpKitFlags.DashInProgress) == 0 ) {
				return DashRuntimeChange.None( BurnoutAmount );
			}

			_stateFlags &= ~PlayerJumpKitFlags.DashInProgress;
			RemainingDashTime = 0.0f;

			if ( BurnoutAmount >= settings.BurnoutMax || HasAnyState( PlayerJumpKitFlags.BurnoutPending ) ) {
				EnterBurnout( in settings );
				return DashRuntimeChange.FromState(
					PlayerJumpKitFlags.DashEnded | PlayerJumpKitFlags.BurnoutEntered,
					BurnoutAmount,
					BurnoutAmount,
					0.0f,
					CurrentBurnoutLockoutSeconds
				);
			}

			return DashRuntimeChange.FromState(
				PlayerJumpKitFlags.DashEnded,
				BurnoutAmount,
				BurnoutAmount,
				0.0f,
				0.0f
			);
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void BeginCoolingDelay()
		{
			_stateFlags &= ~PlayerJumpKitFlags.Cooling;
			BurnoutCooldownElapsed = 0.0f;
		}

		public DashRuntimeChange BeginPassiveCooling()
		{
			if ( (_stateFlags & BLOCKING_DASH_FLAGS) != 0 || BurnoutAmount <= 0.0f ) {
				_stateFlags &= ~PlayerJumpKitFlags.Cooling;
				return DashRuntimeChange.None( BurnoutAmount );
			}

			_stateFlags |= PlayerJumpKitFlags.Cooling;
			return DashRuntimeChange.FromState(
				PlayerJumpKitFlags.CoolingStarted,
				BurnoutAmount,
				BurnoutAmount,
				0.0f,
				0.0f
			);
		}

		public DashRuntimeChange TickPassiveCooling( float delta, in DashModuleRuntimeSettings settings )
		{
			if ( (_stateFlags & PlayerJumpKitFlags.Cooling) == 0
				|| (_stateFlags & BLOCKING_DASH_FLAGS) != 0
				|| BurnoutAmount <= 0.0f )
			{
				return DashRuntimeChange.None( BurnoutAmount );
			}

			BurnoutCooldownElapsed += delta;

			float oldBurnout = BurnoutAmount;
			float newBurnout = oldBurnout - (BURNOUT_RECOVERY_RATE * delta);
			if ( newBurnout <= 0.0f ) {
				BurnoutAmount = 0.0f;
				BurnoutCooldownElapsed = 0.0f;
				CurrentDashDuration = settings.DashDuration;
				_stateFlags &= ~PlayerJumpKitFlags.Cooling;

				return DashRuntimeChange.FromState(
					PlayerJumpKitFlags.BurnoutChanged | PlayerJumpKitFlags.Recharged,
					oldBurnout,
					0.0f,
					0.0f,
					0.0f
				);
			}

			BurnoutAmount = newBurnout;

			float recoveredDuration = CurrentDashDuration + (DASH_DURATION_PENALTY_PER_DASH * delta);
			CurrentDashDuration = recoveredDuration > settings.DashDuration
				? settings.DashDuration
				: recoveredDuration;

			return DashRuntimeChange.FromState(
				PlayerJumpKitFlags.BurnoutChanged,
				oldBurnout,
				newBurnout,
				0.0f,
				0.0f
			);
		}

		public DashRuntimeChange CompleteBurnoutRecharge( in DashModuleRuntimeSettings settings )
		{
			if ( (_stateFlags & PlayerJumpKitFlags.BurnedOut) == 0 ) {
				return DashRuntimeChange.None( BurnoutAmount );
			}

			float oldBurnout = BurnoutAmount;

			_stateFlags = PlayerJumpKitFlags.None;
			BurnoutAmount = 0.0f;
			BurnoutCooldownElapsed = 0.0f;
			RemainingDashTime = 0.0f;
			CurrentDashDuration = settings.DashDuration;
			CurrentBurnoutLockoutSeconds = 0.0f;

			return DashRuntimeChange.FromState(
				PlayerJumpKitFlags.BurnoutChanged | PlayerJumpKitFlags.Recharged,
				oldBurnout,
				0.0f,
				0.0f,
				0.0f
			);
		}

		private void EnterBurnout( in DashModuleRuntimeSettings settings )
		{
			_stateFlags &= ~(PlayerJumpKitFlags.DashInProgress | PlayerJumpKitFlags.Cooling | PlayerJumpKitFlags.BurnoutPending);
			_stateFlags |= PlayerJumpKitFlags.BurnedOut;
			RemainingDashTime = 0.0f;

			CurrentBurnoutLockoutSeconds = settings.BurnoutResetDuration > 0.0f
				? settings.BurnoutResetDuration
				: DEFAULT_BURNOUT_LOCKOUT_SECONDS;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private bool HasAnyState( PlayerJumpKitFlags flags )
		{
			return (_stateFlags & flags) != 0;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static float Max( float a, float b )
		{
			return a > b ? a : b;
		}
	}

	internal readonly struct DashModuleRuntimeSettings
	{
		private const float MINIMUM_DASH_DURATION = 0.10f;

		public readonly float BurnoutMax;
		public readonly float DashBurnoutIncrease;
		public readonly float DashDuration;
		public readonly float BurnoutCooldown;
		public readonly float BurnoutResetDuration;

		private DashModuleRuntimeSettings(
			float burnoutMax,
			float dashBurnoutIncrease,
			float dashDuration,
			float burnoutCooldown,
			float burnoutResetDuration
		)
		{
			BurnoutMax = burnoutMax;
			DashBurnoutIncrease = dashBurnoutIncrease;
			DashDuration = dashDuration;
			BurnoutCooldown = burnoutCooldown;
			BurnoutResetDuration = burnoutResetDuration;
		}

		public static DashModuleRuntimeSettings FromModule( IDashModule module )
		{
			ArgumentGuard.ThrowIfNull( module, nameof( module ) );

			return new DashModuleRuntimeSettings(
				Max( 0.001f, module.BurnoutMax ),
				Max( 0.0f, module.DashBurnoutIncrease ),
				Max( MINIMUM_DASH_DURATION, module.DashDuration ),
				Max( 0.0f, module.BurnoutCooldown ),
				module.BurnoutResetDuration
			);
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static float Max( float a, float b )
		{
			return a > b ? a : b;
		}
	}

	internal readonly struct DashRuntimeChange
	{
		public readonly PlayerJumpKitFlags Flags;
		public readonly float OldBurnoutAmount;
		public readonly float BurnoutAmount;
		public readonly float DashDurationSeconds;
		public readonly float BurnoutLockoutSeconds;

		private DashRuntimeChange(
			PlayerJumpKitFlags flags,
			float oldBurnoutAmount,
			float burnoutAmount,
			float dashDurationSeconds,
			float burnoutLockoutSeconds
		)
		{
			Flags = flags;
			OldBurnoutAmount = oldBurnoutAmount;
			BurnoutAmount = burnoutAmount;
			DashDurationSeconds = dashDurationSeconds;
			BurnoutLockoutSeconds = burnoutLockoutSeconds;
		}

		public bool HasChange => Flags != PlayerJumpKitFlags.None;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool HasAny( PlayerJumpKitFlags flags )
		{
			return (Flags & flags) != 0;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static DashRuntimeChange None( float burnoutAmount )
		{
			return new DashRuntimeChange( PlayerJumpKitFlags.None, burnoutAmount, burnoutAmount, 0.0f, 0.0f );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static DashRuntimeChange FromState(
			PlayerJumpKitFlags flags,
			float oldBurnoutAmount,
			float burnoutAmount,
			float dashDurationSeconds,
			float burnoutLockoutSeconds
		)
		{
			return new DashRuntimeChange( flags, oldBurnoutAmount, burnoutAmount, dashDurationSeconds, burnoutLockoutSeconds );
		}
	}
}
