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
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Game.Sdk.Multiplayer.Lobby;
using Nomad.Game.Sdk.Events.Multiplayer;

namespace Nomad.Game.Sdk.Multiplayer
{
    public interface ILobbyWaitingRoomService : IDisposable
    {
        LobbyWaitingRoomState State { get; }
        uint StateVersion { get; }
        uint RosterVersion { get; }

        bool IsOpen { get; }
        bool CanStart { get; }
        bool CanReady { get; }
        bool CanChangeSettings { get; }

        LobbyWaitingRoomInfo? Current { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer", PayloadName = "LobbyWaitingRoomStateChangedEventArgs")]
        [EventPayload("PreviousState", typeof(LobbyWaitingRoomState), Order = 1)]
        [EventPayload("CurrentState", typeof(LobbyWaitingRoomState), Order = 2)]
        [EventPayload("Version", typeof(uint), Order = 3)]
        IGameEvent<LobbyWaitingRoomStateChangedEventArgs> WaitingRoomStateChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer", PayloadName = "LobbyPeerReadyChangedEventArgs")]
        [EventPayload("PeerId", typeof(PeerId), Order = 1)]
        [EventPayload("PreviousState", typeof(LobbyReadyState), Order = 2)]
        [EventPayload("CurrentState", typeof(LobbyReadyState), Order = 3)]
        [EventPayload("Version", typeof(uint), Order = 4)]
        IGameEvent<LobbyPeerReadyChangedEventArgs> PeerReadyChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer", PayloadName = "LobbyCountdownStartedEventArgs")]
        [EventPayload("Seconds", typeof(int), Order = 1)]
        [EventPayload("Version", typeof(uint), Order = 2)]
        IGameEvent<LobbyCountdownStartedEventArgs> CountdownStarted { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer", PayloadName = "LobbyCountdownCancelledEventArgs")]
        [EventPayload("Version", typeof(uint))]
        IGameEvent<LobbyCountdownCancelledEventArgs> CountdownCancelled { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer", PayloadName = "LobbyGameStartRequestedEventArgs")]
        [EventPayload("Reason", typeof(LobbyGameStartReason), Order = 1)]
        [EventPayload("Version", typeof(uint), Order = 2)]
        IGameEvent<LobbyGameStartRequestedEventArgs> GameStartRequested { get; }

        void Frame();

        bool RequestReady();
        bool RequestNotReady();
        bool RequestToggleReady();
        bool RequestStartGame();
        bool RequestCancelCountdown();

        bool OpenWaitingRoom();
        bool CloseWaitingRoom();
        bool MarkInGame();

        bool SetPeerReadyState(PeerId peerId, LobbyReadyState readyState);
        bool ResetReadyStates();

        bool StartCountdown(int seconds);
        bool CancelCountdown();
        bool StartGame(LobbyGameStartReason reason = LobbyGameStartReason.HostStarted);

        bool SetMinPlayers(int minPlayers);
        bool SetLateJoinAllowed(bool allowed);

        bool TryGetPeerStatus(PeerId peerId, out LobbyPeerStatus status);
        bool IsPeerReady(PeerId peerId);
        int GetReadyCount();
        int GetPeerCount();
        int CopyPeers(LobbyPeerStatus[] destination);
    }
}
