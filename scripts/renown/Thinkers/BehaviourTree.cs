using Godot;

namespace Renown.Thinkers {
	public enum ProcessType {
		Idle,
		Physics
	};
	public enum BehaviourStatus {
		Success,
		Failure,
		Running
	};

	public partial class BehaviourTree {
		private ProcessType ProcessType;
		private Node Actor;
		private Resource Blackboard;
		private bool Verbose;
		private bool Active;
		private BehaviourStatus CurrentStatus;
		private Node EntryPoint;

		public BehaviourTree( Node tree ) {
			ProcessType = (ProcessType)tree.Get( "process_type" ).AsUInt32();
			Actor = (Node)tree.Get( "actor" );
			CurrentStatus = (BehaviourStatus)tree.Get( "current_status" ).AsUInt32();
			Verbose = tree.Get( "verbose" ).AsBool();
		}
	};
};