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
using Nomad.Core.Events;
using Nomad.Game.Domain.Data.Multiplayer.Lobby;
using Nomad.Game.Domain.Events.Multiplayer;
using Nomad.Game.Domain.Interfaces.Multiplayer;

namespace Nomad.Game.Application.Multiplayer.Lobby
{
	/*
	===================================================================================

	VoteService

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal abstract class VoteService : IVoteService
	{
		protected VoteInstance instance;

		public IGameEvent<VoteKickStartedEventArgs> VoteKickStarted => _voteKickStarted;
		private readonly IGameEvent<VoteKickStartedEventArgs> _voteKickStarted = default;

		public IGameEvent<VoteCancelledEventArgs> VoteCancelled => _voteCancelled;
		private readonly IGameEvent<VoteCancelledEventArgs> _voteCancelled = default;

		/*
		===============
		VoteService
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="eventFactory"></param>
		public VoteService( IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );

			_voteKickStarted = eventFactory.GetEvent<VoteKickStartedEventArgs>(
				VoteKickStartedEventArgs.Name,
				VoteKickStartedEventArgs.NameSpace
			);
			_voteKickStarted.Subscribe( OnVoteKickStarted );

			_voteCancelled = eventFactory.GetEvent<VoteCancelledEventArgs>(
				VoteCancelledEventArgs.Name,
				VoteCancelledEventArgs.NameSpace
			);
		}

		private void OnVoteKickStarted( in VoteKickStartedEventArgs args )
		{
			instance = new VoteKickInstance {
				InitiatorId = args.InitiatorId,
				VictimId = args.VictimId,
			};
		}
	};
};
