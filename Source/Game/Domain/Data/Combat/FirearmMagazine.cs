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

namespace Nomad.Game.Domain.Data.Combat
{
	/// <summary>
	/// Represents a snapshot of a magazine that is used within a firearm.
	/// </summary>
	public readonly struct FirearmMagazine
	{
		public readonly int MaxCapacity;
		public readonly int CurrentCapacity;

		public FirearmMagazine( int maxCapacity, int currentCapacity )
		{
			MaxCapacity = maxCapacity;
			CurrentCapacity = currentCapacity;
		}
	};
};
