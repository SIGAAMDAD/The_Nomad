using Godot;
using Renown.Thinkers;
using Renown.World;

public partial class HellbreakerSpawner : Area2D {
	[Export]
	private int SpitterCount = 0;
	[Export]
	private int ImpCount = 0;

	private void OnAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
	//		for ( int i = 0; i < SpitterCount; i++ ) {
	//			Spitter spitter = new Spitter();
	//			AddChild( spitter );
	//		}
			for ( int i = 0; i < ImpCount; i++ ) {
				Imp imp = ResourceCache.GetScene( "res://scenes/mobs/demons/imp.tscn" ).Instantiate<Imp>();
				imp.SetFaction( GetNode<Faction>( "../../HellFaction" ) );
				Hellbreaker.Demons.Add( imp );
				AddChild( imp );
			}

			SetDeferred( "monitoring", false );
			GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", true );
		}
	}
	public void Clear() {
		for ( int i = 0; i < GetChildCount(); i++ ) {
			if ( GetChild( i ) is Imp thinker && thinker != null ) {
				thinker.ProcessMode = ProcessModeEnum.Disabled;
				CallDeferred( "remove_child", thinker );
			}
		}
	}
	public void Reset() {
		SetDeferred( "monitoring", true );
		GetChild<CollisionShape2D>( 0 ).SetDeferred( "disabled", false );
	}

	public override void _Ready() {
		base._Ready();

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnAreaBodyShape2DEntered ) );
	}
};