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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.CVars;
using Nomad.Game.Sdk.Renown;
using Nomad.Game.Sdk.Events.World;
using Nomad.Game.Sdk.World;

namespace Nomad.Game.Application.Gameplay.World.Renown
{
	/*
	===================================================================================

	RenownDecayService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class RenownDecayService : IDisposable
	{
		private readonly IDisposable _dayChanged;
		private readonly IRenownTrackerService _tracker;

		private readonly CVarBinding<int> _gracePeriodDays;
		private readonly CVarBinding<float> _baseDailyDecay;

		public RenownDecayService( IRenownTrackerService tracker, ICalendarService calendar, ICVarSystemService cvarSystem )
		{
			ArgumentGuard.ThrowIfNull( calendar, nameof( calendar ) );
			ArgumentGuard.ThrowIfNull( cvarSystem, nameof( cvarSystem ) );

			_dayChanged = calendar.DayChanged.Subscribe( OnDayChanged );
			_tracker = tracker ?? throw new ArgumentNullException( nameof( tracker ) );

			_gracePeriodDays = new CVarBinding<int>( cvarSystem.GetCVarOrThrow<int>( "game.renown.DecayGracePeriod" ) );
			_baseDailyDecay = new CVarBinding<float>( cvarSystem.GetCVarOrThrow<float>( "game.renown.BaseDailyDecay" ) );
		}

		public void Dispose()
		{
			_dayChanged?.Dispose();
		}

		private void OnDayChanged( in DayChangedEventArgs args )
		{
			foreach ( var status in _tracker.GetAllStatuses().Values ) {
				int daysSinceLastGain = args.Time.Day - status.LastPositiveChangeDay;
				if ( daysSinceLastGain < _gracePeriodDays.Value ) {
					continue;
				}
				float decayAmount = CalculateDecay( status, daysSinceLastGain );
				_tracker.ApplyDecay( status.Region, decayAmount, args.Time.Day );
			}
		}

		private float CalculateDecay( RenownStatus status, int daysSinceGain )
		{
			float baseDecay = _baseDailyDecay.Value;

			float tierMultiplier = status.CurrentTier.Tier switch {
				RenownTier.WhisperInTheWinds => 1.0f,
				RenownTier.TavernTalk => 0.9f,
				RenownTier.HeroFromTheHills => 0.65f,
				RenownTier.LocalLegend => 0.4f,
				RenownTier.WanderingWarrior => 0.25f,
				RenownTier.TheNomad => 0.1f,
				_ => 1.0f
			};

			float inactivityMultiplier = 1.0f + MathF.Min( daysSinceGain / 100.0f, 2.0f );
			return baseDecay * tierMultiplier * inactivityMultiplier;
		}
	};
};
