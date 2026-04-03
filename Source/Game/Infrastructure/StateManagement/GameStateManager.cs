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
using Nomad.Game.Domain.Data.StateManagement;
using Nomad.Game.Domain.Events.StateManagement;
using System;

namespace Nomad.Game.Infrastructure.StateManagement {
	/*
	===================================================================================
	
	GameStateManager
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class GameStateManager : IDisposable {
		/// <summary>
		/// 
		/// </summary>
		public static GameState CurrentState {
			get => _instance._currentState;
			set => _instance.SetState( value );
		}
		private GameState _currentState = GameState.Menu;

		/// <summary>
		/// 
		/// </summary>
		public static IGameEvent<GameStateChangedEventArgs> StateChanged => _instance._stateChanged;
		private readonly IGameEvent<GameStateChangedEventArgs> _stateChanged;

		private static GameStateManager? _instance = null;

		private bool _isDisposed = false;

		/*
		===============
		GameStateManager
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventFactory"></param>
		public GameStateManager( IGameEventRegistryService eventFactory ) {
			_stateChanged = eventFactory.GetEvent<GameStateChangedEventArgs>( EventNames.GAME_STATE_CHANGED, EventNames.NAMESPACE );
			_instance = this;
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			if ( !_isDisposed ) {
				_stateChanged?.Dispose();
			}
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		SetState
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="newState"></param>
		private void SetState( GameState newState ) {
			if ( newState == _currentState ) {
				return;
			}
			var oldState = _currentState;
			_currentState = newState;
			_stateChanged.Publish( new GameStateChangedEventArgs( oldState, _currentState ) );
		}
	};
};
