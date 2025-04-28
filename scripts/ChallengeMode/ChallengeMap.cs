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
		[Export]
		public StringName QuestResource {
			get;
			private set;
		}

		[Signal]
		public delegate void FinishedLoadingEventHandler();

		private System.Threading.Thread LoadThread;
		private PackedScene MapData;
		private Resource Quest;

		public void Load() {
			LoadThread = new System.Threading.Thread( () => {
				string dir = string.Format( "res://resources/challenge_maps/" );
				MapData = ResourceLoader.Load<PackedScene>( "res://resources/challenge_maps/map_" + MapName );
				Quest = ResourceLoader.Load( "res://resources/challenge_maps/objectives/challenge" + ChallengeIndex + ".tres" );
				CallDeferred( "emit_signal", "FinishedLoading" );
			} );
			LoadThread.Start();
		}
	};
};