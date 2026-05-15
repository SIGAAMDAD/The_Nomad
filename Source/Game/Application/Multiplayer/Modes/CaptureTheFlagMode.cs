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

using Nomad.Core.Events;
using Nomad.Networking.Session;
using Nomad.Game.Application.Multiplayer.Modes;
using Nomad.Game.Domain.Data.Multiplayer;
using Nomad.Networking.Rpc;
using Nomad.Networking.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Domain.Data.Multiplayer.Objectives;
using Nomad.Game.Domain.Interfaces.Multiplayer;
using Nomad.Game.Domain.Events.Multiplayer;
using Nomad.Game.Domain.Data.Multiplayer.Modes;
using Nomad.Networking.Messaging;
using Nomad.Game.Domain.Data.Multiplayer.Team;
using System;

namespace Nomad.Game.Application.Multiplayer
{
	/*
	===================================================================================

	CaptureTheFlagMode

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class CaptureTheFlagMode : ModeBase, ICaptureTheFlagMode
	{
		private enum RoundState : byte
		{
			Waiting,
			Active,
			Ended
		};

		private struct HostState
		{
			public SessionId SessionId { get; set; }
			public uint RedTeamScore { get; set; }
			public uint BlueTeamScore { get; set; }
			public uint RoundIndex { get; set; }

			public FlagStatus RedFlagStatus { get; set; }
			public FlagStatus BlueFlagStatus { get; set; }
		};

		public override string ModeName => "Capture The Flag";
		public override Mode Mode => Mode.CaptureTheFlag;

		public IGameEvent<FlagStatusChangedEventArgs> FlagStatusChanged => _flagStatusChanged;
		private readonly IGameEvent<FlagStatusChangedEventArgs> _flagStatusChanged = default;

		public IGameEvent<CTFRoundBeginEventArgs> CTFRoundBegin => _ctfRoundBegin;
		private readonly IGameEvent<CTFRoundBeginEventArgs> _ctfRoundBegin = default;

		public IGameEvent<CTFRoundEndEventArgs> CTFRoundEnd => _ctfRoundEnd;
		private readonly IGameEvent<CTFRoundEndEventArgs> _ctfRoundEnd = default;

		public CaptureTheFlagSnapshot Snapshot => new CaptureTheFlagSnapshot {
			RedTeamScore = _hostState.RedTeamScore,
			BlueTeamScore = _hostState.BlueTeamScore,
			RoundIndex = _hostState.RoundIndex
		};

		private HostState _hostState;
		private readonly MultiplayerStateMachine<RoundState> _roundFlow;

		private readonly ITeamService _teamService;

		public CaptureTheFlagMode(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory,
			ITeamService teamService
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
			_teamService = teamService ?? throw new ArgumentNullException( nameof( teamService ) );

			_flagStatusChanged = eventFactory.GetEvent<FlagStatusChangedEventArgs>(
				FlagStatusChangedEventArgs.Name,
				FlagStatusChangedEventArgs.NameSpace
			);
		}

		public bool TryBeginRound()
		{
			if ( !IsHost ) {
				return false;
			}
			PublishHostEvent( _ctfRoundBegin, new CTFRoundBeginEventArgs() );
			return true;
		}

		public bool TryEndRound( CaptureTheFlagRoundEndReason reason )
		{
			if ( !IsHost ) {
				return false;
			}
			return true;
		}

		public bool TryGetTeamScore( TeamId teamId, out int score )
		{
			score = 0;
			return true;
		}
	};
};
