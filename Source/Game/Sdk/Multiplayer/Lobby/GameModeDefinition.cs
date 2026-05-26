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

namespace Nomad.Game.Sdk.Multiplayer.Lobby
{
	/// <summary>
	///
	/// </summary>
	public sealed record GameModeDefinition
	{
		/// <summary>
		///
		/// </summary>
		public InternString Id { get; init; }

		/// <summary>
		///
		/// </summary>
		public InternString DisplayName { get; init; }

		/// <summary>
		///
		/// </summary>
		public InternString Description { get; init; }

		/// <summary>
		/// The minimum required players in a lobby to start the game.
		/// </summary>
		public int MinPlayers { get; init; }

		/// <summary>
		/// The maximum amount of players allowed in the lobby for this gamemode.
		/// </summary>
		public int MaxPlayers { get; init; }

		/// <summary>
		///
		/// </summary>
		public MultiplayerMode Mode { get; init; }
	}
}
