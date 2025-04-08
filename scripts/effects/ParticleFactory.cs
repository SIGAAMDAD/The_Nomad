using Godot;
using System.Collections.Generic;
using System.Threading;

public struct MeshInstance {
	public Transform2D Transform;
	public MeshInstance Prev = null;
	public MeshInstance Next = null;
	public int EndTime = 0;
	public int Id = 0;
	public float SpeedX = 0.0f;
	public float SpeedY = 0.0f;
};

public partial class MultiMeshManager : MultiMeshInstance2D {
	private MeshInstance[] InstanceList = null;
	private MeshInstance FreeInstances = null;
	private MeshInstance ActiveInstances = null;
	private RandomNumberGenerator Random = new RandomNumberGenerator();
	private Mutex LockObject = new Mutex();
	
	private void FreeInstance( MeshInstance instance ) {
		if ( instance.Prev == null ) {
			Console.PrintError( "MultiMeshManager.FreeInstance: not active" );
			return;
		}
		
		// remove from doubly linked list
		instance.Prev.Next = instance.Next;
		instance.Next.Prev = instance.Prev;
		
		// the free list is only singly linked
		instance.Next = FreeInstances;
		FreeInstances = instance;
		
		Multimesh.VisibleInstanceCount--;
	}
	public MeshInstance AllocInstance( Godot.Vector2 position ) {
		LockObject.WaitOne();
		
		if ( FreeInstances == null ) {
			// no free instances, so free the one at the end of the chain
			// remove the oldest active instance
			FreeInstance( ActivesInstances.Prev );
		}
		
		MeshInstance instance = FreeInstances;
		FreeInstances = FreeInstances.Next;
		
		// link into active list
		instance.Next = ActiveInstances.Next;
		instance.Prev = ActiveInstances;
		ActiveInstances.Next.Prev = instance;
		ActiveInstances.Next = instance;
		
		instance.SpeedX = RandomNumberGenerator.RandfRange( -0.25f, 0.25f );
		instance.SpeedY = RandomNumberGenerator.RandfRange( -0.01f, 0.01f );
		
		Multimesh.VisibleInstanceCount++;
		
		LockObject.ReleaseMutex();
		
		return instance;
	}
	private void AllocateInstances() {
		int maxInstances = 0;
		
		switch ( SettingsData.GetParticleAmounts() ) {
		case ParticleAmounts.Low:
			maxInstances = 64;
			break;
		case ParticleAmounts.Standard:
			maxInstances = 1024;
			break;
		case ParticleAmounts.High:
			maxInstances = 2048;
			break;
		case ParticleAmounts.Insane:
			maxInstances = 4096;
			break;
		};
		
		Multimesh.InstanceCount = maxInstances;
		Multimesh.VisibleInstanceCount = 0;
		
		InstanceList = new MeshInstance[ maxInstances ];
	}
	public void InitInstances() {
		AllocateInstances();
		
		ActiveInstances.Next =
		ActiveInstances.Prev =
			ActiveInstances;
		FreeInstances = InstanceList[0];
		
		for ( int i = 0; i < InstanceList.Length; i++ ) {
			InstanceList[i] = new MeshInstance();
			InstanceList[i].Transform = new Transform2D();
			InstanceList[i].Id = i;
		}
		for ( int i = 0; i < InstanceList.Length - 1; i++ ) {
			InstanceList[i].Next = InstanceList[ i + 1 ];
		}
	}
	
	public override void _Ready() {
		base._Ready();
		
		InitInstances();
	}
	public void Process() {
		MeshInstance next = null;
		
		// walk the list backwards, so any new instances generated
		// will be present this frame
		LockObject.WaitOne();
		MeshInstance instance = ActiveInstances.Prev;
		for ( ; instance != ActiveInstances; instance = next ) {
			// grab next now, so if the instance is freed we 
			// still have it
			next = instance.Prev;
			
			int endTime = instance.EndTime;
			if ( Engine.GetProcessFrames() > endTime ) {
				FreeInstance( instance );
				continue;
			}
			
			Godot.Vector2 position = instance.Transform.Origin;
			position.X += instance.SpeedX;
			position.Y += instance.SpeedY;
			instance.Transform.Origin = position;
			
			Multimesh.SetInstanceTransform2D( instance.Id, instance.Transform );
		}
		LockObject.ReleaseMutex();
	}
};

/// <summary>
/// class singleton from which all smoke/debris particles are generated from.
/// since combat is taxing on the CPU & GPU, the renown system will not be run
/// when lead starts flying.
/// only one thread will be used for combat, and other will be used for particle &
/// event processing.
/// </summary>
public partial class ParticleFactory : Node {
	private MultiMeshManager DebrisMesh = null;
	private Thread WorkThread = null;
	private object LockObject = new object();
	
	private static ParticleFactory Instance = null;
	
	private void OnClearInstances() {
		DebrisMesh.InitInstances();
	}
	
	public override void _Ready() {
		base._Ready();
		
		Instance = this;
		
		Player player = GetTree().CurrentScene.GetNode<Player>( "Network/Players/Player" );
		player.Respawn += OnClearInstances;
		
		DebrisMesh = GetNode<MultiMeshInstance2D>( "DebrisMesh" );
		
		WorkThread = new Thread( Process );
		WorkThread.Start();
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcess() % 20 ) != 0 ) {
			return;
		}
		base._Process( delta );
		
		lock ( LockObject ) {
			Monitor.Pulse( LockObject );
		}
	}
	
	private void Process() {
		lock ( LockObject ) {
			Monitor.Wait( LockObject );
		}
		
		DebrisMesh.Process();
	}
	public static void AddDebrisCloud( Godot.Vector2 position ) {
		Instance.DebrisMesh.AllocInstance( position );
	}
};
