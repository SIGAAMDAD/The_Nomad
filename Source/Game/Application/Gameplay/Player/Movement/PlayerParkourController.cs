using Godot;
using System;
using System.Collections.Generic;
using Nomad.Game.Sdk.Traversal;
using Nomad.Game.Sdk.Events.Player.Movement;
using Nomad.Game.Prefabs;

namespace Nomad.Game.Player.Movement;

/// <summary>
/// Event-driven parkour/traversal motor that consumes PlayerLocomotionCueEventArgs and
/// PlayerCameraStatusChangedEventArgs, then drives a CharacterBody3D along a baked
/// Game.Traversal traversal graph.
///
/// Runtime dependency expectations from the traversal system:
/// - TraversalDatabase
/// - TraversalGraph.FromDatabase(TraversalDatabase)
/// - TraversalGraph.Anchors
/// - TraversalGraph.Edges
/// - TraversalAnchor fields: Position, Normal, Up, Flags, FirstEdge, EdgeCount
/// - TraversalEdge fields: To, MoveType, Flags, Cost, Duration, AnimationId
/// - TraversalAnchorFlags, TraversalEdgeFlags, TraversalMoveType
///
/// The controller intentionally does not poll input. Your movement/camera controllers
/// feed it by calling HandlePlayerLocomotionCue(...) and HandlePlayerCameraStatusChanged(...).
/// </summary>
internal partial class PlayerParkourController
{
	public enum ParkourState : byte
	{
		Inactive,
		Attached,
		TraversingEdge,
	}

	[Signal]
	public delegate void ParkourActivatedEventHandler();

	[Signal]
	public delegate void ParkourDeactivatedEventHandler();

	[Signal]
	public delegate void ParkourStateChangedEventHandler( string oldState, string newState );

	[Signal]
	public delegate void ParkourAnchorChangedEventHandler( int anchorIndex );

	[ExportGroup( "References" )]
	[Export] public CharacterBody3D Body { get; set; }
	[Export] public TraversalDatabase TraversalDatabase { get; set; }
	[Export] public AnimationTree AnimationTree { get; set; }

	[ExportGroup( "Event Filtering" )]
	[Export] public bool LockToFirstPlayerId { get; set; } = true;

	[ExportGroup( "Cue Names" )]
	[Export] public string AttachCueNames { get; set; } = "Climb,Parkour,Interact";
	[Export] public string JumpCueNames { get; set; } = "Jump";
	[Export] public string DropCueNames { get; set; } = "Drop,Cancel";

	[ExportGroup( "Entry" )]
	[Export] public float AttachSearchRadius { get; set; } = 1.35f;
	[Export] public float MinFacingAnchorDot { get; set; } = 0.05f;
	[Export] public float MinFacingWallDot { get; set; } = 0.15f;
	[Export] public float EntryLineStartHeight { get; set; } = 1.05f;
	[Export] public float EntryLineEndHeight { get; set; } = 0.25f;
	[Export] public bool ValidateEntryLineOfSight { get; set; } = true;
	[Export( PropertyHint.Layers3DPhysics )] public uint ValidationMask { get; set; } = uint.MaxValue;

	[ExportGroup( "Attached Motion" )]
	[Export] public float AnchorSnapSpeed { get; set; } = 18.0f;
	[Export] public float TurnToWallSpeed { get; set; } = 20.0f;
	[Export] public float EdgeSnapDistance { get; set; } = 0.04f;
	[Export] public float MinimumEdgeDuration { get; set; } = 0.05f;
	[Export] public float DefaultEdgeDuration { get; set; } = 0.22f;
	[Export] public float EdgeInputDeadZone { get; set; } = 0.20f;
	[Export] public float EdgeSelectionMinScore { get; set; } = 0.25f;

	[ExportGroup( "Special Moves" )]
	[Export] public float MantleInputThreshold { get; set; } = 0.60f;
	[Export] public float MantleUpOffset { get; set; } = 1.00f;
	[Export] public float MantleForwardOffset { get; set; } = 0.75f;
	[Export] public float VaultUpOffset { get; set; } = 0.20f;
	[Export] public float VaultForwardOffset { get; set; } = 1.25f;
	[Export] public float DropPushOffSpeed { get; set; } = 2.00f;
	[Export] public float DropDownSpeed { get; set; } = 2.50f;

	[ExportGroup( "Spatial Query" )]
	[Export] public float InternalSpatialCellSize { get; set; } = 2.0f;
	[Export] public int InitialQueryBufferCapacity { get; set; } = 96;

	[ExportGroup( "Animation" )]
	[Export] public bool DriveAnimationTreeStateMachine { get; set; } = false;
	[Export] public string AnimationPlaybackPath { get; set; } = "parameters/playback";
	[Export] public string AttachAnimationState { get; set; } = "WallAttach";
	[Export] public string IdleAnimationState { get; set; } = "TraversalIdle";
	[Export] public string ClimbUpAnimationState { get; set; } = "ClimbUp";
	[Export] public string ClimbDownAnimationState { get; set; } = "ClimbDown";
	[Export] public string ShimmyLeftAnimationState { get; set; } = "ShimmyLeft";
	[Export] public string ShimmyRightAnimationState { get; set; } = "ShimmyRight";
	[Export] public string MantleAnimationState { get; set; } = "Mantle";
	[Export] public string VaultAnimationState { get; set; } = "Vault";
	[Export] public string DropAnimationState { get; set; } = "Drop";

	public ParkourState State => _state;
	public bool IsActive => _state != ParkourState.Inactive;
	public int CurrentAnchorIndex => _currentAnchorIndex;
	public Vector2 LastMoveInput => _moveInput;
	public Vector3 LastCameraForward => _cameraForward;
	public Vector3 LastCameraRight => _cameraRight;

	private ParkourState _state = ParkourState.Inactive;

	private TraversalGraph _graph;
	private LocalSpatialIndex _spatialIndex;
	private IntQueryBuffer _queryBuffer;

	private int _currentAnchorIndex = -1;
	private int _activeEdgeIndex = -1;

	private Vector3 _edgeStart;
	private Vector3 _edgeEnd;
	private Vector3 _edgeStartNormal;
	private Vector3 _edgeEndNormal;
	private float _edgeElapsed;
	private float _edgeDuration;

	private Vector2 _moveInput;
	private bool _isMoving;
	private Vector3 _lastLocomotionOrigin;
	private uint _lastServerTick;

	private bool _wantsAttach;
	private bool _wantsJump;
	private bool _wantsDrop;

	private bool _hasCamera;
	private Vector3 _cameraOrigin;
	private Vector3 _cameraForward = Vector3.Forward;
	private Vector3 _cameraRight = Vector3.Right;
	private float _cameraPitch;
	private float _cameraYaw;
	private float _cameraRoll;

	private bool _hasLockedPlayerId;
	private PlayerId _lockedPlayerId;

	private readonly HashSet<string> _attachCues = new( StringComparer.OrdinalIgnoreCase );
	private readonly HashSet<string> _jumpCues = new( StringComparer.OrdinalIgnoreCase );
	private readonly HashSet<string> _dropCues = new( StringComparer.OrdinalIgnoreCase );

	private readonly PhysicsRayQueryParameters3D _rayQuery = new();
	private Godot.Collections.Array<Rid> _rayExclude;

	public override void _Ready()
	{
		RebuildCueSets();
		RebuildRuntimeGraph();
		ConfigurePhysicsQuery();
	}

	public override void _PhysicsProcess( double delta )
	{
		float dt = (float)delta;

		if ( Body == null || _graph == null ) {
			ClearOneFrameCueLatches();
			return;
		}

		switch ( _state ) {
			case ParkourState.Inactive:
				PhysicsInactive( dt );
				break;

			case ParkourState.Attached:
				PhysicsAttached( dt );
				break;

			case ParkourState.TraversingEdge:
				PhysicsTraversingEdge( dt );
				break;
		}

		ClearOneFrameCueLatches();
	}

	/// <summary>
	/// Call this from your locomotion event subscription.
	/// The name can be changed if your event system requires a different handler name.
	/// </summary>
	public void HandlePlayerLocomotionCue( in PlayerLocomotionCueEventArgs args )
	{
		OnLocomotionCue( args );
	}

	/// <summary>
	/// Same signature as requested. Kept private here to match your example, but the public wrapper
	/// above is usually easier for external subscription systems.
	/// </summary>
	private void OnLocomotionCue( in PlayerLocomotionCueEventArgs args )
	{
		if ( !AcceptPlayer( args.PlayerId ) ) {
			return;
		}

		_moveInput = args.MoveInput;
		_isMoving = args.IsMoving;
		_lastLocomotionOrigin = args.Origin;
		_lastServerTick = args.ServerTick;

		string cueName = args.Cue.ToString();

		if ( _attachCues.Contains( cueName ) ) {
			_wantsAttach = true;
		}

		if ( _jumpCues.Contains( cueName ) ) {
			_wantsJump = true;
		}

		if ( _dropCues.Contains( cueName ) ) {
			_wantsDrop = true;
		}
	}

	/// <summary>
	/// Call this from your camera status event subscription.
	/// </summary>
	public void HandlePlayerCameraStatusChanged( in PlayerCameraStatusChangedEventArgs args )
	{
		if ( !AcceptPlayer( args.PlayerId ) ) {
			return;
		}

		_hasCamera = true;
		_cameraOrigin = args.Origin;
		_cameraForward = SafeNormalized( args.Forward, Vector3.Forward );
		_cameraRight = SafeNormalized( args.Right, Vector3.Right );
		_cameraPitch = args.Pitch;
		_cameraYaw = args.Yaw;
		_cameraRoll = args.Roll;
	}

	public void RebuildRuntimeGraph()
	{
		if ( TraversalDatabase == null ) {
			_graph = null;
			_spatialIndex = null;
			return;
		}

		_graph = TraversalGraph.FromDatabase( TraversalDatabase );
		_spatialIndex = new LocalSpatialIndex( _graph.Anchors, Mathf.Max( InternalSpatialCellSize, 0.25f ) );
		_queryBuffer = new IntQueryBuffer( Mathf.Max( InitialQueryBufferCapacity, 8 ) );
	}

	public void ForceDetach( bool preserveVelocity = true )
	{
		if ( !preserveVelocity && Body != null ) {
			Body.Velocity = Vector3.Zero;
		}

		SetState( ParkourState.Inactive );
		_currentAnchorIndex = -1;
		_activeEdgeIndex = -1;
		EmitSignal( SignalName.ParkourDeactivated );
	}

	private void ConfigurePhysicsQuery()
	{
		_rayExclude = new Godot.Collections.Array<Rid>();

		if ( Body != null ) {
			_rayExclude.Add( Body.GetRid() );
		}

		_rayQuery.CollideWithAreas = false;
		_rayQuery.CollideWithBodies = true;
		_rayQuery.CollisionMask = ValidationMask;
		_rayQuery.Exclude = _rayExclude;
	}

	private void PhysicsInactive( float dt )
	{
		if ( _wantsAttach ) {
			TryAttachToBestAnchor();
		}
	}

	private void PhysicsAttached( float dt )
	{
		if ( !IsValidAnchorIndex( _currentAnchorIndex ) ) {
			ForceDetach();
			return;
		}

		ref readonly TraversalAnchor anchor = ref _graph.Anchors[_currentAnchorIndex];

		Body.Velocity = Vector3.Zero;
		SnapBodyToAnchor( anchor, dt );
		FaceWall( anchor.Normal, dt );

		if ( _wantsDrop ) {
			DropFromAnchor( anchor );
			return;
		}

		if ( _wantsJump && TryStartPreferredSpecialEdge( TraversalMoveType.Mantle ) ) {
			return;
		}

		if ( TrySelectBestEdge( _moveInput, out int edgeIndex ) ) {
			if ( CanStartEdge( edgeIndex ) ) {
				StartEdge( edgeIndex );
			}
		}
	}

	private void PhysicsTraversingEdge( float dt )
	{
		if ( _activeEdgeIndex < 0 || _activeEdgeIndex >= _graph.Edges.Length ) {
			SetState( ParkourState.Attached );
			return;
		}

		ref readonly TraversalEdge edge = ref _graph.Edges[_activeEdgeIndex];

		_edgeElapsed += dt;

		float t = Mathf.Clamp( _edgeElapsed / Mathf.Max( _edgeDuration, MinimumEdgeDuration ), 0.0f, 1.0f );
		float eased = SmoothStep( t );

		Vector3 targetPosition = _edgeStart.Lerp( _edgeEnd, eased );
		Vector3 targetNormal = SafeNormalized( _edgeStartNormal.Lerp( _edgeEndNormal, eased ), _edgeStartNormal );

		Vector3 delta = targetPosition - Body.GlobalPosition;
		Body.Velocity = delta / Mathf.Max( dt, 0.0001f );
		Body.MoveAndSlide();

		FaceWall( targetNormal, dt );

		if ( t >= 1.0f || Body.GlobalPosition.DistanceSquaredTo( _edgeEnd ) <= EdgeSnapDistance * EdgeSnapDistance ) {
			FinishEdge( edge );
		}
	}

	private bool TryAttachToBestAnchor()
	{
		if ( _graph == null || _spatialIndex == null || _queryBuffer == null ) {
			return false;
		}

		Vector3 bodyPosition = Body.GlobalPosition;
		Vector3 facing = GetAttachFacingDirection();

		_spatialIndex.QuerySphereNonAlloc( bodyPosition, AttachSearchRadius, _queryBuffer );

		int bestAnchor = -1;
		float bestScore = float.NegativeInfinity;

		for ( int i = 0; i < _queryBuffer.Count; i++ ) {
			int anchorIndex = _queryBuffer.Items[i];
			ref readonly TraversalAnchor anchor = ref _graph.Anchors[anchorIndex];

			if ( (anchor.Flags & TraversalAnchorFlags.EntryAllowed) == 0 ) {
				continue;
			}

			Vector3 toAnchor = anchor.Position - bodyPosition;
			float distSq = toAnchor.LengthSquared();

			if ( distSq < 0.0001f ) {
				continue;
			}

			Vector3 toAnchorDir = toAnchor / Mathf.Sqrt( distSq );

			float facingAnchorDot = facing.Dot( toAnchorDir );
			if ( facingAnchorDot < MinFacingAnchorDot ) {
				continue;
			}

			float facingWallDot = facing.Dot( -anchor.Normal );
			if ( facingWallDot < MinFacingWallDot ) {
				continue;
			}

			if ( ValidateEntryLineOfSight && !ValidateLineOfSightToAnchor( anchor ) ) {
				continue;
			}

			float score =
				facingAnchorDot * 1.15f +
				facingWallDot * 1.85f -
				distSq * 0.05f;

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

		Body.GlobalPosition = anchor.Position;
		Body.Velocity = Vector3.Zero;
		FaceWallImmediate( anchor.Normal );

		SetState( ParkourState.Attached );
		EmitSignal( SignalName.ParkourActivated );
		EmitSignal( SignalName.ParkourAnchorChanged, anchorIndex );

		TravelAnimation( AttachAnimationState );
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

			if ( edge.MoveType != moveType ) {
				continue;
			}

			if ( !CanStartEdge( i ) ) {
				continue;
			}

			StartEdge( i );
			return true;
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

		Vector3 right = GetAnchorRight( from );
		Vector3 desired = right * input.X + from.Up * input.Y;

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
			float directionScore = desired.Dot( moveDirection );
			float typeBias = GetMoveTypeBias( edge.MoveType, input );
			float costPenalty = edge.Cost * 0.04f;

			float score = directionScore + typeBias - costPenalty;

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
			if ( !ValidateLine( from.Position + from.Up * 0.5f, to.Position + to.Up * 0.5f ) ) {
				return false;
			}
		}

		if ( (edge.Flags & TraversalEdgeFlags.RequiresClearance) != 0 ) {
			Vector3 target = GetEdgeTargetPosition( from, to, edge );

			if ( !ValidateLine( from.Position + from.Up * 0.9f, target + to.Up * 0.9f ) ) {
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
		_edgeStart = Body.GlobalPosition;
		_edgeEnd = GetEdgeTargetPosition( from, to, edge );
		_edgeStartNormal = from.Normal;
		_edgeEndNormal = to.Normal;
		_edgeElapsed = 0.0f;
		_edgeDuration = Mathf.Max( edge.Duration > 0.0f ? edge.Duration : DefaultEdgeDuration, MinimumEdgeDuration );

		SetState( ParkourState.TraversingEdge );
		TravelAnimation( GetAnimationStateForMove( edge.MoveType ) );
	}

	private void FinishEdge( in TraversalEdge edge )
	{
		if ( (edge.Flags & TraversalEdgeFlags.EndsGrounded) != 0 ) {
			Body.GlobalPosition = _edgeEnd;
			Body.Velocity = Vector3.Zero;
			_currentAnchorIndex = -1;
			_activeEdgeIndex = -1;
			SetState( ParkourState.Inactive );
			EmitSignal( SignalName.ParkourDeactivated );
			return;
		}

		if ( (edge.Flags & TraversalEdgeFlags.EndsFalling) != 0 ) {
			_currentAnchorIndex = -1;
			_activeEdgeIndex = -1;
			SetState( ParkourState.Inactive );
			EmitSignal( SignalName.ParkourDeactivated );
			return;
		}

		_currentAnchorIndex = edge.To;
		_activeEdgeIndex = -1;
		_edgeElapsed = 0.0f;

		if ( IsValidAnchorIndex( _currentAnchorIndex ) ) {
			ref readonly TraversalAnchor anchor = ref _graph.Anchors[_currentAnchorIndex];
			Body.GlobalPosition = anchor.Position;
			Body.Velocity = Vector3.Zero;
			FaceWallImmediate( anchor.Normal );
			EmitSignal( SignalName.ParkourAnchorChanged, _currentAnchorIndex );
			TravelAnimation( IdleAnimationState );
			SetState( ParkourState.Attached );
		} else {
			SetState( ParkourState.Inactive );
			EmitSignal( SignalName.ParkourDeactivated );
		}
	}

	private void DropFromAnchor( in TraversalAnchor anchor )
	{
		Body.Velocity = anchor.Normal * DropPushOffSpeed + Vector3.Down * DropDownSpeed;

		_currentAnchorIndex = -1;
		_activeEdgeIndex = -1;
		TravelAnimation( DropAnimationState );
		SetState( ParkourState.Inactive );
		EmitSignal( SignalName.ParkourDeactivated );
	}

	private Vector3 GetEdgeTargetPosition( in TraversalAnchor from, in TraversalAnchor to, in TraversalEdge edge )
	{
		return edge.MoveType switch {
			TraversalMoveType.Mantle => from.Position + from.Up * MantleUpOffset - from.Normal * MantleForwardOffset,
			TraversalMoveType.Vault => from.Position + from.Up * VaultUpOffset - from.Normal * VaultForwardOffset,
			TraversalMoveType.Drop => from.Position - from.Up * 1.0f + from.Normal * 0.25f,
			_ => to.Position,
		};
	}

	private Vector3 GetEdgeDirection( in TraversalAnchor from, in TraversalAnchor to, TraversalMoveType moveType )
	{
		Vector3 raw = to.Position - from.Position;

		if ( raw.LengthSquared() > 0.0001f ) {
			return raw.Normalized();
		}

		Vector3 right = GetAnchorRight( from );

		return moveType switch {
			TraversalMoveType.ClimbUp => from.Up,
			TraversalMoveType.ClimbDown => -from.Up,
			TraversalMoveType.ShimmyLeft => -right,
			TraversalMoveType.ShimmyRight => right,
			TraversalMoveType.CornerLeft => -right,
			TraversalMoveType.CornerRight => right,
			TraversalMoveType.Mantle => SafeNormalized( from.Up - from.Normal, from.Up ),
			TraversalMoveType.Vault => -from.Normal,
			TraversalMoveType.Drop => -from.Up,
			TraversalMoveType.JumpAcross => -from.Normal,
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
		Body.GlobalPosition = Body.GlobalPosition.Lerp( anchor.Position, alpha );
	}

	private void FaceWall( Vector3 wallNormal, float dt )
	{
		Vector3 forward = -wallNormal;
		forward.Y = 0.0f;

		if ( forward.LengthSquared() < 0.0001f ) {
			return;
		}

		forward = forward.Normalized();

		Vector3 currentForward = -Body.GlobalTransform.Basis.Z;
		currentForward.Y = 0.0f;

		if ( currentForward.LengthSquared() < 0.0001f ) {
			FaceWallImmediate( wallNormal );
			return;
		}

		currentForward = currentForward.Normalized();

		float alpha = Mathf.Clamp( TurnToWallSpeed * dt, 0.0f, 1.0f );
		Vector3 blended = currentForward.Slerp( forward, alpha ).Normalized();

		Body.LookAt( Body.GlobalPosition + blended, Vector3.Up );
	}

	private void FaceWallImmediate( Vector3 wallNormal )
	{
		Vector3 forward = -wallNormal;
		forward.Y = 0.0f;

		if ( forward.LengthSquared() < 0.0001f ) {
			return;
		}

		Body.LookAt( Body.GlobalPosition + forward.Normalized(), Vector3.Up );
	}

	private bool ValidateEntryLineOfSightToAnchor( in TraversalAnchor anchor )
	{
		Vector3 from = Body.GlobalPosition + Vector3.Up * EntryLineStartHeight;
		Vector3 to = anchor.Position + anchor.Up * EntryLineEndHeight;
		return ValidateLine( from, to );
	}

	private bool ValidateLine( Vector3 from, Vector3 to )
	{
		if ( Body == null ) {
			return false;
		}

		_rayQuery.From = from;
		_rayQuery.To = to;
		_rayQuery.CollisionMask = ValidationMask;
		_rayQuery.Exclude = _rayExclude;

		Godot.Collections.Dictionary hit = Body.GetWorld3D().DirectSpaceState.IntersectRay( _rayQuery );
		return hit.Count == 0;
	}

	private Vector3 GetAttachFacingDirection()
	{
		Vector3 forward = _hasCamera ? _cameraForward : -Body.GlobalTransform.Basis.Z;
		forward.Y = 0.0f;
		return SafeNormalized( forward, -Body.GlobalTransform.Basis.Z );
	}

	private static Vector3 GetAnchorRight( in TraversalAnchor anchor )
	{
		Vector3 right = anchor.Up.Cross( anchor.Normal );

		if ( right.LengthSquared() < 0.0001f ) {
			return Vector3.Right;
		}

		return right.Normalized();
	}

	private bool IsValidAnchorIndex( int index )
	{
		return _graph != null && index >= 0 && index < _graph.Anchors.Length;
	}

	private bool AcceptPlayer( PlayerId playerId )
	{
		if ( !LockToFirstPlayerId ) {
			return true;
		}

		if ( !_hasLockedPlayerId ) {
			_lockedPlayerId = playerId;
			_hasLockedPlayerId = true;
			return true;
		}

		return EqualityComparer<PlayerId>.Default.Equals( _lockedPlayerId, playerId );
	}

	private void SetState( ParkourState next )
	{
		if ( _state == next ) {
			return;
		}

		ParkourState old = _state;
		_state = next;
		EmitSignal( SignalName.ParkourStateChanged, old.ToString(), next.ToString() );
	}

	private void RebuildCueSets()
	{
		_attachCues.Clear();
		_jumpCues.Clear();
		_dropCues.Clear();

		AddCueNames( _attachCues, AttachCueNames );
		AddCueNames( _jumpCues, JumpCueNames );
		AddCueNames( _dropCues, DropCueNames );
	}

	private static void AddCueNames( HashSet<string> set, string csv )
	{
		if ( string.IsNullOrWhiteSpace( csv ) ) {
			return;
		}

		string[] parts = csv.Split( ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );

		foreach ( string part in parts ) {
			if ( !string.IsNullOrWhiteSpace( part ) ) {
				set.Add( part );
			}
		}
	}

	private void ClearOneFrameCueLatches()
	{
		_wantsAttach = false;
		_wantsJump = false;
		_wantsDrop = false;
	}

	private void TravelAnimation( string stateName )
	{
		if ( !DriveAnimationTreeStateMachine || AnimationTree == null || string.IsNullOrWhiteSpace( stateName ) ) {
			return;
		}

		Variant playbackVariant = AnimationTree.Get( AnimationPlaybackPath );

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

	private sealed class IntQueryBuffer
	{
		public int[] Items;
		public int Count;

		public IntQueryBuffer( int capacity )
		{
			Items = new int[Mathf.Max( capacity, 8 )];
		}

		public void Clear()
		{
			Count = 0;
		}

		public void Add( int value )
		{
			if ( Count >= Items.Length ) {
				Array.Resize( ref Items, Items.Length * 2 );
			}

			Items[Count++] = value;
		}
	}

	private readonly struct CellKey : IEquatable<CellKey>
	{
		public readonly int X;
		public readonly int Y;
		public readonly int Z;

		public CellKey( int x, int y, int z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		public bool Equals( CellKey other )
		{
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		public override bool Equals( object obj )
		{
			return obj is CellKey other && Equals( other );
		}

		public override int GetHashCode()
		{
			unchecked {
				int hash = 17;
				hash = hash * 31 + X;
				hash = hash * 31 + Y;
				hash = hash * 31 + Z;
				return hash;
			}
		}
	}

	private sealed class LocalSpatialIndex
	{
		private readonly TraversalAnchor[] _anchors;
		private readonly Dictionary<CellKey, int[]> _cells;
		private readonly float _inverseCellSize;

		public LocalSpatialIndex( TraversalAnchor[] anchors, float cellSize )
		{
			_anchors = anchors;
			_inverseCellSize = 1.0f / Mathf.Max( cellSize, 0.25f );

			var build = new Dictionary<CellKey, List<int>>();

			for ( int i = 0; i < anchors.Length; i++ ) {
				CellKey cell = PositionToCell( anchors[i].Position );

				if ( !build.TryGetValue( cell, out List<int> list ) ) {
					list = new List<int>( 8 );
					build[cell] = list;
				}

				list.Add( i );
			}

			_cells = new Dictionary<CellKey, int[]>( build.Count );

			foreach ( (CellKey key, List<int> list) in build ) {
				_cells[key] = list.ToArray();
			}
		}

		public void QuerySphereNonAlloc( Vector3 center, float radius, IntQueryBuffer buffer )
		{
			buffer.Clear();

			float radiusSq = radius * radius;
			CellKey min = PositionToCell( center - Vector3.One * radius );
			CellKey max = PositionToCell( center + Vector3.One * radius );

			for ( int z = min.Z; z <= max.Z; z++ ) {
				for ( int y = min.Y; y <= max.Y; y++ ) {
					for ( int x = min.X; x <= max.X; x++ ) {
						CellKey key = new( x, y, z );

						if ( !_cells.TryGetValue( key, out int[] indices ) ) {
							continue;
						}

						for ( int i = 0; i < indices.Length; i++ ) {
							int anchorIndex = indices[i];

							if ( (_anchors[anchorIndex].Position - center).LengthSquared() <= radiusSq ) {
								buffer.Add( anchorIndex );
							}
						}
					}
				}
			}
		}

		private CellKey PositionToCell( Vector3 position )
		{
			return new CellKey(
				Mathf.FloorToInt( position.X * _inverseCellSize ),
				Mathf.FloorToInt( position.Y * _inverseCellSize ),
				Mathf.FloorToInt( position.Z * _inverseCellSize )
			);
		}
	}
}
