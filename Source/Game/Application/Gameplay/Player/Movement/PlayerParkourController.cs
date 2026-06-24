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
using Nomad.Core.Events;
using Nomad.Events.Globals;
using Nomad.EngineUtils;
using Nomad.Game.Application.Gameplay.Player.Movement.Parkour;
using Nomad.Game.Application.Gameplay.Traversal;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Traversal;
using NumericsVector2 = System.Numerics.Vector2;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal sealed class PlayerParkourController : IPlayerParkourController
	{
		public event Action ParkourActivated;
		public event Action ParkourDeactivated;
		public event Action<PlayerParkourState, PlayerParkourState> ParkourStateChanged;
		public event Action<int> ParkourAnchorChanged;

		public PlayerParkourState State => _runtime.State;
		public bool IsActive => _runtime.State != PlayerParkourState.Inactive;
		public int CurrentAnchorIndex => _runtime.CurrentAnchor.AnchorIndex;
		public int CurrentTraversalSourceId => _runtime.CurrentAnchor.SourceId;
		public NumericsVector2 LastMoveInput => _input.Current.Move;

		public float AttachSearchRadius { get => _settings.AttachSearchRadius; set => _settings.AttachSearchRadius = value; }
		public float MinFacingAnchorDot { get => _settings.MinFacingAnchorDot; set => _settings.MinFacingAnchorDot = value; }
		public float MinFacingWallDot { get => _settings.MinFacingWallDot; set => _settings.MinFacingWallDot = value; }
		public float EntryLineStartHeight { get => _settings.EntryLineStartHeight; set => _settings.EntryLineStartHeight = value; }
		public float EntryLineEndHeight { get => _settings.EntryLineEndHeight; set => _settings.EntryLineEndHeight = value; }
		public bool ValidateEntryLineOfSight { get => _settings.ValidateEntryLineOfSight; set => _settings.ValidateEntryLineOfSight = value; }
		public uint ValidationMask { get => _settings.ValidationMask; set => _settings.ValidationMask = value; }

		public float AnchorSnapSpeed { get => _settings.AnchorSnapSpeed; set => _settings.AnchorSnapSpeed = value; }
		public float TurnToWallSpeed { get => _settings.TurnToWallSpeed; set => _settings.TurnToWallSpeed = value; }
		public float EdgeSnapDistance { get => _settings.EdgeSnapDistance; set => _settings.EdgeSnapDistance = value; }
		public float MinimumEdgeDuration { get => _settings.MinimumEdgeDuration; set => _settings.MinimumEdgeDuration = value; }
		public float DefaultEdgeDuration { get => _settings.DefaultEdgeDuration; set => _settings.DefaultEdgeDuration = value; }
		public float EdgeInputDeadZone { get => _settings.EdgeInputDeadZone; set => _settings.EdgeInputDeadZone = value; }
		public float EdgeSelectionMinScore { get => _settings.EdgeSelectionMinScore; set => _settings.EdgeSelectionMinScore = value; }

		public float MantleInputThreshold { get => _settings.MantleInputThreshold; set => _settings.MantleInputThreshold = value; }
		public float MantleUpOffset { get => _settings.MantleUpOffset; set => _settings.MantleUpOffset = value; }
		public float MantleForwardOffset { get => _settings.MantleForwardOffset; set => _settings.MantleForwardOffset = value; }
		public float VaultUpOffset { get => _settings.VaultUpOffset; set => _settings.VaultUpOffset = value; }
		public float VaultForwardOffset { get => _settings.VaultForwardOffset; set => _settings.VaultForwardOffset = value; }
		public float DropPushOffSpeed { get => _settings.DropPushOffSpeed; set => _settings.DropPushOffSpeed = value; }
		public float DropDownSpeed { get => _settings.DropDownSpeed; set => _settings.DropDownSpeed = value; }
		public int InitialQueryBufferCapacity { get => _settings.InitialQueryBufferCapacity; set => _settings.InitialQueryBufferCapacity = value; }

		public bool DriveAnimationTreeStateMachine { get => _animation.DriveAnimationTreeStateMachine; set => _animation.DriveAnimationTreeStateMachine = value; }
		public string AnimationPlaybackPath { get => _animation.AnimationPlaybackPath; set => _animation.AnimationPlaybackPath = value; }
		public string AttachAnimationState { get => _animation.AttachAnimationState; set => _animation.AttachAnimationState = value; }
		public string IdleAnimationState { get => _animation.IdleAnimationState; set => _animation.IdleAnimationState = value; }
		public string ClimbUpAnimationState { get => _animation.ClimbUpAnimationState; set => _animation.ClimbUpAnimationState = value; }
		public string ClimbDownAnimationState { get => _animation.ClimbDownAnimationState; set => _animation.ClimbDownAnimationState = value; }
		public string ShimmyLeftAnimationState { get => _animation.ShimmyLeftAnimationState; set => _animation.ShimmyLeftAnimationState = value; }
		public string ShimmyRightAnimationState { get => _animation.ShimmyRightAnimationState; set => _animation.ShimmyRightAnimationState = value; }
		public string MantleAnimationState { get => _animation.MantleAnimationState; set => _animation.MantleAnimationState = value; }
		public string VaultAnimationState { get => _animation.VaultAnimationState; set => _animation.VaultAnimationState = value; }
		public string DropAnimationState { get => _animation.DropAnimationState; set => _animation.DropAnimationState = value; }

		private readonly PlayerPrefab _prefab;
		private readonly ITraversalGraphProvider _graphProvider;
		private readonly PlayerParkourSettings _settings;
		private readonly PlayerParkourRuntime _runtime;
		private readonly PlayerParkourInputAdapter _input;
		private readonly TraversalQueryBuffer _queryBuffer;
		private readonly PlayerParkourRayValidator _rayValidator;
		private readonly PlayerParkourAttachQuery _attachQuery;
		private readonly PlayerParkourEdgeMotion _edgeMotion;
		private readonly PlayerParkourEdgeSelector _edgeSelector;
		private readonly PlayerParkourEdgeValidator _edgeValidator;
		private readonly PlayerParkourMotor _motor;
		private readonly PlayerParkourAnimationState _animation;
		private readonly ISubscriptionHandle _cameraSubscription;

		private PlayerId _playerId;
		private bool _hasCamera;
		private Vector3 _cameraForward = Vector3.Forward;
		private bool _isDisposed;

		public PlayerParkourController(
			PlayerPrefab prefab,
			ITraversalGraphProvider graphProvider,
			PlayerParkourAnimationState animationState
		)
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_graphProvider = graphProvider ?? throw new ArgumentNullException( nameof( graphProvider ) );
			_animation = animationState ?? new PlayerParkourAnimationState( prefab.GetTree(), prefab.GetNodeOrNull<AnimationTree>( "AnimationTree" ) );

			_settings = new PlayerParkourSettings();
			_runtime = new PlayerParkourRuntime();
			_input = new PlayerParkourInputAdapter();
			_queryBuffer = new TraversalQueryBuffer( Math.Max( _settings.InitialQueryBufferCapacity, 8 ) );
			_rayValidator = new PlayerParkourRayValidator( prefab, _settings );
			_attachQuery = new PlayerParkourAttachQuery( graphProvider, _settings, _queryBuffer, _rayValidator );
			_edgeMotion = new PlayerParkourEdgeMotion( _settings );
			_edgeSelector = new PlayerParkourEdgeSelector( _settings, _edgeMotion );
			_edgeValidator = new PlayerParkourEdgeValidator( _rayValidator, _edgeMotion );
			_motor = new PlayerParkourMotor( prefab, _settings );

			_cameraSubscription = GameEventRegistry.Instance
				.GetEvent<PlayerCameraStatusChangedEventArgs>(
					PlayerCameraStatusChangedEventArgs.Name,
					PlayerCameraStatusChangedEventArgs.NameSpace
				)
				.Subscribe( HandlePlayerCameraStatusChanged );
		}

		public void Dispose()
		{
			if ( _isDisposed ) {
				return;
			}

			_cameraSubscription?.Dispose();
			_animation.Dispose();
			_rayValidator.Dispose();
			_queryBuffer.Dispose();
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public void ApplyInput( in PlayerInputFrame input )
		{
			if ( input.PlayerId.IsValid ) {
				_playerId = input.PlayerId;
			}

			_input.Apply( input );
		}

		public void PhysicsUpdate( float dt )
		{
			switch ( _runtime.State ) {
				case PlayerParkourState.Inactive:
					if ( _input.Current.WantsAttach ) {
						TryAttachToBestAnchor();
					}
					break;

				case PlayerParkourState.Attached:
					PhysicsAttached( dt );
					break;

				case PlayerParkourState.TraversingEdge:
					PhysicsTraversingEdge( dt );
					break;
			}

			_input.ClearFrameRequests();
		}

		public void RequestAttach()
		{
			_input.RequestAttach();
		}

		public void RequestJump()
		{
			_input.RequestJump();
		}

		public void RequestDrop()
		{
			_input.RequestDrop();
		}

		public void ForceDetach( bool preserveVelocity = true )
		{
			if ( !preserveVelocity ) {
				_prefab.Velocity = Vector3.Zero;
			}

			bool wasActive = IsActive;
			_runtime.ClearAnchor();
			SetState( PlayerParkourState.Inactive );

			if ( wasActive ) {
				ParkourDeactivated?.Invoke();
			}
		}

		public void SetTraversalInput( NumericsVector2 moveInput )
		{
			_input.SetTraversalInput( moveInput );
		}

		public void HandlePlayerCameraStatusChanged( in PlayerCameraStatusChangedEventArgs args )
		{
			if ( _playerId.IsValid && args.PlayerId != _playerId ) {
				return;
			}

			_hasCamera = true;
			_cameraForward = PlayerParkourMath.SafeNormalized( args.Forward.ToGodot(), Vector3.Forward );
		}

		public void RebuildRuntimeGraph()
		{
			if ( _runtime.CurrentAnchor.IsValid && !_graphProvider.TryGetGraph( _runtime.CurrentAnchor.SourceId, out _ ) ) {
				ForceDetach();
			}
		}

		private void PhysicsAttached( float dt )
		{
			if ( !TryGetCurrentAnchor( out TraversalGraph graph, out TraversalAnchor anchor ) ) {
				ForceDetach();
				return;
			}

			_prefab.Velocity = Vector3.Zero;
			_motor.SnapBodyToAnchor( anchor, dt );
			_motor.FaceWall( PlayerParkourMath.AnchorNormal( anchor ), dt );

			PlayerTraversalInput input = _input.Current;
			Vector2 moveInput = new Vector2( input.Move.X, input.Move.Y );

			if ( input.WantsDrop || (input.WantsAttach && moveInput.Y < -_settings.MantleInputThreshold) ) {
				DropFromAnchor( anchor );
				return;
			}

			if (
				input.WantsJump &&
				_edgeSelector.TryStartPreferredSpecialEdge( graph, _runtime.CurrentAnchor, TraversalMoveType.Mantle, out int specialEdge ) &&
				_edgeValidator.CanStartEdge( graph, _runtime.CurrentAnchor.AnchorIndex, specialEdge )
			) {
				StartEdge( graph, specialEdge );
				return;
			}

			if (
				_edgeSelector.TrySelectBestEdge( graph, _runtime.CurrentAnchor, moveInput, input.WantsJump, input.WantsDrop, out int edgeIndex ) &&
				_edgeValidator.CanStartEdge( graph, _runtime.CurrentAnchor.AnchorIndex, edgeIndex )
			) {
				StartEdge( graph, edgeIndex );
			}
		}

		private void PhysicsTraversingEdge( float dt )
		{
			if ( !TryGetCurrentGraph( out TraversalGraph graph ) || _runtime.ActiveEdgeIndex < 0 || _runtime.ActiveEdgeIndex >= graph.Edges.Length ) {
				SetState( PlayerParkourState.Attached );
				return;
			}

			ref readonly TraversalEdge edge = ref graph.Edges[_runtime.ActiveEdgeIndex];

			_runtime.EdgeElapsed += dt;
			_motor.TraverseEdge( _runtime, dt );

			float t = Mathf.Clamp( _runtime.EdgeElapsed / Mathf.Max( _runtime.EdgeDuration, _settings.MinimumEdgeDuration ), 0.0f, 1.0f );
			if ( t >= 1.0f || _motor.IsNearEdgeEnd( _runtime ) ) {
				FinishEdge( graph, edge );
			}
		}

		private bool TryAttachToBestAnchor()
		{
			if ( _graphProvider.GraphCount <= 0 ) {
				return false;
			}

			Vector3 bodyPosition = _prefab.GlobalPosition;
			Vector3 facing = GetAttachFacingDirection();

			if ( !_attachQuery.TryFindBestAnchor( bodyPosition, facing, out TraversalAnchorHandle handle ) ) {
				return false;
			}

			AttachToAnchor( handle );
			return true;
		}

		private void AttachToAnchor( TraversalAnchorHandle handle )
		{
			if ( !_graphProvider.TryGetGraph( handle.SourceId, out TraversalGraph graph ) || handle.AnchorIndex < 0 || handle.AnchorIndex >= graph.Anchors.Length ) {
				return;
			}

			ref readonly TraversalAnchor anchor = ref graph.Anchors[handle.AnchorIndex];

			_runtime.CurrentAnchor = handle;
			_runtime.ActiveEdgeIndex = -1;
			_runtime.EdgeElapsed = 0.0f;

			_motor.AttachImmediate( anchor );

			SetState( PlayerParkourState.Attached );
			ParkourActivated?.Invoke();
			ParkourAnchorChanged?.Invoke( handle.AnchorIndex );
			_animation.QueueAttach();
		}

		private void StartEdge( TraversalGraph graph, int edgeIndex )
		{
			ref readonly TraversalEdge edge = ref graph.Edges[edgeIndex];
			ref readonly TraversalAnchor from = ref graph.Anchors[_runtime.CurrentAnchor.AnchorIndex];
			ref readonly TraversalAnchor to = ref graph.Anchors[edge.To];

			_runtime.ActiveEdgeIndex = edgeIndex;
			_runtime.EdgeStart = _prefab.GlobalPosition;
			_runtime.EdgeEnd = _edgeMotion.GetTargetPosition( from, to, edge );
			_runtime.EdgeStartNormal = PlayerParkourMath.AnchorNormal( from );
			_runtime.EdgeEndNormal = PlayerParkourMath.AnchorNormal( to );
			_runtime.EdgeElapsed = 0.0f;
			_runtime.EdgeDuration = Mathf.Max( edge.Duration > 0.0f ? edge.Duration : _settings.DefaultEdgeDuration, _settings.MinimumEdgeDuration );

			SetState( PlayerParkourState.TraversingEdge );
			_animation.QueueMove( edge.MoveType );
		}

		private void FinishEdge( TraversalGraph graph, in TraversalEdge edge )
		{
			if ( (edge.Flags & TraversalEdgeFlags.EndsGrounded) != 0 ) {
				_prefab.GlobalPosition = _runtime.EdgeEnd;
				_prefab.Velocity = Vector3.Zero;
				_runtime.ClearAnchor();
				SetState( PlayerParkourState.Inactive );
				ParkourDeactivated?.Invoke();
				return;
			}

			if ( (edge.Flags & TraversalEdgeFlags.EndsFalling) != 0 ) {
				_runtime.ClearAnchor();
				SetState( PlayerParkourState.Inactive );
				ParkourDeactivated?.Invoke();
				return;
			}

			_runtime.CurrentAnchor = new TraversalAnchorHandle( _runtime.CurrentAnchor.SourceId, edge.To );
			_runtime.ActiveEdgeIndex = -1;
			_runtime.EdgeElapsed = 0.0f;

			if ( edge.To >= 0 && edge.To < graph.Anchors.Length ) {
				ref readonly TraversalAnchor anchor = ref graph.Anchors[edge.To];
				_motor.AttachImmediate( anchor );
				ParkourAnchorChanged?.Invoke( edge.To );
				_animation.QueueIdle();
				SetState( PlayerParkourState.Attached );
			} else {
				_runtime.ClearAnchor();
				SetState( PlayerParkourState.Inactive );
				ParkourDeactivated?.Invoke();
			}
		}

		private void DropFromAnchor( in TraversalAnchor anchor )
		{
			_motor.DropFromAnchor( anchor );
			_runtime.ClearAnchor();
			_animation.QueueDrop();
			SetState( PlayerParkourState.Inactive );
			ParkourDeactivated?.Invoke();
		}

		private bool TryGetCurrentGraph( out TraversalGraph graph )
		{
			if ( !_runtime.CurrentAnchor.IsValid ) {
				graph = null;
				return false;
			}

			return _graphProvider.TryGetGraph( _runtime.CurrentAnchor.SourceId, out graph );
		}

		private bool TryGetCurrentAnchor( out TraversalGraph graph, out TraversalAnchor anchor )
		{
			anchor = default;

			if ( !TryGetCurrentGraph( out graph ) ) {
				return false;
			}

			int anchorIndex = _runtime.CurrentAnchor.AnchorIndex;
			if ( anchorIndex < 0 || anchorIndex >= graph.Anchors.Length ) {
				return false;
			}

			anchor = graph.Anchors[anchorIndex];
			return true;
		}

		private Vector3 GetAttachFacingDirection()
		{
			Vector3 forward = _hasCamera ? _cameraForward : -_prefab.GlobalTransform.Basis.Z;
			forward.Y = 0.0f;
			return PlayerParkourMath.SafeNormalized( forward, -_prefab.GlobalTransform.Basis.Z );
		}

		private void SetState( PlayerParkourState next )
		{
			if ( _runtime.State == next ) {
				return;
			}

			PlayerParkourState old = _runtime.State;
			_runtime.State = next;
			ParkourStateChanged?.Invoke( old, next );
		}
	}
}
