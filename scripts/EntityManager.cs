using Godot;
using Renown;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class EntityManager : Node {
	private List<Entity> PhysicsProcessCache;
	private List<Entity> ProcessCache;
	private HashSet<Entity> ProcessList;
	private HashSet<Entity> PhysicsProcessList;
	private static EntityManager Instance;

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

		PhysicsProcessCache = new List<Entity>();
		ProcessCache = new List<Entity>();

		PhysicsProcessList = new HashSet<Entity>();
		ProcessList = new HashSet<Entity>();

		Instance = this;
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