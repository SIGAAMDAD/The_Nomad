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
using Nomad.Core.Compatibility.Guards;
using Nomad.Core.CVars;
using Nomad.Core.Engine.Windowing;
using Nomad.Core.Events;
using Nomad.Core.Input;
using Nomad.CVars;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk;
using Nomad.Game.Sdk.Player;
using Nomad.Input;
using Nomad.Input.Interfaces;
using Nomad.Input.ValueObjects;
using Nomad.Game.Sdk.Player.Input;

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
		public PlayerId PlayerId => _playerId;
		private readonly PlayerId _playerId;
		private readonly int _localSlot;
		private readonly IInputDeviceSlotService _inputDeviceSlots;

		public bool IsEnabled {
			get => _isEnabled;
			set => _isEnabled = value;
		}
		private bool _isEnabled = true;

		public PlayerInputFrame Current => _current;

		private readonly IDisposable _slideAction;
		private readonly IDisposable _dashAction;
		private readonly IDisposable _interactAction;
		private readonly IDisposable _jumpAction;
		private readonly IDisposable _dropAction;
		private readonly IDisposable _moveAction;
		private readonly IDisposable _lookAction;

		private readonly IDisposable _windowSizeChanged;

		private float _mouseAngle = 0.0f;
		private Vector2 _mouseDirection = Vector2.Zero;
		private Vector2 _windowSize = Vector2.Zero;

		private Vector2 _moveInput = Vector2.Zero;
		private PlayerInputButtons _buttonsDown = PlayerInputButtons.None;
		private PlayerInputButtons _buttonsPressed = PlayerInputButtons.None;

		private PlayerInputFrame _current = PlayerInputFrame.Empty;
		private uint _lastTick = uint.MaxValue;
		private ushort _sequence = 0;
		private bool _isDisposed = false;

		/*
		===============
		LocalPlayerInputSource
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="playerId"></param>
		/// <param name="eventFactory"></param>
		public LocalPlayerInputSource( PlayerId playerId, int localSlot, ICVarSystemService cvarSystem, IInputDeviceSlotService inputDeviceSlots, IGameEventRegistryService eventFactory )
		{
			ArgumentGuard.ThrowIfNull( eventFactory, nameof( eventFactory ) );
			ArgumentGuard.ThrowIfNull( cvarSystem, nameof( cvarSystem ) );
			ArgumentGuard.ThrowIfNull( inputDeviceSlots, nameof( inputDeviceSlots ) );

			var windowSize = cvarSystem.GetCVarOrThrow<WindowResolution>( Core.Constants.CVars.EngineUtils.Display.WINDOW_RESOLUTION );
			var size = (WindowSize)windowSize.Value;
			_windowSize = new Vector2( size.Width, size.Height ) * 0.5f;

			_windowSizeChanged = windowSize.ValueChanged.Subscribe( OnWindowSizeChanged );

			_playerId = playerId;
			_localSlot = localSlot;
			_inputDeviceSlots = inputDeviceSlots;

			_slideAction = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"Slide:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( ( in ButtonActionEventArgs args ) => OnButtonActionTriggered( in args, PlayerInputButtons.Slide ) );

			_dashAction = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"Dash:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( ( in ButtonActionEventArgs args ) => OnButtonActionTriggered( in args, PlayerInputButtons.Dash ) );

			_interactAction = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"Interact:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( ( in ButtonActionEventArgs args ) => OnButtonActionTriggered( in args, PlayerInputButtons.Interact ) );

			_jumpAction = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"Jump:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( ( in ButtonActionEventArgs args ) => OnButtonActionTriggered( in args, PlayerInputButtons.Jump ) );

			_dropAction = eventFactory
				.GetEvent<ButtonActionEventArgs>(
					$"Drop:{ButtonActionEventArgs.Name}",
					ButtonActionEventArgs.NameSpace
				)
				.Subscribe( ( in ButtonActionEventArgs args ) => OnButtonActionTriggered( in args, PlayerInputButtons.Drop ) );

			_moveAction = eventFactory
				.GetEvent<AxisActionEventArgs>(
					$"Move:{AxisActionEventArgs.Name}",
					AxisActionEventArgs.NameSpace
				)
				.Subscribe( OnMoveActionTriggered );

			_lookAction = eventFactory
				.GetEvent<MousePositionChangedEventArgs>(
					MousePositionChangedEventArgs.Name,
					MousePositionChangedEventArgs.NameSpace
				)
				.Subscribe( OnMousePositionChanged );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_slideAction?.Dispose();
			_dashAction?.Dispose();
			_interactAction?.Dispose();
			_jumpAction?.Dispose();
			_dropAction?.Dispose();
			_moveAction?.Dispose();
			_lookAction?.Dispose();
			_windowSizeChanged?.Dispose();

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		/*
		===============
		ReadFrame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="tick"></param>
		/// <returns></returns>
		public PlayerInputFrame ReadFrame( uint tick )
		{
			if ( _lastTick == tick ) {
				return _current;
			}

			_lastTick = tick;

			if ( !_isEnabled ) {
				_current = new PlayerInputFrame(
					_playerId,
					tick,
					_sequence++,
					Vector2.Zero,
					Vector2.Zero,
					0.0f,
					PlayerInputButtons.None,
					PlayerInputButtons.None
				);
				_buttonsPressed = PlayerInputButtons.None;
				return _current;
			}

			_current = new PlayerInputFrame(
				_playerId,
				tick,
				_sequence++,
				_moveInput,
				_mouseDirection,
				_mouseAngle,
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
		/// <summary>
		///
		/// </summary>
		public void Reset()
		{
			_moveInput = Vector2.Zero;
			_buttonsDown = PlayerInputButtons.None;
			_buttonsPressed = PlayerInputButtons.None;
			_current = PlayerInputFrame.Empty.WithPeer( _playerId );
			_lastTick = uint.MaxValue;
			_sequence = 0;
		}

		/*
		===============
		OnMoveActionTriggered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnMoveActionTriggered( in AxisActionEventArgs args )
		{
			if ( args.LocalSlot != _localSlot ) {
				return;
			}

			if ( args.Phase == InputActionPhase.Started || args.Phase == InputActionPhase.Performed ) {
				_moveInput = args.Value;

				// Preserve the existing input convention: positive Y means forward in
				// gameplay space. The 3D movement controller projects this onto the
				// camera-relative XZ ground plane.

				if ( _moveInput.LengthSquared() > 1.0f ) {
					_moveInput = Vector2.Normalize( _moveInput );
				}

				return;
			}
			_moveInput = Vector2.Zero;
		}

		/*
		===============
		OnMousePositionChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnMousePositionChanged( in MousePositionChangedEventArgs args )
		{
			if ( !_inputDeviceSlots.IsAssignedToLocalSlot( InputDeviceSlot.Mouse, _localSlot ) ) {
				return;
			}

			var position = new Vector2( args.PositionX, args.PositionY );
			_mouseDirection = position - _windowSize;
			_mouseAngle = MathF.Atan2( _mouseDirection.Y, _mouseDirection.X );
		}

		/*
		===============
		OnButtonActionTriggered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnButtonActionTriggered( in ButtonActionEventArgs args, PlayerInputButtons button )
		{
			if ( args.LocalSlot != _localSlot ) {
				return;
			}

			if ( args.Phase == InputActionPhase.Started ) {
				_buttonsDown |= button;
				_buttonsPressed |= button;
				return;
			}

			if ( args.Phase == InputActionPhase.Canceled ) {
				_buttonsDown &= ~button;
			}
		}

		/*
		===============
		OnWindowSizeChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWindowSizeChanged( in CVarValueChangedEventArgs<WindowResolution> args )
		{
			var size = (WindowSize)args.NewValue;
			_windowSize = new Vector2( size.Width, size.Height ) * 0.5f;
		}
	};
};
