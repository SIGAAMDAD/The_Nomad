using System;
using System.Collections.Generic;
using Godot;
using Nomad.Core.Events;
using Nomad.EngineUtils;
using Nomad.Events.Globals;
using Nomad.Game.Application.Gameplay.Traversal;
using Nomad.Game.Prefabs;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Sdk.Multiplayer;
using Nomad.Game.Sdk.Player.Input;
using Nomad.Game.Sdk.Player.Movement;
using Nomad.Game.Sdk.Traversal;

namespace Nomad.Game.Application.Gameplay.Player.Movement
{
	internal sealed class PlayerParkourController : IPlayerParkourController
	{
		public event Action ParkourActivated;
		public event Action ParkourDeactivated;
		public event Action<PlayerParkourState, PlayerParkourState> ParkourStateChanged;
		public event Action<int> ParkourAnchorChanged;

		public PlayerParkourState State => _state;
		public bool IsActive => _state != PlayerParkourState.Inactive;
		public int CurrentAnchorIndex => _currentAnchorIndex;
		public System.Numerics.Vector2 LastMoveInput => _moveInput.ToSystem();

		public float AttachSearchRadius { get; set; } = 1.35f;
		public float MinFacingAnchorDot { get; set; } = 0.05f;
		public float MinFacingWallDot { get; set; } = 0.15f;
		public float EntryLineStartHeight { get; set; } = 1.05f;
		public float EntryLineEndHeight { get; set; } = 0.25f;
		public bool ValidateEntryLineOfSight { get; set; } = true;
		public uint ValidationMask { get; set; } = uint.MaxValue;

		public float AnchorSnapSpeed { get; set; } = 18.0f;
		public float TurnToWallSpeed { get; set; } = 20.0f;
		public float EdgeSnapDistance { get; set; } = 0.04f;
		public float MinimumEdgeDuration { get; set; } = 0.05f;
		public float DefaultEdgeDuration { get; set; } = 0.22f;
		public float EdgeInputDeadZone { get; set; } = 0.20f;
		public float EdgeSelectionMinScore { get; set; } = 0.25f;

		public float MantleInputThreshold { get; set; } = 0.60f;
		public float MantleUpOffset { get; set; } = 1.00f;
		public float MantleForwardOffset { get; set; } = 0.75f;
		public float VaultUpOffset { get; set; } = 0.20f;
		public float VaultForwardOffset { get; set; } = 1.25f;
		public float DropPushOffSpeed { get; set; } = 2.00f;
		public float DropDownSpeed { get; set; } = 2.50f;

		public int InitialQueryBufferCapacity { get; set; } = 96;

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

		private readonly PlayerPrefab _prefab;
		private readonly IPlayerInputSource _inputSource;
		private readonly SceneTree _sceneTree;
		private readonly AnimationTree _animationTree;
		private readonly ISubscriptionHandle _cameraSubscription;

		private TraversalDatabase _database;
		private TraversalGraph _graph;
		private TraversalSpatialIndex _spatialIndex;
		private TraversalQueryBuffer _queryBuffer;

		private PlayerParkourState _state = PlayerParkourState.Inactive;
		private int _currentAnchorIndex = -1;
		private int _activeEdgeIndex = -1;

		private Vector3 _edgeStart;
		private Vector3 _edgeEnd;
		private Vector3 _edgeStartNormal;
		private Vector3 _edgeEndNormal;
		private float _edgeElapsed;
		private float _edgeDuration;

		private Vector2 _moveInput;
		private uint _inputTick;

		private bool _wantsAttach;
		private bool _wantsJump;
		private bool _wantsDrop;

		private bool _hasCamera;
		private Vector3 _cameraForward = Vector3.Forward;

		private readonly PhysicsRayQueryParameters3D _rayQuery = new();
		private readonly Godot.Collections.Array<Rid> _rayExclude = new();

		private string _queuedAnimationState;
		private bool _isDisposed;

		public PlayerParkourController(
			PlayerPrefab prefab,
			IPlayerInputSource inputSource,
			TraversalDatabase database,
			AnimationTree animationTree = null,
			IGameEventRegistryService eventFactory = null
		)
		{
			_prefab = prefab ?? throw new ArgumentNullException( nameof( prefab ) );
			_inputSource = inputSource ?? throw new ArgumentNullException( nameof( inputSource ) );
			_database = database;
			_animationTree = animationTree ?? prefab.GetNodeOrNull<AnimationTree>( "AnimationTree" );
			_sceneTree = prefab.GetTree() ?? throw new InvalidOperationException( "PlayerPrefab must be inside a SceneTree before parkour is initialized." );

			ConfigurePhysicsQuery();
			RebuildRuntimeGraph();

			_sceneTree.PhysicsFrame += OnPhysicsFrame;
			_sceneTree.ProcessFrame += OnProcessFrame;

			eventFactory ??= GameEventRegistry.Instance;
			_cameraSubscription = eventFactory
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

			_sceneTree.PhysicsFrame -= OnPhysicsFrame;
			_sceneTree.ProcessFrame -= OnProcessFrame;
			_cameraSubscription?.Dispose();
			_rayQuery.Dispose();
			GC.SuppressFinalize( this );
			_isDisposed = true;
		}

		public void SetTraversalDatabase( TraversalDatabase database )
		{
			_database = database;
			RebuildRuntimeGraph();
		}

		public void RebuildRuntimeGraph()
		{
			if ( _database == null ) {
				_graph = null;
				_spatialIndex = null;
				_queryBuffer = null;
				ForceDetach();
				return;
			}

			_graph = TraversalGraph.FromDatabase( _database );
			_spatialIndex = _graph.SpatialIndex;
			_queryBuffer = new TraversalQueryBuffer( Math.Max( InitialQueryBufferCapacity, 8 ) );
		}

		public void RequestAttach()
		{
			_wantsAttach = true;
		}

		public void RequestJump()
		{
			_wantsJump = true;
		}

		public void RequestDrop()
		{
			_wantsDrop = true;
		}

		public void ForceDetach( bool preserveVelocity = true )
		{
			if ( !preserveVelocity ) {
				_prefab.Velocity = Vector3.Zero;
			}

			_currentAnchorIndex = -1;
			_activeEdgeIndex = -1;
			SetState( PlayerParkourState.Inactive );
			ParkourDeactivated?.Invoke();
		}

		public void SetTraversalInput( System.Numerics.Vector2 moveInput )
		{
			_moveInput = moveInput.ToGodot();
		}

		public void HandlePlayerCameraStatusChanged( in PlayerCameraStatusChangedEventArgs args )
		{
			if ( args.PlayerId != _inputSource.PlayerId ) {
				return;
			}

			_hasCamera = true;
			_cameraForward = SafeNormalized( args.Forward.ToGodot(), Vector3.Forward );
		}

		private void OnPhysicsFrame()
		{
			ReadInputFrame();
			PhysicsUpdate( (float)_prefab.GetPhysicsProcessDeltaTime() );
			ClearOneFrameInput();
		}

		private void OnProcessFrame()
		{
			if ( _queuedAnimationState == null ) {
				return;
			}

			string stateName = _queuedAnimationState;
			_queuedAnimationState = null;
			TravelAnimation( stateName );
		}

		private void ReadInputFrame()
		{
			PlayerInputFrame input = _inputSource.IsEnabled
				? _inputSource.ReadFrame( _inputTick++ )
				: PlayerInputFrame.Empty;

			_moveInput = input.Move.ToGodot();

			if ( input.InteractPressed ) {
				_wantsAttach = true;
			}

			if ( input.JumpPressed || input.DashPressed ) {
				_wantsJump = true;
			}

			if ( input.DropPressed ) {
				_wantsDrop = true;
			}
		}

		private void PhysicsUpdate( float dt )
		{
			if ( _graph == null || _prefab == null ) {
				return;
			}

			switch ( _state ) {
				case PlayerParkourState.Inactive:
					if ( _wantsAttach ) {
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
		}

		private void PhysicsAttached( float dt )
		{
			if ( !IsValidAnchorIndex( _currentAnchorIndex ) ) {
				ForceDetach();
				return;
			}

			ref readonly TraversalAnchor anchor = ref _graph.Anchors[_currentAnchorIndex];

			_prefab.Velocity = Vector3.Zero;
			SnapBodyToAnchor( anchor, dt );
			FaceWall( AnchorNormal( anchor ), dt );

			if ( _wantsDrop || (_wantsAttach && _moveInput.Y < -MantleInputThreshold) ) {
				DropFromAnchor( anchor );
				return;
			}

			if ( _wantsJump && TryStartPreferredSpecialEdge( TraversalMoveType.Mantle ) ) {
				return;
			}

			if ( TrySelectBestEdge( _moveInput, out int edgeIndex ) && CanStartEdge( edgeIndex ) ) {
				StartEdge( edgeIndex );
			}
		}

		private void PhysicsTraversingEdge( float dt )
		{
			if ( _activeEdgeIndex < 0 || _activeEdgeIndex >= _graph.Edges.Length ) {
				SetState( PlayerParkourState.Attached );
				return;
			}

			ref readonly TraversalEdge edge = ref _graph.Edges[_activeEdgeIndex];

			_edgeElapsed += dt;
			float t = Mathf.Clamp( _edgeElapsed / Mathf.Max( _edgeDuration, MinimumEdgeDuration ), 0.0f, 1.0f );
			float eased = SmoothStep( t );

			Vector3 targetPosition = _edgeStart.Lerp( _edgeEnd, eased );
			Vector3 targetNormal = SafeNormalized( _edgeStartNormal.Lerp( _edgeEndNormal, eased ), _edgeStartNormal );

			Vector3 delta = targetPosition - _prefab.GlobalPosition;
			_prefab.Velocity = delta / Mathf.Max( dt, 0.0001f );
			_prefab.MoveAndSlide();
			FaceWall( targetNormal, dt );

			if ( t >= 1.0f || _prefab.GlobalPosition.DistanceSquaredTo( _edgeEnd ) <= EdgeSnapDistance * EdgeSnapDistance ) {
				FinishEdge( edge );
			}
		}

		private bool TryAttachToBestAnchor()
		{
			if ( _spatialIndex == null || _queryBuffer == null ) {
				return false;
			}

			Vector3 bodyPosition = _prefab.GlobalPosition;
			Vector3 facing = GetAttachFacingDirection();
			_spatialIndex.QuerySphereNonAlloc( bodyPosition.ToSystem(), AttachSearchRadius, _queryBuffer );

			int bestAnchor = -1;
			float bestScore = float.NegativeInfinity;

			for ( int i = 0; i < _queryBuffer.Count; i++ ) {
				int anchorIndex = _queryBuffer.Items[i];
				ref readonly TraversalAnchor anchor = ref _graph.Anchors[anchorIndex];

				if ( (anchor.Flags & TraversalAnchorFlags.EntryAllowed) == 0 ) {
					continue;
				}

				Vector3 anchorPosition = AnchorPosition( anchor );
				Vector3 toAnchor = anchorPosition - bodyPosition;
				float distSq = toAnchor.LengthSquared();

				if ( distSq < 0.0001f ) {
					continue;
				}

				Vector3 toAnchorDir = toAnchor / Mathf.Sqrt( distSq );
				float facingAnchorDot = facing.Dot( toAnchorDir );
				if ( facingAnchorDot < MinFacingAnchorDot ) {
					continue;
				}

				float facingWallDot = facing.Dot( -AnchorNormal( anchor ) );
				if ( facingWallDot < MinFacingWallDot ) {
					continue;
				}

				if ( ValidateEntryLineOfSight && !ValidateEntryLineOfSightToAnchor( anchor ) ) {
					continue;
				}

				float score = facingAnchorDot * 1.15f + facingWallDot * 1.85f - distSq * 0.05f;

				if ( score > bestScore ) {
					bestScore = score;
					bestAnchor = anchorIndex;
				}
			}

			if ( bestAnchor < 0 ) {
				return false;
			}

			AttachToAnchor( bestAnchor );
			return true;
		}

		private void AttachToAnchor( int anchorIndex )
		{
			ref readonly TraversalAnchor anchor = ref _graph.Anchors[anchorIndex];

			_currentAnchorIndex = anchorIndex;
			_activeEdgeIndex = -1;
			_edgeElapsed = 0.0f;

			_prefab.GlobalPosition = AnchorPosition( anchor );
			_prefab.Velocity = Vector3.Zero;
			FaceWallImmediate( AnchorNormal( anchor ) );

			SetState( PlayerParkourState.Attached );
			ParkourActivated?.Invoke();
			ParkourAnchorChanged?.Invoke( anchorIndex );
			QueueAnimation( AttachAnimationState );
		}

		private bool TryStartPreferredSpecialEdge( TraversalMoveType moveType )
		{
			if ( !IsValidAnchorIndex( _currentAnchorIndex ) ) {
				return false;
			}

			ref readonly TraversalAnchor from = ref _graph.Anchors[_currentAnchorIndex];
			int start = from.FirstEdge;
			int end = start + from.EdgeCount;

			for ( int i = start; i < end; i++ ) {
				ref readonly TraversalEdge edge = ref _graph.Edges[i];

				if ( edge.MoveType == moveType && CanStartEdge( i ) ) {
					StartEdge( i );
					return true;
				}
			}

			return false;
		}

		private bool TrySelectBestEdge( Vector2 input, out int edgeIndex )
		{
			edgeIndex = -1;

			if ( !IsValidAnchorIndex( _currentAnchorIndex ) ) {
				return false;
			}

			if ( input.LengthSquared() < EdgeInputDeadZone * EdgeInputDeadZone ) {
				return false;
			}

			ref readonly TraversalAnchor from = ref _graph.Anchors[_currentAnchorIndex];
			Vector3 desired = GetAnchorRight( from ) * input.X + AnchorUp( from ) * input.Y;

			if ( desired.LengthSquared() < 0.0001f ) {
				return false;
			}

			desired = desired.Normalized();

			float bestScore = EdgeSelectionMinScore;
			int start = from.FirstEdge;
			int end = start + from.EdgeCount;

			for ( int i = start; i < end; i++ ) {
				ref readonly TraversalEdge edge = ref _graph.Edges[i];

				if ( !IsValidAnchorIndex( edge.To ) ) {
					continue;
				}

				if ( edge.MoveType == TraversalMoveType.Mantle && input.Y < MantleInputThreshold && !_wantsJump ) {
					continue;
				}

				if ( edge.MoveType == TraversalMoveType.Drop && !_wantsDrop && input.Y > -MantleInputThreshold ) {
					continue;
				}

				ref readonly TraversalAnchor to = ref _graph.Anchors[edge.To];
				Vector3 moveDirection = GetEdgeDirection( from, to, edge.MoveType );
				float score = desired.Dot( moveDirection ) + GetMoveTypeBias( edge.MoveType, input ) - edge.Cost * 0.04f;

				if ( score > bestScore ) {
					bestScore = score;
					edgeIndex = i;
				}
			}

			return edgeIndex >= 0;
		}

		private bool CanStartEdge( int edgeIndex )
		{
			if ( edgeIndex < 0 || edgeIndex >= _graph.Edges.Length ) {
				return false;
			}

			ref readonly TraversalEdge edge = ref _graph.Edges[edgeIndex];

			if ( !IsValidAnchorIndex( _currentAnchorIndex ) || !IsValidAnchorIndex( edge.To ) ) {
				return false;
			}

			ref readonly TraversalAnchor from = ref _graph.Anchors[_currentAnchorIndex];
			ref readonly TraversalAnchor to = ref _graph.Anchors[edge.To];

			if ( (edge.Flags & TraversalEdgeFlags.RequiresLineOfSight) != 0 ) {
				if ( !ValidateLine( AnchorPosition( from ) + AnchorUp( from ) * 0.5f, AnchorPosition( to ) + AnchorUp( to ) * 0.5f ) ) {
					return false;
				}
			}

			if ( (edge.Flags & TraversalEdgeFlags.RequiresClearance) != 0 ) {
				Vector3 target = GetEdgeTargetPosition( from, to, edge );

				if ( !ValidateLine( AnchorPosition( from ) + AnchorUp( from ) * 0.9f, target + AnchorUp( to ) * 0.9f ) ) {
					return false;
				}
			}

			return true;
		}

		private void StartEdge( int edgeIndex )
		{
			ref readonly TraversalEdge edge = ref _graph.Edges[edgeIndex];
			ref readonly TraversalAnchor from = ref _graph.Anchors[_currentAnchorIndex];
			ref readonly TraversalAnchor to = ref _graph.Anchors[edge.To];

			_activeEdgeIndex = edgeIndex;
			_edgeStart = _prefab.GlobalPosition;
			_edgeEnd = GetEdgeTargetPosition( from, to, edge );
			_edgeStartNormal = AnchorNormal( from );
			_edgeEndNormal = AnchorNormal( to );
			_edgeElapsed = 0.0f;
			_edgeDuration = Mathf.Max( edge.Duration > 0.0f ? edge.Duration : DefaultEdgeDuration, MinimumEdgeDuration );

			SetState( PlayerParkourState.TraversingEdge );
			QueueAnimation( GetAnimationStateForMove( edge.MoveType ) );
		}

		private void FinishEdge( in TraversalEdge edge )
		{
			if ( (edge.Flags & TraversalEdgeFlags.EndsGrounded) != 0 ) {
				_prefab.GlobalPosition = _edgeEnd;
				_prefab.Velocity = Vector3.Zero;
				_currentAnchorIndex = -1;
				_activeEdgeIndex = -1;
				SetState( PlayerParkourState.Inactive );
				ParkourDeactivated?.Invoke();
				return;
			}

			if ( (edge.Flags & TraversalEdgeFlags.EndsFalling) != 0 ) {
				_currentAnchorIndex = -1;
				_activeEdgeIndex = -1;
				SetState( PlayerParkourState.Inactive );
				ParkourDeactivated?.Invoke();
				return;
			}

			_currentAnchorIndex = edge.To;
			_activeEdgeIndex = -1;
			_edgeElapsed = 0.0f;

			if ( IsValidAnchorIndex( _currentAnchorIndex ) ) {
				ref readonly TraversalAnchor anchor = ref _graph.Anchors[_currentAnchorIndex];
				_prefab.GlobalPosition = AnchorPosition( anchor );
				_prefab.Velocity = Vector3.Zero;
				FaceWallImmediate( AnchorNormal( anchor ) );
				ParkourAnchorChanged?.Invoke( _currentAnchorIndex );
				QueueAnimation( IdleAnimationState );
				SetState( PlayerParkourState.Attached );
			} else {
				SetState( PlayerParkourState.Inactive );
				ParkourDeactivated?.Invoke();
			}
		}

		private void DropFromAnchor( in TraversalAnchor anchor )
		{
			_prefab.Velocity = AnchorNormal( anchor ) * DropPushOffSpeed + Vector3.Down * DropDownSpeed;
			_currentAnchorIndex = -1;
			_activeEdgeIndex = -1;
			QueueAnimation( DropAnimationState );
			SetState( PlayerParkourState.Inactive );
			ParkourDeactivated?.Invoke();
		}

		private Vector3 GetEdgeTargetPosition( in TraversalAnchor from, in TraversalAnchor to, in TraversalEdge edge )
		{
			return edge.MoveType switch {
				TraversalMoveType.Mantle => AnchorPosition( from ) + AnchorUp( from ) * MantleUpOffset - AnchorNormal( from ) * MantleForwardOffset,
				TraversalMoveType.Vault => AnchorPosition( from ) + AnchorUp( from ) * VaultUpOffset - AnchorNormal( from ) * VaultForwardOffset,
				TraversalMoveType.Drop => AnchorPosition( from ) - AnchorUp( from ) * 1.0f + AnchorNormal( from ) * 0.25f,
				_ => AnchorPosition( to ),
			};
		}

		private static Vector3 GetEdgeDirection( in TraversalAnchor from, in TraversalAnchor to, TraversalMoveType moveType )
		{
			Vector3 raw = AnchorPosition( to ) - AnchorPosition( from );

			if ( raw.LengthSquared() > 0.0001f ) {
				return raw.Normalized();
			}

			Vector3 right = GetAnchorRight( from );

			return moveType switch {
				TraversalMoveType.ClimbUp => AnchorUp( from ),
				TraversalMoveType.ClimbDown => -AnchorUp( from ),
				TraversalMoveType.ShimmyLeft => -right,
				TraversalMoveType.ShimmyRight => right,
				TraversalMoveType.CornerLeft => -right,
				TraversalMoveType.CornerRight => right,
				TraversalMoveType.Mantle => SafeNormalized( AnchorUp( from ) - AnchorNormal( from ), AnchorUp( from ) ),
				TraversalMoveType.Vault => -AnchorNormal( from ),
				TraversalMoveType.Drop => -AnchorUp( from ),
				TraversalMoveType.JumpAcross => -AnchorNormal( from ),
				_ => Vector3.Zero,
			};
		}

		private float GetMoveTypeBias( TraversalMoveType moveType, Vector2 input )
		{
			return moveType switch {
				TraversalMoveType.ClimbUp when input.Y > 0.5f => 0.25f,
				TraversalMoveType.ClimbDown when input.Y < -0.5f => 0.25f,
				TraversalMoveType.ShimmyLeft when input.X < -0.5f => 0.20f,
				TraversalMoveType.ShimmyRight when input.X > 0.5f => 0.20f,
				TraversalMoveType.Mantle when input.Y > MantleInputThreshold => 0.35f,
				TraversalMoveType.Drop when input.Y < -MantleInputThreshold => 0.25f,
				_ => 0.0f,
			};
		}

		private void SnapBodyToAnchor( in TraversalAnchor anchor, float dt )
		{
			float alpha = Mathf.Clamp( AnchorSnapSpeed * dt, 0.0f, 1.0f );
			_prefab.GlobalPosition = _prefab.GlobalPosition.Lerp( AnchorPosition( anchor ), alpha );
		}

		private void FaceWall( Vector3 wallNormal, float dt )
		{
			Vector3 forward = -wallNormal;
			forward.Y = 0.0f;

			if ( forward.LengthSquared() < 0.0001f ) {
				return;
			}

			forward = forward.Normalized();
			Vector3 currentForward = -_prefab.GlobalTransform.Basis.Z;
			currentForward.Y = 0.0f;

			if ( currentForward.LengthSquared() < 0.0001f ) {
				FaceWallImmediate( wallNormal );
				return;
			}

			currentForward = currentForward.Normalized();
			float alpha = Mathf.Clamp( TurnToWallSpeed * dt, 0.0f, 1.0f );
			Vector3 blended = currentForward.Slerp( forward, alpha ).Normalized();

			_prefab.LookAt( _prefab.GlobalPosition + blended, Vector3.Up );
		}

		private void FaceWallImmediate( Vector3 wallNormal )
		{
			Vector3 forward = -wallNormal;
			forward.Y = 0.0f;

			if ( forward.LengthSquared() < 0.0001f ) {
				return;
			}

			_prefab.LookAt( _prefab.GlobalPosition + forward.Normalized(), Vector3.Up );
		}

		private bool ValidateEntryLineOfSightToAnchor( in TraversalAnchor anchor )
		{
			Vector3 from = _prefab.GlobalPosition + Vector3.Up * EntryLineStartHeight;
			Vector3 to = AnchorPosition( anchor ) + AnchorUp( anchor ) * EntryLineEndHeight;
			return ValidateLine( from, to );
		}

		private bool ValidateLine( Vector3 from, Vector3 to )
		{
			_rayQuery.From = from;
			_rayQuery.To = to;
			_rayQuery.CollisionMask = ValidationMask;
			_rayQuery.Exclude = _rayExclude;

			Godot.Collections.Dictionary hit = _prefab.GetWorld3D().DirectSpaceState.IntersectRay( _rayQuery );
			return hit.Count == 0;
		}

		private Vector3 GetAttachFacingDirection()
		{
			Vector3 forward = _hasCamera ? _cameraForward : -_prefab.GlobalTransform.Basis.Z;
			forward.Y = 0.0f;
			return SafeNormalized( forward, -_prefab.GlobalTransform.Basis.Z );
		}

		private void ConfigurePhysicsQuery()
		{
			_rayExclude.Clear();
			_rayExclude.Add( _prefab.GetRid() );

			_rayQuery.CollideWithAreas = false;
			_rayQuery.CollideWithBodies = true;
			_rayQuery.CollisionMask = ValidationMask;
			_rayQuery.Exclude = _rayExclude;
		}

		private void QueueAnimation( string stateName )
		{
			_queuedAnimationState = stateName;
		}

		private void TravelAnimation( string stateName )
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

		private bool IsValidAnchorIndex( int index )
		{
			return _graph != null && index >= 0 && index < _graph.Anchors.Length;
		}

		private void SetState( PlayerParkourState next )
		{
			if ( _state == next ) {
				return;
			}

			PlayerParkourState old = _state;
			_state = next;
			ParkourStateChanged?.Invoke( old, next );
		}

		private void ClearOneFrameInput()
		{
			_wantsAttach = false;
			_wantsJump = false;
			_wantsDrop = false;
		}

		private static Vector3 GetAnchorRight( in TraversalAnchor anchor )
		{
			Vector3 right = AnchorUp( anchor ).Cross( AnchorNormal( anchor ) );

			if ( right.LengthSquared() < 0.0001f ) {
				return Vector3.Right;
			}

			return right.Normalized();
		}

		private static Vector3 AnchorPosition( in TraversalAnchor anchor )
		{
			return anchor.Position.ToGodot();
		}

		private static Vector3 AnchorNormal( in TraversalAnchor anchor )
		{
			return anchor.Normal.ToGodot();
		}

		private static Vector3 AnchorUp( in TraversalAnchor anchor )
		{
			return anchor.Up.ToGodot();
		}

		private static Vector3 SafeNormalized( Vector3 value, Vector3 fallback )
		{
			float lenSq = value.LengthSquared();

			if ( lenSq < 0.0001f ) {
				return fallback;
			}

			return value / Mathf.Sqrt( lenSq );
		}

		private static float SmoothStep( float t )
		{
			t = Mathf.Clamp( t, 0.0f, 1.0f );
			return t * t * (3.0f - 2.0f * t);
		}
	}
}
