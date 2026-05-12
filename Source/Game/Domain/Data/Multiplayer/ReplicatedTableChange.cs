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

namespace Nomad.Game.Domain.Data.Multiplayer
{
	public readonly struct ReplicatedTableChange<TKey, TValue>
	{
		public readonly ReplicatedTableChangeType Type;
		public readonly TKey Key;
		public readonly TValue PreviousValue;
		public readonly TValue CurrentValue;
		public readonly bool HadPreviousValue;
		public readonly uint Version;

		public ReplicatedTableChange(
			ReplicatedTableChangeType type,
			TKey key,
			TValue previousValue,
			TValue currentValue,
			bool hadPreviousValue,
			uint version
		)
		{
			Type = type;
			Key = key;
			PreviousValue = previousValue;
			CurrentValue = currentValue;
			HadPreviousValue = hadPreviousValue;
			Version = version;
		}
	};
};
