using Godot;

public partial class ThinkerPoolManager : Node {
	private const int MAX_THINKER_GROUPS = 10;
	private System.Collections.Generic.List<ThinkerGroup> Groups = new System.Collections.Generic.List<ThinkerGroup>();
	private int LastGroup = 0;

	private class ThinkerGroup {
		private System.Collections.Generic.List<MobBase> Thinkers;
		private System.Timers.Timer ThinkTimer;
		private Node Parent;

		private void OnThinkTimerTimeout( System.Object source, System.Timers.ElapsedEventArgs e ) {
			for ( int i = 0; i < Thinkers.Count; ++i ) {
				Thinkers[i].Think( (float)Parent.GetProcessDeltaTime() );
			}
		}

		public ThinkerGroup( Node parent, float interval ) {
			Thinkers = new System.Collections.Generic.List<MobBase>();
			ThinkTimer = new System.Timers.Timer( interval );
			ThinkTimer.AutoReset = true;
			ThinkTimer.Enabled = true;
			ThinkTimer.Elapsed += OnThinkTimerTimeout;
			ThinkTimer.Start();

			Parent = parent;
		}

		public void AddThinker( MobBase thinker ) {
			Thinkers.Add( thinker );
		}
		public int GroupSize() {
			return Thinkers.Count;
		}
		public void Clear() {
			Thinkers.Clear();
		}
	};

	public ThinkerPoolManager() {
		float interval = 200.0f;
		for ( int i = 0; i < MAX_THINKER_GROUPS; i++ ) {
			Groups.Add( new ThinkerGroup( this, interval ) );
			interval += 200.0f;
		}

		LastGroup = 0;
	}

	public void AddThinker( MobBase thinker ) {
		if ( LastGroup == Groups.Count ) {
			LastGroup = 0;
		}
		Groups[ LastGroup++ ].AddThinker( thinker );
	}
	public void ClearThinkers() {
		for ( int i = 0; i < Groups.Count; i++ ) {
			Groups[i].Clear();
		}
	}
};