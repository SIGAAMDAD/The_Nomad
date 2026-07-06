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
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player;
using Nomad.Game.Sdk.Player.Input;

namespace Nomad.Game.Gameplay.Player.Input
{
	/*
	===================================================================================

	RemotePlayerInputSource

	===================================================================================
	*/
	/// <summary>
	/// Input source for a player simulated locally from network-delivered input.
	/// Typically used by the host for remote client players.
	/// </summary>

	internal sealed class RemotePlayerInputSource : IPlayerInputSource
	{
		private const int DEFAULT_BUFFER_SIZE = 64;

		public PlayerId PlayerId => _playerId;
		private readonly PlayerId _playerId;

		public bool IsEnabled {
			get => _isEnabled;
			set => _isEnabled = value;
		}
		private bool _isEnabled = true;

		public PlayerInputFrame Current => _current;

		private readonly PlayerInputFrame[] _buffer;
		private int _head;
		private int _count;

		private PlayerInputFrame _current;
		private ushort _lastSequence;
		private bool _hasSequence;
		private bool _isDisposed;

		/*
		===============
		RemotePlayerInputSource
		===============
		*/
		public RemotePlayerInputSource( PlayerId playerId, int bufferSize = DEFAULT_BUFFER_SIZE )
		{
			if ( bufferSize < 1 ) {
				bufferSize = DEFAULT_BUFFER_SIZE;
			}

			_playerId = playerId;
			_buffer = new PlayerInputFrame[bufferSize];
			_current = PlayerInputFrame.Empty.WithPeer( playerId );
		}

		/*
		===============
		Dispose
		===============
		*/
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			Reset();
			_isDisposed = true;
			GC.SuppressFinalize( this );
		}

		/*
		===============
		PushFrame
		===============
		*/
		public bool PushFrame( in PlayerInputFrame frame )
		{
			if ( !_isEnabled ) {
				return false;
			}

			if ( frame.PlayerId != _playerId ) {
				return false;
			}

			if ( IsStaleSequence( frame.Sequence ) ) {
				return false;
			}

			int index = (_head + _count) % _buffer.Length;
			_buffer[index] = frame;

			if ( _count == _buffer.Length ) {
				_head = (_head + 1) % _buffer.Length;
			} else {
				_count++;
			}

			_lastSequence = frame.Sequence;
			_hasSequence = true;
			return true;
		}

		/*
		===============
		ReadFrame
		===============
		*/
		public PlayerInputFrame ReadFrame( uint tick )
		{
			if ( !_isEnabled ) {
				_current = PlayerInputFrame.Empty.WithPeer( _playerId ).WithTick( tick );
				return _current;
			}

			if ( _count > 0 ) {
				_current = _buffer[_head].WithTick( tick );
				_head = (_head + 1) % _buffer.Length;
				_count--;
				return _current;
			}

			// Keep movement held if packets briefly stop arriving, but never
			// repeat edge-triggered buttons like SlidePressed.
			_current = _current.WithoutTransientButtons( tick );
			return _current;
		}

		/*
		===============
		Reset
		===============
		*/
		public void Reset()
		{
			Array.Clear( _buffer, 0, _buffer.Length );
			_head = 0;
			_count = 0;
			_current = PlayerInputFrame.Empty.WithPeer( _playerId );
			_lastSequence = 0;
			_hasSequence = false;
		}

		/*
		===============
		IsStaleSequence
		===============
		*/
		private bool IsStaleSequence( ushort sequence )
		{
			if ( !_hasSequence ) {
				return false;
			}

			// Accept wrap-around naturally. Values that are more than half the
			// ushort range behind the current sequence are treated as old.
			unchecked {
				ushort delta = (ushort)(sequence - _lastSequence);
				return delta == 0 || delta > short.MaxValue;
			}
		}
	}
}
