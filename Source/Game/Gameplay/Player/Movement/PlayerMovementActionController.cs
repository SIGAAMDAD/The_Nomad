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
using Nomad.Game.Sdk.Events.Player.JumpKit;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.JumpKit;
using Nomad.Game.Sdk.Player.State;

namespace Nomad.Game.Gameplay.Player.Movement
{
	/*
	===================================================================================

	PlayerMovementActionController

	===================================================================================
	*/
	/// <summary>
	/// Handles transient dash/slide runtime state and writes action intent into the
	/// current PlayerMovementFrame. This replaces the out-of-band slide Timer with a
	/// deterministic physics-frame countdown.
	/// </summary>

	internal sealed class PlayerMovementActionController : IDisposable
	{
		private const PlayerJumpKitFlags DASH_END_FLAGS = PlayerJumpKitFlags.DashEnded | PlayerJumpKitFlags.BurnoutEntered;

		private readonly PlayerId _playerId;
		private readonly PlayerMovementRuntime _runtime;
		private readonly PlayerCameraController _camera;
		private readonly IPlayerFlagService _flags;
		private readonly IDisposable _jumpKitStatusChanged;

		private Vector2 _moveInput;
		private bool _isDisposed;

		public PlayerMovementActionController(
			PlayerId playerId,
			PlayerMovementRuntime runtime,
			PlayerCameraController camera,
			IPlayerFlagService flags,
			IGameEventRegistryService eventFactory
		)
		{
			_playerId = playerId;
			_runtime = runtime ?? throw new ArgumentNullException( nameof( runtime ) );
			_camera = camera ?? throw new ArgumentNullException( nameof( camera ) );
			_flags = flags;

			_jumpKitStatusChanged = eventFactory
				.GetEvent<PlayerJumpKitStatusChangedEventArgs>(
					PlayerJumpKitStatusChangedEventArgs.Name,
					PlayerJumpKitStatusChangedEventArgs.NameSpace,
					EventFlags.NoLock
				)
				.Subscribe( OnJumpKitStatusChanged );
		}

		public void BeginFrame( ref PlayerMovementFrame frame )
		{
			_moveInput = frame.MoveInput;
			frame.SetActionFlags( ReadActionFlags() );
			UpdateSlideCountdown( ref frame );
		}

		public void ProcessGroundActions( ref PlayerMovementFrame frame )
		{
			if ( frame.Input.SlidePressed ) {
				TryStartSlide( ref frame );
			}
		}

		public void Cancel()
		{
			_runtime.Actions.SlideTimeRemaining = 0.0f;
			_flags?.RemoveFlags( PlayerFlags.Sliding );
			_flags?.RemoveFlags( PlayerFlags.Dashing );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_jumpKitStatusChanged.Dispose();
			_isDisposed = true;
			GC.SuppressFinalize( this );
		}

		private PlayerMovementActionFlags ReadActionFlags()
		{
			if ( _flags == null ) {
				return PlayerMovementActionFlags.None;
			}

			PlayerMovementActionFlags actionFlags = PlayerMovementActionFlags.None;
			if ( _flags.GetFlags( PlayerFlags.Dashing ) ) {
				actionFlags |= PlayerMovementActionFlags.Dashing;
			}

			if ( _flags.GetFlags( PlayerFlags.Sliding ) ) {
				actionFlags |= PlayerMovementActionFlags.Sliding;
			}

			return actionFlags;
		}

		private void TryStartSlide( ref PlayerMovementFrame frame )
		{
			if ( frame.IsSliding ) {
				return;
			}

			_runtime.Actions.SetSlideDirection( _camera.ResolveActionDirection( frame.WishDirection ) );
			_runtime.Actions.SlideTimeRemaining = Sdk.Player.Constants.SLIDE_DURATION;
			_flags?.AddFlags( PlayerFlags.Sliding );
			frame.AddActionFlags( PlayerMovementActionFlags.Sliding );
		}

		private void UpdateSlideCountdown( ref PlayerMovementFrame frame )
		{
			if ( _runtime.Actions.SlideTimeRemaining <= 0.0f ) {
				return;
			}

			_runtime.Actions.SlideTimeRemaining -= frame.DeltaTime;
			if ( _runtime.Actions.SlideTimeRemaining > 0.0f ) {
				return;
			}

			_runtime.Actions.SlideTimeRemaining = 0.0f;
			_flags?.RemoveFlags( PlayerFlags.Sliding );
			frame.RemoveActionFlags( PlayerMovementActionFlags.Sliding );
		}

		private void OnJumpKitStatusChanged( in PlayerJumpKitStatusChangedEventArgs args )
		{
			if ( !args.PlayerId.Equals( _playerId ) ) {
				return;
			}

			PlayerJumpKitFlags jumpKitFlags = args.NewStatus.Flags;

			if ( (jumpKitFlags & PlayerJumpKitFlags.DashStarted) != 0 ) {
				Vector3 wishDirection = _camera.GetWishDirection( _moveInput );
				_runtime.Actions.SetDashDirection( _camera.ResolveActionDirection( wishDirection ) );
				_flags?.AddFlags( PlayerFlags.Dashing );
				_runtime.Current.AddActionFlags( PlayerMovementActionFlags.Dashing );
				return;
			}

			if ( (jumpKitFlags & DASH_END_FLAGS) != 0 ) {
				_flags?.RemoveFlags( PlayerFlags.Dashing );
				_runtime.Current.RemoveActionFlags( PlayerMovementActionFlags.Dashing );
			}
		}
	};
};
