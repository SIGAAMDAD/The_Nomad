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
			public byte RedTeamScore { get; set; }
			public byte BlueTeamScore { get; set; }
			public byte RoundIndex { get; set; }

			public FlagStatus RedFlagStatus { get; set; }
			public FlagStatus BlueFlagStatus { get; set; }
		};

		public override string ModeName => "Capture The Flag";
		public override Mode Mode => Mode.CaptureTheFlag;

		public IGameEvent<FlagStatusChangedEventArgs> FlagStatusChanged => _flagStatusChanged;
		private readonly IGameEvent<FlagStatusChangedEventArgs> _flagStatusChanged = default;

		public CaptureTheFlagInstanceData Snapshot => new CaptureTheFlagInstanceData {
			RedTeamScore = _hostState.RedTeamScore,
			BlueTeamScore = _hostState.BlueTeamScore,
			RedFlagState = _hostState.RedFlagStatus,
			BlueFlagState = _hostState.BlueFlagStatus,
			RoundIndex = _hostState.RoundIndex
		};

		private HostState _hostState;
		private readonly MultiplayerStateMachine<RoundState> _roundFlow;

		public CaptureTheFlagMode(
			INetworkSessionService sessionService,
			INetworkRpcBus rpcBus,
			INetworkEventBus eventBus,
			INetworkMessageRegistry messageRegistry,
			IGameEventRegistryService eventFactory
		)
			: base( sessionService, rpcBus, eventBus, messageRegistry, eventFactory )
		{
			_flagStatusChanged = eventFactory.GetEvent<FlagStatusChangedEventArgs>(
				FlagStatusChangedEventArgs.Name,
				FlagStatusChangedEventArgs.NameSpace
			);
		}
	};
};
