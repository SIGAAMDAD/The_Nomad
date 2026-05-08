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

namespace Nomad.Game.Domain.Data.Renown
{
	/// <summary>
	/// Represents a "live"/active contract.
	/// </summary>
	public record ContractInstance
	{
		/// <summary>
		/// What the contract's internal id is.
		/// </summary>
		public Guid Id { get; init; }

		/// <summary>
		/// The contract's internal type.
		/// </summary>
		public Guid Archetype { get; init; }

		/// <summary>
		/// The time of the contract's creation.
		/// </summary>
		public DateTime CreationTime { get; init; }

		/// <summary>
		/// The time of which this contract is due.
		/// </summary>
		public DateTime DueDate { get; init; }
	};
};
