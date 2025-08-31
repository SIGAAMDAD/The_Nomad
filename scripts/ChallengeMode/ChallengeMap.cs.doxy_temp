using Godot;

namespace ChallengeMode {
	public partial class ChallengeMap : Resource {
		[Export]
		public int ChallengeIndex {
			get;
			private set;
		}
		[Export]
		public StringName MapName {
			get;
			private set;
		}

		[Signal]
		public delegate void FinishedLoadingEventHandler();

		private System.Threading.Thread LoadThread;
		private PackedScene MapData;
		private Resource Quest;
		private System.Action<PackedScene, Resource> FinishedLoadingDelegate;

		private void OnFinishedLoading() {
			LoadThread.Join();

			FinishedLoading -= OnFinishedLoading;
			FinishedLoadingDelegate( MapData, Quest );
		}
		public void Load( System.Action<PackedScene, Resource> finishedLoading ) {
			FinishedLoading += OnFinishedLoading;
			FinishedLoadingDelegate = finishedLoading;
			LoadThread = new System.Threading.Thread( () => {
				string dir = string.Format( "res://resources/challenge_maps/" );
				MapData = ResourceLoader.Load<PackedScene>( dir + "map_" + MapName + ".tscn" );
				Quest = ResourceLoader.Load( dir + "objectives/challenge" + ChallengeIndex + ".tres" );
				CallDeferred( "emit_signal", "FinishedLoading" );
			} );
			LoadThread.Start();
		}
	};
};