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
using System.Numerics;
using Nomad.Core.Events;
using Nomad.Core.OnlineServices;
using Nomad.Events.Globals;
using Nomad.Game.Domain.Data.Player;
using Nomad.Game.Domain.Interfaces.Player;
using Nomad.Input;
using Nomad.Input.ValueObjects;

namespace Nomad.Game.Application.Gameplay.Player.Input
{
	/*
	===================================================================================

	LocalPlayerInputSource

	===================================================================================
	*/
	/// <summary>
	/// Input source for the locally controlled player.
	/// Converts Nomad input events into normalized gameplay input frames.
	/// </summary>

	internal sealed class LocalPlayerInputSource : IPlayerInputSource
	{
		public PeerId PeerId => _peerId;
		private readonly PeerId _peerId;

		public bool IsEnabled {
			get => _isEnabled;
			set => _isEnabled = value;
		}
		private bool _isEnabled = true;

		public PlayerInputFrame Current => _current;

		private readonly IGameEvent<ButtonActionEventArgs> _slideAction;
		private readonly IGameEvent<AxisActionEventArgs> _moveAction;

		private Vector2 _moveInput = Vector2.Zero;
		private PlayerInputButtons _buttonsDown = PlayerInputButtons.None;
		private PlayerInputButtons _buttonsPressed = PlayerInputButtons.None;

		private PlayerInputFrame _current = PlayerInputFrame.Empty;
		private uint _lastTick = uint.MaxValue;
		private ushort _sequence;
		private bool _isDisposed;

		/*
		===============
		LocalPlayerInputSource
		===============
		*/
		public LocalPlayerInputSource( PeerId peerId )
			: this( peerId, GameEventRegistry.Instance )
		{
		}

		/*
		===============
		LocalPlayerInputSource
		===============
		*/
		public LocalPlayerInputSource( PeerId peerId, IGameEventRegistryService eventFactory )
		{
			if ( eventFactory == null ) {
				throw new ArgumentNullException( nameof( eventFactory ) );
			}

			_peerId = peerId;

			_slideAction = eventFactory.GetEvent<ButtonActionEventArgs>(
				$"Slide:{ButtonActionEventArgs.Name}",
				ButtonActionEventArgs.NameSpace
			);

			_moveAction = eventFactory.GetEvent<AxisActionEventArgs>(
				$"Move:{AxisActionEventArgs.Name}",
				AxisActionEventArgs.NameSpace
			);

			_slideAction.Subscribe( OnSlideActionTriggered );
			_moveAction.Subscribe( OnMoveActionTriggered );
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

			_slideAction.Unsubscribe( OnSlideActionTriggered );
			_moveAction.Unsubscribe( OnMoveActionTriggered );

			_isDisposed = true;
			GC.SuppressFinalize( this );
		}

		/*
		===============
		ReadFrame
		===============
		*/
		public PlayerInputFrame ReadFrame( uint tick )
		{
			if ( _lastTick == tick ) {
				return _current;
			}

			_lastTick = tick;

			if ( !_isEnabled ) {
				_current = new PlayerInputFrame(
					_peerId,
					tick,
					_sequence++,
					Vector2.Zero,
					PlayerInputButtons.None,
					PlayerInputButtons.None
				);
				_buttonsPressed = PlayerInputButtons.None;
				return _current;
			}

			_current = new PlayerInputFrame(
				_peerId,
				tick,
				_sequence++,
				_moveInput,
				_buttonsDown,
				_buttonsPressed
			);

			// ButtonPressed is a one-frame edge. ButtonDown persists.
			_buttonsPressed = PlayerInputButtons.None;

			return _current;
		}

		/*
		===============
		Reset
		===============
		*/
		public void Reset()
		{
			_moveInput = Vector2.Zero;
			_buttonsDown = PlayerInputButtons.None;
			_buttonsPressed = PlayerInputButtons.None;
			_current = PlayerInputFrame.Empty.WithPeer( _peerId );
			_lastTick = uint.MaxValue;
			_sequence = 0;
		}

		/*
		===============
		OnMoveActionTriggered
		===============
		*/
		private void OnMoveActionTriggered( in AxisActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started || args.Phase == InputActionPhase.Performed ) {
				_moveInput = args.Value;

				// Preserve the existing controller behavior: input Y is inverted
				// for the 2D gameplay plane.
				_moveInput.Y = -_moveInput.Y;
				return;
			}

			_moveInput = Vector2.Zero;
		}

		/*
		===============
		OnSlideActionTriggered
		===============
		*/
		private void OnSlideActionTriggered( in ButtonActionEventArgs args )
		{
			if ( args.Phase == InputActionPhase.Started ) {
				_buttonsDown |= PlayerInputButtons.Slide;
				_buttonsPressed |= PlayerInputButtons.Slide;
				return;
			}

			if ( args.Phase == InputActionPhase.Canceled ) {
				_buttonsDown &= ~PlayerInputButtons.Slide;
			}
		}
	}
}
