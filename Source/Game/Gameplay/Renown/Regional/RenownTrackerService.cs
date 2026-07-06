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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Core.Numerics;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Renown;
using Nomad.Game.Sdk.Events.Renown.Region;
using Nomad.Game.Sdk.Renown.Regional;
using Nomad.Game.Gameplay.Runtime.Renown;
using Nomad.Game.Sdk.Areas;

namespace Nomad.Game.Gameplay.Renown.Regional
{
	/*
	===================================================================================

	RenownTrackerService

	===================================================================================
	*/
	/// <summary>
	/// Handles incoming renown events.
	/// </summary>

	internal sealed class RenownTrackerService : IRenownTrackerService
	{
		private readonly ConcurrentDictionary<AreaDefinitionId, RenownStatus> _regionRenown = new();
		private readonly RenownTierDefinition[] _tiers;
		private readonly CompiledRenownDatabase _compiledRenown;

		public IGameEvent<RenownScoreChangedEventArgs> RenownScoreChanged => _renownScoreChanged;
		private readonly IGameEvent<RenownScoreChangedEventArgs> _renownScoreChanged = null;

		public IGameEvent<RenownTierChangedEventArgs> RenownTierChanged => _renownTierChanged;
		private readonly IGameEvent<RenownTierChangedEventArgs> _renownTierChanged = null;

		/*
		===============
		RenownTrackerService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="eventFactory"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public RenownTrackerService(
			IGameEventRegistryService eventFactory,
			CompiledRenownDatabase? compiledRenown = null )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			_compiledRenown = compiledRenown ?? CompiledRenownDatabase.Empty;

			_renownScoreChanged = eventFactory
				.GetEvent<RenownScoreChangedEventArgs>(
					RenownScoreChangedEventArgs.Name,
					RenownScoreChangedEventArgs.NameSpace
				);

			_renownTierChanged = eventFactory
				.GetEvent<RenownTierChangedEventArgs>(
					RenownTierChangedEventArgs.Name,
					RenownTierChangedEventArgs.NameSpace
				);

			_tiers = new RenownTierDefinition[] {
				Constants.TIER_0_DEFINITION,
				Constants.TIER_1_DEFINITION,
				Constants.TIER_2_DEFINITION,
				Constants.TIER_3_DEFINITION,
				Constants.TIER_4_DEFINITION,
				Constants.TIER_5_DEFINITION
			};
		}

		/*
		===============
		ApplyDecay
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="amount"></param>
		/// <param name="worldDay"></param>
		public void ApplyDecay( AreaDefinitionId regionId, float amount, int worldDay )
		{
			if ( amount <= 0.0f ) {
				return;
			}

			RenownStatus status = GetOrCreateStatus( regionId );

			float previousRenown = status.CurrentRenown;
			RenownTier previousTier = status.CurrentTier.Tier;

			float newRenown = status.CurrentRenown - amount;

			// Historical memory floor.
			// Renown can fade, but major events should not vanish.
			status.CurrentRenown = MathF.Max( status.DecayFloor, newRenown );

			if ( ScalarMath.NearlyEqual( previousRenown, status.CurrentRenown ) ) {
				return;
			}

			status.LastAnyChangeDay = worldDay;
			status.CurrentTier = ResolveRenownTier( status.CurrentRenown );
			_renownScoreChanged.Publish(
				new RenownScoreChangedEventArgs(
					regionId,
					previousRenown,
					newRenown
				)
			);

			if ( status.CurrentTier.Tier != previousTier ) {
				_renownTierChanged.Publish(
					new RenownTierChangedEventArgs(
						regionId,
						previousTier,
						status.CurrentTier.Tier
					)
				);
			}
		}

		/*
		===============
		ApplyDelta
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public void ApplyDelta( RenownDelta delta )
		{
			RenownStatus status = GetOrCreateStatus( delta.RegionId );

			float previousValue = status.CurrentRenown;
			RenownTier previousTier = status.CurrentTier.Tier;

			status.CurrentRenown = MathF.Max(
				status.DecayFloor,
				status.CurrentRenown + delta.Amount
			);

			status.PeakRenown = MathF.Max( status.PeakRenown, status.CurrentRenown );
			status.LastAnyChangeDay = delta.WorldDay;

			if ( delta.Amount > 0.0f ) {
				status.LastPositiveChangeDay = delta.WorldDay;
			}

			status.CurrentTier = ResolveRenownTier( status.CurrentRenown );
			_renownScoreChanged.Publish(
				new RenownScoreChangedEventArgs(
					delta.RegionId,
					previousValue,
					status.CurrentRenown
				)
			);

			if ( status.CurrentTier.Tier != previousTier ) {
				_renownTierChanged.Publish(
					new RenownTierChangedEventArgs(
						delta.RegionId,
						previousTier,
						status.CurrentTier.Tier
					)
				);
			}
		}

		/*
		===============
		GetAllStatuses
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public IReadOnlyDictionary<AreaDefinitionId, RenownStatus> GetAllStatuses()
		{
			return _regionRenown;
		}

		/*
		===============
		GetRenownRatio
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public float GetRenownRatio( AreaDefinitionId regionId )
		{
			RenownStatus status = GetStatus( regionId );

			float current = status.CurrentRenown;
			RenownTierDefinition currentTier = status.CurrentTier;
			RenownTierDefinition? nextTier = status.CurrentTier.Tier == RenownTier.TheNomad ? null : _tiers[(int)status.CurrentTier.Tier + 1];

			if ( nextTier == null ) {
				return 1.0f;
			}

			float tierStart = currentTier.RequiredRenown;
			float tierEnd = nextTier.RequiredRenown;

			if ( tierEnd <= tierStart ) {
				return 1.0f;
			}
			return Math.Clamp( (current - tierStart) / (tierEnd - tierStart), 0.0f, 1.0f );
		}

		/*
		===============
		GetStatus
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public RenownStatus GetStatus( AreaDefinitionId regionId )
		{
			return GetOrCreateStatus( regionId );
		}

		/*
		===============
		GetTier
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public RenownTier GetTier( AreaDefinitionId regionId )
		{
			var status = GetOrCreateStatus( regionId );
			return status.CurrentTier.Tier;
		}

		/*
		===============
		SetHistoricalFloor
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="sourceId"></param>
		/// <param name="floorValue"></param>
		public void SetHistoricalFloor( AreaDefinitionId regionId, InternString sourceId, float floorValue )
		{
			var status = GetOrCreateStatus( regionId );
			//status.HistoricalFloorSources[sourceId] = floorValue;
		}

		/*
		===============
		ResolveRenownTier
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="score"></param>
		/// <returns></returns>
		private RenownTierDefinition ResolveRenownTier( float score )
		{
			if ( score < Constants.RENOWN_REQUIREMENT_TIER_1 ) {
				return _tiers[(int)RenownTier.WhisperInTheWinds];
			} else if ( score < Constants.RENOWN_REQUIREMENT_TIER_2 ) {
				return _tiers[(int)RenownTier.TavernTalk];
			} else if ( score < Constants.RENOWN_REQUIREMENT_TIER_3 ) {
				return _tiers[(int)RenownTier.HeroFromTheHills];
			} else if ( score < Constants.RENOWN_REQUIREMENT_TIER_4 ) {
				return _tiers[(int)RenownTier.LocalLegend];
			} else if ( score < Constants.RENOWN_REQUIREMENT_TIER_5 ) {
				return _tiers[(int)RenownTier.WanderingWarrior];
			}
			return _tiers[(int)RenownTier.TheNomad];
		}

		/*
		===============
		GetOrCreateStatus
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		private RenownStatus GetOrCreateStatus( AreaDefinitionId regionId )
		{
			return _regionRenown.GetOrAdd( regionId, CreateInitialStatus );
		}

		private RenownStatus CreateInitialStatus( AreaDefinitionId areaId )
		{
			float initialRenown = 0.0f;
			float decayFloor = 0.0f;

			if ( _compiledRenown.Regional.TryGet( areaId, out CompiledRegionalRenownDefinition compiled ) ) {
				initialRenown = compiled.InitialRenownScore;
				decayFloor = compiled.HistoricalFloor;
			}

			RenownTierDefinition tier = ResolveRenownTier( initialRenown );

			return new RenownStatus {
				Region = areaId,
				CurrentRenown = initialRenown,
				PeakRenown = initialRenown,
				CurrentTier = tier,
				HighestTierReached = tier,
				DecayFloor = decayFloor
			};
		}
	};
};
