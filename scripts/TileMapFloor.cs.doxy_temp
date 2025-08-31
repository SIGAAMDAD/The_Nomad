using Godot;
using Renown.World;

public partial class TileMapFloor : Node2D {
	[Export]
	private StringName AreaName;
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

	public StringName GetAreaName() => AreaName;
	public bool IsInside() => !IsExterior;
	public bool GetPlayerStatus() => IsPlayerHere;
	public TileMapFloor GetUpper() => UpperLayer;
	public TileMapFloor GetLower() => LowerLayer;

	private void OnArea2DBodyEntered( Node2D body ) {
		if ( body is not Player ) {
			return;
		}

		if ( Floor != null ) {
			( Floor.Material as ShaderMaterial )?.SetShaderParameter( "alpha_blend", false );
		}
		if ( UpperLayer != null ) {
			if ( IsExterior && !UpperLayer.IsPlayerHere ) {
				UpperLayer.CallDeferred( MethodName.Hide );
			} else {
				UpperLayer.CallDeferred( MethodName.Show );
			}
		}
		if ( LowerLayer != null ) {
			if ( IsExterior ) {
				LowerLayer.CallDeferred( MethodName.Hide );
			} else {
				LowerLayer.CallDeferred( MethodName.Show );
			}
		}

		if ( IsExterior ) {
			for ( int i = 0; i < InteriorLayers.Length; i++ ) {
				InteriorLayers[ i ].CallDeferred( MethodName.Hide );
			}
		} else {
			SetDeferred( PropertyName.ProcessMode, (long)ProcessModeEnum.Pausable );
		}

		CallDeferred( MethodName.Show );

		//		FloorBounds?.SetDeferred( "collision_layer", (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity ) );
		//		FloorBounds?.SetDeferred( "collision_mask", (uint)( PhysicsLayer.Player | PhysicsLayer.SpriteEntity ) );

		IsPlayerHere = true;
	}
	private void OnArea2DBodyExited( Node2D body ) {
		if ( body is not Player ) {
			return;
		}

		( body as Player ).SetTileMapFloor( null );

		if ( !IsExterior && UpperLayer == null ) {
			CallDeferred( MethodName.Hide );
		}

		if ( UpperLayer != null && !IsPlayerHere ) {
			UpperLayer.CallDeferred( MethodName.Hide );
		} else if ( UpperLayer != null && !UpperLayer.IsExterior ) {
			if ( Floor != null ) {
				( Floor.Material as ShaderMaterial )?.SetDeferred( "shader_parameter/alpha_blend", true );
			}
		} else if ( IsExterior && UpperLayer != null ) {
			for ( int i = 0; i < InteriorLayers.Length; i++ ) {
				InteriorLayers[ i ].CallDeferred( MethodName.Hide );
			}
		}
		if ( LowerLayer != null ) {
			if ( IsExterior ) {
				LowerLayer.CallDeferred( MethodName.Hide );
			}
		}

		//		FloorBounds?.SetDeferred( "collision_layer", (uint)PhysicsLayer.None );
		//		FloorBounds?.SetDeferred( "collision_mask", (uint)PhysicsLayer.None );
		IsPlayerHere = false;
	}

	public override void _Ready() {
		base._Ready();

		Area?.Connect( Area2D.SignalName.BodyEntered, Callable.From<Node2D>( OnArea2DBodyEntered ) );
		Area?.Connect( Area2D.SignalName.BodyExited, Callable.From<Node2D>( OnArea2DBodyExited ) );

		ProcessMode = ProcessModeEnum.Disabled;
		ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
		ProcessThreadGroupOrder = 4;

		SetProcess( false );
		SetProcessInternal( false );
	}
};
