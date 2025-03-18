using System.Threading;
using Godot;

namespace Renown.World {
	public partial class WorldArea : Node2D {
		[Export]
		private StringName AreaName;
		[Export]
		private StringName PathName;

		private Area2D LoadArea;
		private Area2D ProcessArea;

		private Thread LoadThread;
		private PackedScene LoadedScene;
		private Node2D SceneData;

		private void OnSceneLoad() {
			LoadedScene = ResourceLoader.Load<PackedScene>( "res://levels/areas/" + PathName + ".tscn" );
			SceneData = LoadedScene.Instantiate<Node2D>();
			CallDeferred( "add_child", SceneData );
		}
	
		private void OnLoadAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is not Player ) {
				return;
			}
			LoadThread.Start();
		}
		private void OnLoadAreaBodyShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			RemoveChild( SceneData );
			SceneData.QueueFree();
			LoadedScene.Free();
		}
		private void OnProcessAreaBodyShape2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			LoadThread.Join();
			SceneData.SetProcess( true );
			SceneData.SetProcessInternal( true );
			SceneData.SetPhysicsProcess( true );
			SceneData.SetPhysicsProcessInternal( true );
		}
		private void OnProcessAreaBodyShape2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			SceneData.SetProcess( false );
			SceneData.SetProcessInternal( false );
			SceneData.SetPhysicsProcess( false );
			SceneData.SetPhysicsProcessInternal( false );
		}

		public override void _Ready() {	
			base._Ready();

			LoadThread = new Thread( OnSceneLoad );

			LoadArea = GetNode<Area2D>( "LoadArea" );
			LoadArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnLoadAreaBodyShape2DEntered ) );
			LoadArea.Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnLoadAreaBodyShape2DExited ) );

			ProcessArea = GetNode<Area2D>( "ProcessArea" );
			ProcessArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnProcessAreaBodyShape2DEntered ) );
			ProcessArea.Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnProcessAreaBodyShape2DExited ) );
		}
    };
};