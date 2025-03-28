using Godot;
using Renown.World;

public partial class TileMapFloor : Node2D {
	[Export]
	private TileMapLayer Floor;
	[Export]
	private TileMapLayer Decor;
	[Export]
	private Area2D Area;
	[Export]
	private StaticBody2D FloorBounds;
	[Export]
	private TileMapFloor Exterior;
	[Export]
	private WorldArea ParentLocation;
	[Export]
	private int Level = 0; // "floor"

	/// <Summary>
	/// only in use if this is the exterior
	/// </Summary>
	[Export]
	private TileMapFloor[] InteriorLayers = null;
	[Export]
	private TileMapFloor UpperLayer;
	[Export]
	private TileMapFloor LowerLayer;

	[Export]
	private bool IsExterior = false;

	private bool IsPlayerHere = false;

	public bool IsInside() => !IsExterior;
	public bool GetPlayerStatus() => IsPlayerHere;
	public TileMapFloor GetUpper() => UpperLayer;
	public TileMapFloor GetLower() => LowerLayer;

	private void OnArea2DBodyShapeEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			if ( body is NetworkPlayer ) {
			} else if ( body is Renown.Thinker ) {
				( body as Renown.Thinker ).SetTileMapFloor( this );
			}
			return;
		}

		body.CallDeferred( "SetTileMapFloorLevel", Level );

		( Floor.Material as ShaderMaterial )?.SetShaderParameter( "alpha_blend", false );
		if ( UpperLayer != null ) {
			if ( IsExterior && !UpperLayer.IsPlayerHere ) {
				UpperLayer.CallDeferred( "hide" );
			} else {
				UpperLayer.CallDeferred( "show" );
			}
		}
		if ( LowerLayer != null ) {
			if ( IsExterior ) {
				LowerLayer.CallDeferred( "hide" );
			} else {
				LowerLayer.CallDeferred( "show" );
			}
		}

		if ( IsExterior ) {
			for ( int i = 0; i < InteriorLayers.Length; i++ ) {
				InteriorLayers[i].CallDeferred( "hide" );
			}
		}

		CallDeferred( "show" );

		LowerLayer?.Floor.SetDeferred( "collision_enabled", false );

		Floor.SetDeferred( "collision_enabled", true );
		FloorBounds?.SetDeferred( "collision_layer", 1 | 2 );
		FloorBounds?.SetDeferred( "collision_mask", 1 );
		IsPlayerHere = true;
	}
	private void OnArea2DBodyShapeExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			if ( body is NetworkPlayer ) {

			} else if ( body is Renown.Thinker ) {
				( body as Renown.Thinker ).SetTileMapFloor( null );
			}
			return;
		}

		if ( !IsExterior && UpperLayer == null ) {
			CallDeferred( "hide" );
		}

		if ( UpperLayer != null && !IsPlayerHere ) {
			UpperLayer.CallDeferred( "hide" );
		} else if ( UpperLayer != null && !UpperLayer.IsExterior ) {
			( Floor.Material as ShaderMaterial )?.SetShaderParameter( "alpha_blend", true );
		}
		if ( LowerLayer != null ) {
			if ( IsExterior ) {
				LowerLayer.CallDeferred( "hide" );
			}
		}

		Floor.SetDeferred( "collision_enabled", false );
		FloorBounds?.SetDeferred( "collision_layer", 0 );
		FloorBounds?.SetDeferred( "collision_mask", 0 );
		IsPlayerHere = false;
	}

	public override void _Ready() {
		base._Ready();

		Area.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnArea2DBodyShapeEntered ) );
		Area.Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnArea2DBodyShapeExited ) );

		ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
		ProcessThreadGroupOrder = 4;

		SetProcess( false );
		SetProcessInternal( false );
	}
};
