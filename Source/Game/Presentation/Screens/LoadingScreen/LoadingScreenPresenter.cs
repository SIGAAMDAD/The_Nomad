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
using Nomad.Game.Sdk.Gameplay;

namespace Nomad.Game.Presentation.Screens.LoadingScreen
{
	internal sealed class LoadingScreenPresenter : IDisposable
	{
		private readonly ILoadingScreenView _view;
		private readonly IGameEventRegistryService _eventFactory;

		private readonly string[] _tips = {
			"LOADING...",
			"STOKING THE FIRE...",
			"CHARTING THE WASTES..."
		};
		private int _currentTip = 0;

		private bool _isDisposed = false;

		public LoadingScreenPresenter( ILoadingScreenView view, IGameEventRegistryService eventFactory )
		{
			_view = view ?? throw new ArgumentNullException( nameof( view ) );
			_eventFactory = eventFactory ?? throw new ArgumentNullException( nameof( eventFactory ) );

			_view.SetTipText( _tips[_currentTip] );

			_eventFactory
				.GetEvent<GameSessionTransitionStartedEventArgs>(
					GameSessionTransitionStartedEventArgs.Name,
					GameSessionTransitionStartedEventArgs.NameSpace
				)
				.Subscribe( OnTransitionStarted );

			_eventFactory
				.GetEvent<GameSessionTransitionProgressChangedEventArgs>(
					GameSessionTransitionProgressChangedEventArgs.Name,
					GameSessionTransitionProgressChangedEventArgs.NameSpace
				)
				.Subscribe( OnTransitionProgressChanged );

			_eventFactory
				.GetEvent<GameSessionTransitionCompletedEventArgs>(
					GameSessionTransitionCompletedEventArgs.Name,
					GameSessionTransitionCompletedEventArgs.NameSpace
				)
				.Subscribe( OnTransitionCompleted );
		}

		private void OnTransitionStarted( in GameSessionTransitionStartedEventArgs args )
		{
			_currentTip = 0;
			_view.SetTipText( string.IsNullOrWhiteSpace( args.Message ) ? _tips[_currentTip] : args.Message );
			_view.Show();
		}

		private void OnTransitionProgressChanged( in GameSessionTransitionProgressChangedEventArgs args )
		{
			if ( string.IsNullOrWhiteSpace( args.Message ) ) {
				_currentTip = ( _currentTip + 1 ) % _tips.Length;
				_view.SetTipText( _tips[_currentTip] );
				return;
			}

			_view.SetTipText( args.Message );
		}

		private void OnTransitionCompleted( in GameSessionTransitionCompletedEventArgs args )
		{
			if ( args.State == GameSessionState.Failed ) {
				_view.Hide();
			}
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_eventFactory
				.GetEvent<GameSessionTransitionStartedEventArgs>(
					GameSessionTransitionStartedEventArgs.Name,
					GameSessionTransitionStartedEventArgs.NameSpace
				)
				.Unsubscribe( OnTransitionStarted );

			_eventFactory
				.GetEvent<GameSessionTransitionProgressChangedEventArgs>(
					GameSessionTransitionProgressChangedEventArgs.Name,
					GameSessionTransitionProgressChangedEventArgs.NameSpace
				)
				.Unsubscribe( OnTransitionProgressChanged );

			_eventFactory
				.GetEvent<GameSessionTransitionCompletedEventArgs>(
					GameSessionTransitionCompletedEventArgs.Name,
					GameSessionTransitionCompletedEventArgs.NameSpace
				)
				.Unsubscribe( OnTransitionCompleted );

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}
	};
};
