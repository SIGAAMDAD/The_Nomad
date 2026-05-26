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
using Nomad.Game.Sdk.Multiplayer.Voting;
using Nomad.Game.Sdk.Events.Multiplayer;

namespace Nomad.Game.Sdk.Multiplayer
{
    public interface IVotingService : IDisposable
    {
        VoteServiceState State { get; }
        uint StateVersion { get; }
        uint VoteVersion { get; }

        bool HasActiveVote { get; }
        bool CanRequestVote { get; }
        bool CanCastVote { get; }

        VoteInfo? CurrentVote { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("PreviousState", typeof(VoteServiceState), Order = 1)]
        [EventPayload("CurrentState", typeof(VoteServiceState), Order = 2)]
        [EventPayload("Version", typeof(uint), Order = 3)]
        IGameEvent<VoteServiceStateChangedEventArgs> VoteServiceStateChanged { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("VoteId", typeof(VoteId), Order = 1)]
        [EventPayload("Kind", typeof(VoteKind), Order = 2)]
        [EventPayload("StartedBy", typeof(PeerId), Order = 3)]
        [EventPayload("TargetPeer", typeof(PeerId), Order = 4)]
        [EventPayload("OptionCount", typeof(byte), Order = 5)]
        [EventPayload("Version", typeof(uint), Order = 6)]
        IGameEvent<VoteStartedEventArgs> VoteStarted { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("VoteId", typeof(VoteId), Order = 1)]
        [EventPayload("PeerId", typeof(PeerId), Order = 2)]
        [EventPayload("OptionId", typeof(VoteOptionId), Order = 3)]
        [EventPayload("Version", typeof(uint), Order = 4)]
        IGameEvent<VoteCastEventArgs> VoteCast { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("VoteId", typeof(VoteId), Order = 1)]
        [EventPayload("Kind", typeof(VoteKind), Order = 2)]
        [EventPayload("Passed", typeof(bool), Order = 3)]
        [EventPayload("WinningOptionId", typeof(VoteOptionId), Order = 4)]
        [EventPayload("Reason", typeof(VoteEndReason), Order = 5)]
        [EventPayload("Version", typeof(uint), Order = 6)]
        IGameEvent<VoteEndedEventArgs> VoteEnded { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Events.Multiplayer")]
        [EventPayload("VoteId", typeof(VoteId), Order = 1)]
        [EventPayload("Reason", typeof(VoteEndReason), Order = 2)]
        [EventPayload("Version", typeof(uint), Order = 3)]
        IGameEvent<VoteCancelledEventArgs> VoteCancelled { get; }

        bool RequestStartGameVote();
        bool CastVote(VoteOptionId optionId);
        bool Abstain();

        bool StartGameStartVote(PeerId startedBy);
        bool CastPeerVote(PeerId peerId, VoteOptionId optionId);
        bool CancelVote(VoteEndReason reason = VoteEndReason.Cancelled);
        bool ResolveVote();

        bool LockVoting();
        bool UnlockVoting();
        bool ResetVoting();

        bool TryGetVote(VoteId voteId, out VoteInfo info);
        bool TryGetOption(VoteOptionId optionId, out VoteOptionInfo option);
        bool TryGetPeerVote(PeerId peerId, out VoteOptionId optionId);

        bool HasPeerVoted(PeerId peerId);
        int GetVoteCount(VoteOptionId optionId);
        int GetTotalVotesCast();

        int CopyOptions(VoteOptionInfo[] destination);
        int CopyBallots(VoteBallotInfo[] destination);
    }
}
