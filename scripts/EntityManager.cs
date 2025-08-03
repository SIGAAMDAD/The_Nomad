using Godot;
using Renown;
using System.Collections.Generic;

public partial class EntityManager : Node {
	private List<Entity> PhysicsProcessCache;
	private List<Entity> ProcessCache;
	private HashSet<Entity> ProcessList;
	private HashSet<Entity> PhysicsProcessList;
	private static EntityManager Instance;

	// cached constants for runtime ease of access
	private PhysicsDirectSpaceState2D SpaceState2D;

	public static void RegisterPhysicsProcess( Entity node ) {
		Instance.PhysicsProcessCache.Add( node );
		Instance.PhysicsProcessList.Add( node );
	}
	public static void DisablePhysicsProcess( Entity node ) {
		Instance.PhysicsProcessList.Remove( node );
	}
	public static void RegisterProcess( Entity node ) {
		Instance.ProcessCache.Add( node );
		Instance.ProcessList.Add( node );
	}
	public static void DisableProcess( Entity node ) {
		Instance.ProcessList.Remove( node );
	}

	public override void _Ready() {
		base._Ready();

		Instance = this;

		SetProcess( false );
		SetPhysicsProcess( false );
	}
	public static void Init() {
		LevelData.Instance.ExitLevel += () => {
			Instance.PhysicsProcessCache.Clear();
			Instance.PhysicsProcessList.Clear();

			Instance.ProcessCache.Clear();
			Instance.ProcessList.Clear();

			Instance.SetProcess( false );
			Instance.SetPhysicsProcess( false );
		};

		Instance.SpaceState2D = LevelData.Instance.GetWorld2D().DirectSpaceState;

		Instance.PhysicsProcessCache = new List<Entity>();
		Instance.ProcessCache = new List<Entity>();

		Instance.PhysicsProcessList = new HashSet<Entity>();
		Instance.ProcessList = new HashSet<Entity>();

		Instance.SetProcess( true );
		Instance.SetPhysicsProcess( true );
	}
	public override void _Process( double delta ) {
		for ( int i = 0; i < ProcessCache.Count; i++ ) {
			ProcessCache[ i ].Update( delta );
		}
	}
	public override void _PhysicsProcess( double delta ) {
		for ( int i = 0; i < PhysicsProcessCache.Count; i++ ) {
			PhysicsProcessCache[ i ].PhysicsUpdate( delta );
		}
	}
};