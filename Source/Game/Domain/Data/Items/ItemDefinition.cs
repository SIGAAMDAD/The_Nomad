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
using Nomad.Core.Util;

namespace Nomad.Game.Domain.Data.Items
{
	/// <summary>
	///
	/// </summary>
	public abstract record ItemDefinition : IDisposable
	{
		/// <summary>
		///
		/// </summary>
		public abstract ItemType BaseType { get; }

		/// <summary>
		///
		/// </summary>
		public ItemDefinitionId Id { get; init; }

		/// <summary>
		///
		/// </summary>
		public InternString Name { get; init; }

		/// <summary>
		///
		/// </summary>
		public InternString JournalEntry { get; init; }

		/// <summary>
		///
		/// </summary>
		public float Weight { get; init; }

		/// <summary>
		///
		/// </summary>
		public float BaseCost { get; init; }

		public void Dispose()
		{
			GC.SuppressFinalize( this );
		}
	};
};
