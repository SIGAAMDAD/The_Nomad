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
using System.Threading;
using System.Threading.Tasks;
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.Events;
using Nomad.Game.Sdk.Events.Gameplay;
using Nomad.Game.Sdk.Gameplay;
using Nomad.Save.Services;

namespace Nomad.Game.Application.Gameplay
{
	internal sealed class GameSessionService : IGameSessionService, IDisposable
	{
		public GameSessionState State { get; private set; } = GameSessionState.None;
		public GameSessionKind Kind { get; private set; } = GameSessionKind.None;

		public bool IsTransitioning { get; private set; } = false;

		private readonly object _lock = new();

		private readonly IGameStateService _gameStateService;
		private readonly ISaveDataProvider _dataProvider;
		private readonly IGameEvent<WorldBootstrapRequestEventArgs> _worldBootstrapRequested;
		private readonly IDisposable _worldBootstrapSucceeded;
		private readonly IDisposable _worldBootstrapFailed;

		private bool _isDisposed = false;

		public IGameEvent<GameSessionTransitionStartedEventArgs> TransitionStarted => _transitionStarted;
		private readonly IGameEvent<GameSessionTransitionStartedEventArgs> _transitionStarted;

		public IGameEvent<GameSessionTransitionCompletedEventArgs> TransitionCompleted => _transitionCompleted;
		private readonly IGameEvent<GameSessionTransitionCompletedEventArgs> _transitionCompleted;

		public IGameEvent<GameSessionTransitionProgressChangedEventArgs> TransitionProgressChanged => _transitionProgressChanged;
		private readonly IGameEvent<GameSessionTransitionProgressChangedEventArgs> _transitionProgressChanged;

		public GameSessionService( IGameStateService gameStateService, ISaveDataProvider dataProvider, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			_gameStateService = gameStateService ?? throw new ArgumentNullException( nameof( gameStateService ) );
			_dataProvider = dataProvider ?? throw new ArgumentNullException( nameof( dataProvider ) );

			_transitionStarted = eventFactory
				.GetEvent<GameSessionTransitionStartedEventArgs>(
					GameSessionTransitionStartedEventArgs.Name,
					GameSessionTransitionStartedEventArgs.NameSpace
				);

			_transitionCompleted = eventFactory
				.GetEvent<GameSessionTransitionCompletedEventArgs>(
					GameSessionTransitionCompletedEventArgs.Name,
					GameSessionTransitionCompletedEventArgs.NameSpace
				);

			_transitionProgressChanged = eventFactory
				.GetEvent<GameSessionTransitionProgressChangedEventArgs>(
					GameSessionTransitionProgressChangedEventArgs.Name,
					GameSessionTransitionProgressChangedEventArgs.NameSpace
				);

			_worldBootstrapRequested = eventFactory
				.GetEvent<WorldBootstrapRequestEventArgs>(
					WorldBootstrapRequestEventArgs.Name,
					WorldBootstrapRequestEventArgs.NameSpace
				);

			_worldBootstrapSucceeded = eventFactory
				.GetEvent<WorldBootstrapSucceededEventArgs>(
					WorldBootstrapSucceededEventArgs.Name,
					WorldBootstrapSucceededEventArgs.NameSpace
				)
				.Subscribe( OnWorldBootstrapSucceeded );

			_worldBootstrapFailed = eventFactory
				.GetEvent<WorldBootstrapFailureEventArgs>(
					WorldBootstrapFailureEventArgs.Name,
					WorldBootstrapFailureEventArgs.NameSpace
				)
				.Subscribe( OnWorldBootstrapFailed );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_transitionCompleted.Dispose();
			_transitionStarted.Dispose();
			_transitionProgressChanged.Dispose();
			_worldBootstrapRequested.Dispose();
			_worldBootstrapFailed.Dispose();
			_worldBootstrapSucceeded.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public Task LoadSinglePlayerAsync( string saveName, CancellationToken ct = default )
		{
			ct.ThrowIfCancellationRequested();

			if ( string.IsNullOrWhiteSpace( saveName ) ) {
				throw new ArgumentException( "Save name is required.", nameof( saveName ) );
			}

			BeginTransition( GameSessionKind.LoadedSinglePlayer, "Loading saved game..." );
			SetState( GameSessionState.LoadingData, "Loading save data..." );

			SetState( GameSessionState.LoadingScene, "Loading world..." );
			PublishWorldBootstrapRequest(
				WorldBootstrapMode.SinglePlayerLoadGame,
				"world.single.default",
				DifficultyPreset.Standard,
				saveName,
				null
			);

			return Task.CompletedTask;
		}

		public Task StartNewSinglePlayerAsync( CancellationToken ct = default )
		{
			ct.ThrowIfCancellationRequested();

			BeginTransition( GameSessionKind.NewSinglePlayer, "Preparing new game..." );
			SetState( GameSessionState.LoadingScene, "Loading world..." );

			PublishWorldBootstrapRequest(
				WorldBootstrapMode.SinglePlayerNewGame,
				"world.single.default",
				DifficultyPreset.Standard,
				string.Empty,
				null
			);

			return Task.CompletedTask;
		}

		public Task StartMultiplayerAsync( CancellationToken ct = default )
		{
			ct.ThrowIfCancellationRequested();

			BeginTransition( GameSessionKind.MultiplayerHost, "Preparing multiplayer session..." );
			SetState( GameSessionState.LoadingScene, "Loading world..." );

			PublishWorldBootstrapRequest(
				WorldBootstrapMode.MultiplayerHost,
				"world.single.default",
				DifficultyPreset.Standard,
				string.Empty,
				null
			);

			return Task.CompletedTask;
		}

		public void MarkWorldReady()
		{
			GameSessionKind kind;
			GameSessionState state;

			lock ( _lock ) {
				if ( _isDisposed || State == GameSessionState.Ready ) {
					return;
				}

				State = GameSessionState.Ready;
				IsTransitioning = false;
				kind = Kind;
				state = State;
			}

			_transitionProgressChanged.Publish(
				new GameSessionTransitionProgressChangedEventArgs(
					kind,
					state,
					"World ready."
				)
			);

			_transitionCompleted.Publish(
				new GameSessionTransitionCompletedEventArgs(
					kind,
					state,
					"World ready."
				)
			);
		}

		public void Clear()
		{
			lock ( _lock ) {
				Kind = GameSessionKind.None;
				State = GameSessionState.None;
				IsTransitioning = false;
			}
		}

		private void BeginTransition( GameSessionKind kind, string message )
		{
			lock ( _lock ) {
				if ( _isDisposed ) {
					throw new ObjectDisposedException( nameof( GameSessionService ) );
				}

				Kind = kind;
				State = GameSessionState.Preparing;
				IsTransitioning = true;
			}

			_gameStateService.Current = GameState.Loading;

			_transitionStarted.Publish(
				new GameSessionTransitionStartedEventArgs(
					Kind,
					State,
					message
				)
			);
		}

		private void SetState( GameSessionState state, string message )
		{
			GameSessionKind kind;

			lock ( _lock ) {
				State = state;
				kind = Kind;
			}

			_transitionProgressChanged.Publish(
				new GameSessionTransitionProgressChangedEventArgs(
					kind,
					state,
					message
				)
			);
		}

		private void PublishWorldBootstrapRequest(
			WorldBootstrapMode mode,
			string worldId,
			DifficultyPreset difficulty,
			string saveName,
			Guid? lobbyId
		)
		{
			_worldBootstrapRequested.Publish(
				new WorldBootstrapRequestEventArgs(
					Guid.NewGuid(),
					mode,
					worldId,
					difficulty,
					saveName,
					lobbyId
				)
			);
		}

		private void OnWorldBootstrapSucceeded( in WorldBootstrapSucceededEventArgs args )
		{
			SetState( GameSessionState.HydratingWorld, "Preparing world..." );
			MarkWorldReady();
		}

		private void OnWorldBootstrapFailed( in WorldBootstrapFailureEventArgs args )
		{
			GameSessionKind kind;

			lock ( _lock ) {
				State = GameSessionState.Failed;
				IsTransitioning = false;
				kind = Kind;
			}

			string message = $"World bootstrap failed: {args.Reason}";
			_transitionProgressChanged.Publish(
				new GameSessionTransitionProgressChangedEventArgs(
					kind,
					GameSessionState.Failed,
					message
				)
			);

			_transitionCompleted.Publish(
				new GameSessionTransitionCompletedEventArgs(
					kind,
					GameSessionState.Failed,
					message
				)
			);

			_gameStateService.Current = GameState.Menu;
		}
	};
};
