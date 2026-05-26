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
using System;

namespace Nomad.Game.Application.Gameplay.Player.JumpKit
{
	/*
	===================================================================================

	DashRuntime

	===================================================================================
	*/
	/// <summary>
	/// Owns dash state and timing. No engine side-effects live here.
	/// </summary>

	internal sealed class DashRuntime
	{
		private const float BURNOUT_PER_DASH = 0.30f;
		private const float BURNOUT_RECOVERY_RATE = 0.10f;
		private const float DASH_DURATION_PENALTY_PER_DASH = 0.05f;
		private const float DASH_DURATION_RECOVERY_RATE = 0.05f;
		private const float MINIMUM_DASH_DURATION = 0.10f;

		public float BurnoutAmount { get; private set; }
		public float BurnoutCooldownElapsed { get; private set; }
		public float CurrentDashDuration { get; private set; }
		public float RemainingDashTime { get; private set; }

		public bool IsDashing { get; private set; }
		public bool IsBurnedOut { get; private set; }

		private readonly float _burnoutRechargeDuration;
		private float _burnoutRechargeRemaining;

		/*
		===============
		DashRuntime
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="initialDashDuration"></param>
		/// <param name="burnoutRechargeDuration"></param>
		public DashRuntime( float initialDashDuration, float burnoutRechargeDuration )
		{
			CurrentDashDuration = initialDashDuration;
			_burnoutRechargeDuration = burnoutRechargeDuration;
		}

		/*
		===============
		CanStartDash
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public bool CanStartDash()
		{
			return !IsDashing && !IsBurnedOut;
		}

		/*
		===============
		ResetDashDuration
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="dashDuration"></param>
		public void ResetDashDuration( float dashDuration )
		{
			CurrentDashDuration = dashDuration;
		}

		/*
		===============
		TryStartDash
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		public DashStartResult TryStartDash( IDashModule module )
		{
			ArgumentGuard.ThrowIfNull( module );

			if ( IsDashing || IsBurnedOut ) {
				return new DashStartResult(
					status: DashStartStatus.Rejected,
					burnoutAmount: BurnoutAmount,
					dashDuration: CurrentDashDuration,
					remainingDashTime: RemainingDashTime,
					isBurnedOut: IsBurnedOut
				);
			}

			if ( BurnoutAmount >= 1.0f ) {
				EnterBurnout( module );

				return new DashStartResult(
					status: DashStartStatus.BurnedOut,
					burnoutAmount: BurnoutAmount,
					dashDuration: CurrentDashDuration,
					remainingDashTime: 0.0f,
					isBurnedOut: true
				);
			}

			IsDashing = true;
			RemainingDashTime = CurrentDashDuration;

			BurnoutAmount += module.DashBurnoutIncrease;
			CurrentDashDuration = Math.Max( MINIMUM_DASH_DURATION, CurrentDashDuration - DASH_DURATION_PENALTY_PER_DASH );
			BurnoutCooldownElapsed = 0.0f;

			return new DashStartResult(
				status: DashStartStatus.Started,
				burnoutAmount: BurnoutAmount,
				dashDuration: CurrentDashDuration,
				remainingDashTime: RemainingDashTime,
				isBurnedOut: false
			);
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="module"></param>
		/// <returns></returns>
		public DashUpdateResult Update( float delta, IDashModule module )
		{
			ArgumentGuard.ThrowIfNull( module );

			bool dashEnded = false;
			bool burnoutChanged = false;
			bool burnedOutThisFrame = false;
			bool rechargedThisFrame = false;

			if ( IsDashing ) {
				RemainingDashTime -= delta;
				if ( RemainingDashTime <= 0.0f ) {
					RemainingDashTime = 0.0f;
					IsDashing = false;
					dashEnded = true;
				}
			}

			if ( IsBurnedOut ) {
				_burnoutRechargeRemaining -= delta;
				if ( _burnoutRechargeRemaining <= 0.0f ) {
					IsBurnedOut = false;
					_burnoutRechargeRemaining = 0.0f;

					BurnoutAmount = 0.0f;
					BurnoutCooldownElapsed = 0.0f;
					CurrentDashDuration = module.DashDuration;

					burnoutChanged = true;
					rechargedThisFrame = true;
				}

				return new DashUpdateResult(
					burnoutAmount: BurnoutAmount,
					dashDuration: CurrentDashDuration,
					remainingDashTime: RemainingDashTime,
					dashEnded: dashEnded,
					burnoutChangedThisFrame: burnoutChanged,
					burnedOutThisFrame: burnedOutThisFrame,
					rechargedThisFrame: rechargedThisFrame,
					isDashing: IsDashing,
					isBurnedOut: IsBurnedOut
				);
			}

			if ( BurnoutAmount > 0.0f ) {
				BurnoutCooldownElapsed += delta;

				if ( BurnoutCooldownElapsed > module.BurnoutCooldown ) {
					float previousBurnout = BurnoutAmount;

					BurnoutAmount = Math.Clamp(
						BurnoutAmount - (BURNOUT_RECOVERY_RATE * delta),
						0.0f,
						previousBurnout );

					CurrentDashDuration = Math.Min(
						module.DashDuration,
						CurrentDashDuration + (DASH_DURATION_RECOVERY_RATE * delta) );

					burnoutChanged = BurnoutAmount != previousBurnout;
				}
			}

			return new DashUpdateResult(
				burnoutAmount: BurnoutAmount,
				dashDuration: CurrentDashDuration,
				remainingDashTime: RemainingDashTime,
				dashEnded: dashEnded,
				burnoutChangedThisFrame: burnoutChanged,
				burnedOutThisFrame: burnedOutThisFrame,
				rechargedThisFrame: rechargedThisFrame,
				isDashing: IsDashing,
				isBurnedOut: IsBurnedOut
			);
		}

		/*
		===============
		EnterBurnout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="module"></param>
		private void EnterBurnout( IDashModule module )
		{
			IsDashing = false;
			RemainingDashTime = 0.0f;

			IsBurnedOut = true;
			_burnoutRechargeRemaining = _burnoutRechargeDuration;
		}
	};
};
