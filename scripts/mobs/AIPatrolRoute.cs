using Godot;

public partial class AIPatrolRoute : Node2D {
	[Export]
	private NavigationLink2D[] LinkNodes;
	
	private int CurrentLink = 0;
	
	public override void _Ready() {
		base._Ready();
		
		if ( LinkNodes.Length == 0 ) {
			GD.PushError( "Empty AIPatrolRoute!" );
			return;
		}
		
		SetProcess( false );
		SetProcessInternal( false );
		SetPhysicsProcess( false );
		SetPhysicsProcessInternal( false );
	}
	public NavigationLink2D GetNextPath( bool bReached, bool bPanic ) {
		if ( bPanic ) {
			return LinkNodes[ 0 ];
		}
		if ( !bReached ) {
			return LinkNodes[ CurrentLink ];
		}
		
		if ( CurrentLink == LinkNodes.Length - 1 ) {
			CurrentLink = 0;
		} else {
			CurrentLink++;
		}
		return LinkNodes[ CurrentLink ];
	}
};