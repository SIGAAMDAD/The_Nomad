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
using Nomad.Game.Sdk.Multiplayer.Team;
using Nomad.Game.Sdk.Events.Multiplayer;

namespace Nomad.Game.Sdk.Multiplayer
{
    public interface ITeamService : IDisposable
    {
        TeamServiceState State { get; }

        /// <summary>
        /// Version of the team service lifecycle state.
        /// Changes when State changes.
        /// </summary>
        uint StateVersion { get; }

        /// <summary>
        /// Version of the peer-to-team assignment table.
        /// Changes when a peer joins, leaves, moves team, or teams reset.
        /// </summary>
        uint TeamVersion { get; }

        /// <summary>
        /// True when the local peer may request or perform a team assignment change.
        /// Usually true only while the service is Open and a network session is active.
        /// </summary>
        bool CanRequestTeamChange { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("PreviousState", typeof(TeamServiceState), Order = 1)]
        [EventPayload("CurrentState", typeof(TeamServiceState), Order = 2)]
        [EventPayload("Version", typeof(uint), Order = 3)]
        IGameEvent<TeamServiceStateChangedEventArgs> TeamServiceStateChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("PeerId", typeof(PeerId), Order = 1)]
        [EventPayload("PreviousTeam", typeof(TeamId), Order = 2)]
        [EventPayload("CurrentTeam", typeof(TeamId), Order = 3)]
        [EventPayload("Reason", typeof(TeamChangeReason), Order = 4)]
        [EventPayload("Version", typeof(uint), Order = 5)]
        IGameEvent<TeamChangedEventArgs> TeamChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("Version", typeof(uint))]
        IGameEvent<TeamsResetEventArgs> TeamsReset { get; }

        //
        // Client requests
        //

        bool RequestJoinTeam(TeamId teamId);
        bool RequestLeaveTeam();
        bool RequestAutoAssign();

        //
        // Host only
        //

        bool SetPeerTeam(PeerId peerId, TeamId teamId);
        bool AutoAssignPeer(PeerId peerId);
        bool RemovePeer(PeerId peerId, TeamChangeReason reason = TeamChangeReason.Left);

        bool OpenTeams();
        bool LockTeams();
        bool BeginMatch();
        bool CloseTeams();
        bool ResetTeams();

        //
        // Utility
        //

        bool TryGetTeam(PeerId peerId, out TeamId teamId);
        bool TryGetTeamInfo(TeamId teamId, out TeamInfo info);

        bool IsPeerAssigned(PeerId peerId);

        int GetTeamMemberCount(TeamId teamId);
        int CopyTeamMembers(TeamId teamId, PeerId[] destination);
    }
}
