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
using Nomad.Game.Sdk.Multiplayer;

namespace Nomad.Game.Application.Multiplayer
{
	/*
	===================================================================================

	MultiplayerCoordinator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class MultiplayerCoordinator
	{
		private readonly ILocalPlayerProfileService _localPlayerProfile;
		private readonly ILobbyWaitingRoomService _waitingRoomService;
		private readonly IVotingService _votingService;

		public MultiplayerCoordinator(
			ILocalPlayerProfileService localProfileService,
			ILobbyWaitingRoomService waitingRoomService,
			IVotingService votingService
		)
		{
			_localPlayerProfile = localProfileService ?? throw new ArgumentNullException( nameof( localProfileService ) );
			_waitingRoomService = waitingRoomService ?? throw new ArgumentNullException( nameof( waitingRoomService ) );
			_votingService = votingService ?? throw new ArgumentNullException( nameof( votingService ) );
		}
	};
};
