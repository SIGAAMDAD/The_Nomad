using Godot;
using System.Collections.Generic;

/// <summary>
/// class singleton from which all smoke/debris particles are generated from.
/// since combat is taxing on the CPU & GPU, the renown system will not be run
/// when lead starts flying.
/// only one thread will be used for combat, and other will be used for particle &
/// event processing.
/// </summary>
public partial class ParticleFactory : Node {
	private MultiMeshManager DebrisMesh = null;
	
	private static ParticleFactory Instance = null;
	
	private void OnClearInstances() {
		DebrisMesh.InitInstances();
	}
	
	public override void _Ready() {
		base._Ready();
		
		Instance = this;
		
		Player player = GetTree().CurrentScene.GetNode<Player>( "Network/Players/Player0" );
//		player.Respawn += OnClearInstances;
		
		DebrisMesh = GetNode<MultiMeshManager>( "DebrisMesh" );
	
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 20 ) != 0 ) {
			return;
		}
		base._Process( delta );

		DebrisMesh.Process();
	}

	public static void AddDebrisCloud( Godot.Vector2 position ) {
		Instance.DebrisMesh.AllocInstance( position );
	}
};
