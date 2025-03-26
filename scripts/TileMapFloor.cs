using System;
using Godot;

public partial class TileMapFloor : Node2D {
	[Export]
	private TileMapLayer Floor;
	[Export]
	private TileMapLayer Decor;
	[Export]
	private Area2D Area;

	/// <summary>
	/// only in use if this is the exterior
	/// </summary>
	[Export]
	private TileMapFloor[] InteriorLayers = null;
	[Export]
	private TileMapFloor UpperLayer;
	[Export]
	private TileMapFloor LowerLayer;

	[Export]
	private bool IsExterior = false;

	private bool IsPlayerHere = false;

	private void OnArea2DBodyShapeEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

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

		IsPlayerHere = true;
	}
	private void OnArea2DBodyShapeExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
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

		IsPlayerHere = false;
	}

	public override void _Ready() {
		base._Ready();

		Area.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnArea2DBodyShapeEntered ) );
		Area.Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnArea2DBodyShapeExited ) );

		ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
		ProcessThreadGroupOrder = 4;
	}
};
