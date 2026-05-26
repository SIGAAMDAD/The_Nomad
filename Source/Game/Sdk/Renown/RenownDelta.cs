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

using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Renown
{
	/// <summary>
	/// 
	/// </summary>
	public readonly struct RenownDelta
	{
		public readonly InternString RegionId;
		public readonly float Amount;
		public readonly int WorldDay;

		public readonly InternString SourceId;
		public readonly RenownSourceType SourceType;

		public readonly bool CanSpread;
		public readonly bool CanDecay;
		public readonly bool IsHistorical;
		public readonly float HistoricalFloorValue;

		public RenownDelta(
			InternString regionId,
			float amount,
			int worldDay,
			InternString sourceId,
			RenownSourceType sourceType,
			bool canSpread = true,
			bool canDecay = true,
			bool isHistorical = false,
			float historicalFloorValue = 0f
		)
		{
			RegionId = regionId;
			Amount = amount;
			WorldDay = worldDay;
			SourceId = sourceId;
			SourceType = sourceType;
			CanSpread = canSpread;
			CanDecay = canDecay;
			IsHistorical = isHistorical;
			HistoricalFloorValue = historicalFloorValue;
		}
	}
}
