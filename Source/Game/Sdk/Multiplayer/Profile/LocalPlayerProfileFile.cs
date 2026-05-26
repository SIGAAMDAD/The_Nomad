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
using Nomad.Core.OnlineServices;

namespace Nomad.Game.Sdk.Multiplayer.Profile
{
	public sealed record LocalPlayerProfileFile
	{
		public const int CURRENT_SCHEMA_VERSION = 1;

		public int SchemaVersion { get; init; } = CURRENT_SCHEMA_VERSION;

		public PeerId PeerId { get; init; }
		public PlayerProfileRecord Profile { get; init; }

		/// <summary>
		/// Increments every time the local profile changes.
		/// Useful for save debouncing, debugging, and sync.
		/// </summary>
		public uint LocalRevision { get; init; }

		public DateTimeOffset CreatedAtUtc { get; init; }

		public DateTimeOffset LastSavedAtUtc { get; init; }
	}
}
