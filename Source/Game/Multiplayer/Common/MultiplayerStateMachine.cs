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
using System.Collections.Generic;
using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Multiplayer
{
	internal sealed class MultiplayerStateMachine<TState>
		where TState : struct
	{
		public delegate bool TransitionGuard( TState currentState, TState nextState );
		public delegate void TransitionCallback( TState previousState, TState currentState, uint stateVersion );

		public TState State { get; private set; }
		public uint StateVersion => _stateVersion;

		public event TransitionCallback Transitioned;

		private readonly TransitionGuard _guard;
		private uint _stateVersion = 0;

		public MultiplayerStateMachine( TState initialState = default, TransitionGuard guard = null )
		{
			State = initialState;
			_guard = guard ?? throw new ArgumentNullException( nameof( guard ) );
		}

		public bool Is( TState state )
		{
			return EqualityComparer<TState>.Default.Equals( State, state );
		}

		public bool IsAny( params TState[] states )
		{
			for ( int i = 0; i < states.Length; i++ ) {
				if ( EqualityComparer<TState>.Default.Equals( State, states[i] ) ) {
					return true;
				}
			}

			return false;
		}

		public bool CanTransitionTo( TState nextState )
		{
			return _guard == null || _guard.Invoke( State, nextState );
		}

		public bool TransitionTo( TState nextState, bool allowSameState = false )
		{
			if ( !allowSameState && EqualityComparer<TState>.Default.Equals( State, nextState ) ) {
				return false;
			}

			if ( !CanTransitionTo( nextState ) ) {
				return false;
			}

			TState previousState = State;
			State = nextState;
			AdvanceVersion();
			Transitioned?.Invoke( previousState, State, _stateVersion );
			return true;
		}

		public bool TransitionHost( MultiplayerObject obj, TState nextState, TransitionCallback replicate = null, bool allowSameState = false )
		{
			ArgumentGuard.ThrowIfNull( obj, nameof( obj ) );

			if ( !obj.IsHost ) {
				return false;
			}

			TState previousState = State;
			if ( !TransitionTo( nextState, allowSameState ) ) {
				return false;
			}

			replicate?.Invoke( previousState, State, _stateVersion );
			return true;
		}

		public bool ApplyReplicated( TState nextState, uint stateVersion )
		{
			if ( IsStaleVersion( stateVersion ) ) {
				return false;
			}

			if ( !CanTransitionTo( nextState ) ) {
				return false;
			}

			TState previousState = State;
			State = nextState;
			_stateVersion = stateVersion;
			Transitioned?.Invoke( previousState, State, _stateVersion );
			return true;
		}

		public bool IsStaleVersion( uint stateVersion )
		{
			if ( stateVersion == 0 || _stateVersion == 0 ) {
				return false;
			}

			return stateVersion <= _stateVersion;
		}

		public uint AdvanceVersion()
		{
			unchecked {
				_stateVersion++;
			}
			return _stateVersion;
		}

		public void ResetVersion()
		{
			_stateVersion = 0;
		}

		public void Reset( TState state = default )
		{
			State = state;
			_stateVersion = 0;
		}
	};
};
