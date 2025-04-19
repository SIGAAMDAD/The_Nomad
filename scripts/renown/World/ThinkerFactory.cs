using Godot;
using System.Collections.Generic;

namespace Renown.World {
	public partial class ThinkerFactory : Node {
		private static List<long> Threads = new List<long>();

		private static void CreateThinker( Settlement location ) {
			Thinker thinker = Thinker.Create( location );
		}
		public static void QueueThinker( Settlement location ) {
			Threads.Add( WorkerThreadPool.AddTask( Callable.From( () => { CreateThinker( location ); } ) ) );
		}
		public static void WaitForFinished() {
			for ( int i = 0; i < Threads.Count; i++ ) {
				WorkerThreadPool.WaitForTaskCompletion( Threads[i] );
			}
			Threads.Clear();
		}
	};
};