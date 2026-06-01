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

using System.Threading;
using System.Threading.Tasks;
using Nomad.Core.Events;

namespace Nomad.Game.Sdk.Gameplay
{
    /// <summary>
    ///
    /// </summary>
    public interface IGameSessionService
    {
        GameSessionState State { get; }
        GameSessionKind Kind { get; }
        bool IsTransitioning { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Gameplay", EventPayloadName = "GameSessionTransitionStartedEventArgs")]
        [EventPayload("Kind", typeof(GameSessionKind), Order = 1)]
        [EventPayload("State", typeof(GameSessionState), Order = 2)]
        [EventPayload("Message", typeof(string), Order = 3)]
        IGameEvent<GameSessionTransitionStartedEventArgs> TransitionStarted { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Gameplay", EventPayloadName = "GameSessionTransitionCompletedEventArgs")]
        [EventPayload("Kind", typeof(GameSessionKind), Order = 1)]
        [EventPayload("State", typeof(GameSessionState), Order = 2)]
        [EventPayload("Message", typeof(string), Order = 3)]
        IGameEvent<GameSessionTransitionCompletedEventArgs> TransitionCompleted { get; }

        [Event(nameSpace: "Nomad.Game.Sdk.Gameplay", EventPayloadName = "GameSessionTransitionProgressChangedEventArgs")]
        [EventPayload("Kind", typeof(GameSessionKind), Order = 1)]
        [EventPayload("State", typeof(GameSessionState), Order = 2)]
        [EventPayload("Message", typeof(string), Order = 3)]
        IGameEvent<GameSessionTransitionProgressChangedEventArgs> TransitionProgressChanged { get; }

        Task LoadSinglePlayerAsync(string saveName, CancellationToken ct = default);
        Task StartNewSinglePlayerAsync(CancellationToken ct = default);
        Task StartMultiplayerAsync(CancellationToken ct = default);

        void MarkWorldReady();
        void Clear();
    }
}
