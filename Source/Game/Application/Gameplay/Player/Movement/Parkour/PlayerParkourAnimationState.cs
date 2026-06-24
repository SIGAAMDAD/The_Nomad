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
using Godot;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Application.Gameplay.Player.Movement.Parkour
{
	internal sealed class PlayerParkourAnimationState : IDisposable
	{
		private readonly SceneTree _sceneTree;
		private readonly AnimationTree _animationTree;
		private string _queuedState;
		private bool _isDisposed;

		public bool DriveAnimationTreeStateMachine { get; set; } = true;
		public string AnimationPlaybackPath { get; set; } = "parameters/playback";
		public string AttachAnimationState { get; set; } = "WallAttach";
		public string IdleAnimationState { get; set; } = "TraversalIdle";
		public string ClimbUpAnimationState { get; set; } = "ClimbUp";
		public string ClimbDownAnimationState { get; set; } = "ClimbDown";
		public string ShimmyLeftAnimationState { get; set; } = "ShimmyLeft";
		public string ShimmyRightAnimationState { get; set; } = "ShimmyRight";
		public string MantleAnimationState { get; set; } = "Mantle";
		public string VaultAnimationState { get; set; } = "Vault";
		public string DropAnimationState { get; set; } = "Drop";

		public PlayerParkourAnimationState( SceneTree sceneTree, AnimationTree animationTree )
		{
			_sceneTree = sceneTree;
			_animationTree = animationTree;

			if ( _sceneTree != null ) {
				_sceneTree.ProcessFrame += OnProcessFrame;
			}
		}

		public void QueueAttach()
		{
			Queue( AttachAnimationState );
		}

		public void QueueIdle()
		{
			Queue( IdleAnimationState );
		}

		public void QueueDrop()
		{
			Queue( DropAnimationState );
		}

		public void QueueMove( TraversalMoveType moveType )
		{
			Queue( GetAnimationStateForMove( moveType ) );
		}

		public void Queue( string stateName )
		{
			_queuedState = stateName;
		}

		private void OnProcessFrame()
		{
			if ( _queuedState == null ) {
				return;
			}

			string stateName = _queuedState;
			_queuedState = null;
			Travel( stateName );
		}

		private void Travel( string stateName )
		{
			if ( !DriveAnimationTreeStateMachine || _animationTree == null || string.IsNullOrWhiteSpace( stateName ) ) {
				return;
			}

			Variant playbackVariant = _animationTree.Get( AnimationPlaybackPath );

			if ( playbackVariant.VariantType == Variant.Type.Nil ) {
				return;
			}

			GodotObject playbackObject = playbackVariant.AsGodotObject();

			if ( playbackObject == null ) {
				return;
			}

			playbackObject.Call( "travel", stateName );
		}

		private string GetAnimationStateForMove( TraversalMoveType moveType )
		{
			return moveType switch {
				TraversalMoveType.ClimbUp => ClimbUpAnimationState,
				TraversalMoveType.ClimbDown => ClimbDownAnimationState,
				TraversalMoveType.ShimmyLeft => ShimmyLeftAnimationState,
				TraversalMoveType.ShimmyRight => ShimmyRightAnimationState,
				TraversalMoveType.Mantle => MantleAnimationState,
				TraversalMoveType.Vault => VaultAnimationState,
				TraversalMoveType.Drop => DropAnimationState,
				_ => IdleAnimationState,
			};
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			if ( _sceneTree != null ) {
				_sceneTree.ProcessFrame -= OnProcessFrame;
			}

			GC.SuppressFinalize( this );
			_isDisposed = true;
		}
	}
}
